using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace AltoLib.Rendering
{
    public class SSR_RenderPass : ScriptableRenderPass
    {
        Material _ssrMaterial;
        RTHandle _ssrRT;
        RTHandle _ssrBlurredRT;
        RTHandle _tempCopy;

        static readonly int ID_Intensity           = Shader.PropertyToID("_SSR_Intensity");
        static readonly int ID_MaxSteps            = Shader.PropertyToID("_SSR_MaxSteps");
        static readonly int ID_StepSize            = Shader.PropertyToID("_SSR_StepSize");
        static readonly int ID_MaxDistance         = Shader.PropertyToID("_SSR_MaxDistance");
        static readonly int ID_Thickness           = Shader.PropertyToID("_SSR_Thickness");
        static readonly int ID_BinarySearchSteps   = Shader.PropertyToID("_SSR_BinarySearchSteps");
        static readonly int ID_EdgeFade            = Shader.PropertyToID("_SSR_EdgeFade");
        static readonly int ID_DistanceFade        = Shader.PropertyToID("_SSR_DistanceFade");
        static readonly int ID_SurfaceFadeDistance = Shader.PropertyToID("_SSR_SurfaceFadeDistance");
        static readonly int ID_SurfaceFadePower    = Shader.PropertyToID("_SSR_SurfaceFadePower");
        static readonly int ID_BlurRadius          = Shader.PropertyToID("_SSR_BlurRadius");
        static readonly int ID_BlurTexelSize       = Shader.PropertyToID("_SSR_BlurTexelSize");
        static readonly int ID_SSRTexture          = Shader.PropertyToID("_SSR_Texture");

        static readonly ProfilingSampler _profilingSampler = new("SSR");

        //----------------------------------------------------------------------
        // RenderGraph pass data
        //----------------------------------------------------------------------

        class CopyPassData
        {
            public TextureHandle source;
        }

        class RayMarchPassData
        {
            public TextureHandle source;
            public Material material;
        }

        class CompositePassData
        {
            public TextureHandle source;
            public Material material;
        }

        class BlurPassData
        {
            public TextureHandle source;
            public Material material;
        }

        //----------------------------------------------------------------------
        // Constructor / Dispose
        //----------------------------------------------------------------------

        public SSR_RenderPass(Material material, RenderPassEvent passEvent)
        {
            _ssrMaterial = material;
            renderPassEvent = passEvent;
            ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal);
        }

        public void Dispose()
        {
            _ssrRT?.Release();
            _ssrBlurredRT?.Release();
            _tempCopy?.Release();
        }

        //--------------------------------------------------------------
        // RenderGraph path (for Unity 6)
        //--------------------------------------------------------------

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var resourceData = frameData.Get<UniversalResourceData>();
            var cameraData   = frameData.Get<UniversalCameraData>();

            if (resourceData.isActiveTargetBackBuffer) { return; }

            var stack = VolumeManager.instance.stack;
            var ssr = stack.GetComponent<SSR_VolumeComponent>();
            if (ssr == null || !ssr.IsActive()) { return; }

            SetMaterialProperties(ssr);

            // SSR のレンダリング解像度を指定
            float scale = ssr.resolutionScale.value;
            var ssrDesc = cameraData.cameraTargetDescriptor;
            ssrDesc.depthBufferBits = 0;
            ssrDesc.msaaSamples = 1;
            ssrDesc.width  = Mathf.Max(1, (int)(ssrDesc.width  * scale));
            ssrDesc.height = Mathf.Max(1, (int)(ssrDesc.height * scale));
            ssrDesc.colorFormat = RenderTextureFormat.ARGBHalf;

            SetBlurTexelSize(ssrDesc.width, ssrDesc.height);

            TextureHandle ssrRT = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph, ssrDesc, "_SSR_RT", false, FilterMode.Bilinear
            );

            TextureHandle ssrBlurredRT = TextureHandle.nullHandle;
            bool useBlur = ssr.smoothness.value > 0.01f;
            if (useBlur)
            {
                ssrBlurredRT = UniversalRenderer.CreateRenderGraphTexture(
                    renderGraph, ssrDesc, "_SSR_BlurredRT", false, FilterMode.Bilinear
                );
            }
            TextureHandle ssrFinal = useBlur ? ssrBlurredRT : ssrRT;

            // SSR の合成パスでシーンの色をサンプリングするための一時的なコピーを作成
            var copyDesc = cameraData.cameraTargetDescriptor;
            copyDesc.depthBufferBits = 0;
            copyDesc.msaaSamples = 1;
            TextureHandle tempCopy = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph, copyDesc, "_SSR_TempCopy", false, FilterMode.Bilinear
            );

            // シーンの色を tempCopy にコピー
            TextureHandle activeColor = resourceData.activeColorTexture;
            using (var builder = renderGraph.AddRasterRenderPass<CopyPassData>("SSR_CopyColor", out var copyData))
            {
                copyData.source = activeColor;
                builder.UseTexture(activeColor, AccessFlags.Read);
                builder.SetRenderAttachment(tempCopy, 0);
                builder.AllowPassCulling(false);

                builder.SetRenderFunc(static (CopyPassData data, RasterGraphContext ctx) =>
                {
                    Blitter.BlitTexture(ctx.cmd, data.source, new Vector4(1, 1, 0, 0), 0, false);
                });
            }

            //----- Pass 0 : レイマーチング
            using (var builder = renderGraph.AddRasterRenderPass<RayMarchPassData>("SSR_RayMarch", out var rmData))
            {
                rmData.source   = activeColor;
                rmData.material = _ssrMaterial;

                builder.UseTexture(activeColor, AccessFlags.Read);
                if (resourceData.cameraDepthTexture.IsValid()) {
                    builder.UseTexture(resourceData.cameraDepthTexture, AccessFlags.Read);
                }
                if (resourceData.cameraNormalsTexture.IsValid()) {
                    builder.UseTexture(resourceData.cameraNormalsTexture, AccessFlags.Read);
                }

                builder.SetRenderAttachment(ssrRT, 0);
                if (!useBlur) { builder.SetGlobalTextureAfterPass(ssrRT, ID_SSRTexture); }
                builder.AllowPassCulling(false);

                builder.SetRenderFunc(static (RayMarchPassData data, RasterGraphContext ctx) =>
                {
                    Blitter.BlitTexture(
                        ctx.cmd, data.source, new Vector4(1, 1, 0, 0), data.material,
                        pass: 0
                    );
                });
            }

            //----- Pass 1 : ぼかし効果
            if (useBlur)
            {
                using (var builder = renderGraph.AddRasterRenderPass<BlurPassData>("SSR_Blur", out var blurData))
                {
                    blurData.source   = ssrRT;
                    blurData.material = _ssrMaterial;

                    builder.UseTexture(ssrRT, AccessFlags.Read);
                    builder.SetRenderAttachment(ssrBlurredRT, 0);
                    builder.SetGlobalTextureAfterPass(ssrBlurredRT, ID_SSRTexture);
                    builder.AllowPassCulling(false);

                    builder.SetRenderFunc(static (BlurPassData data, RasterGraphContext ctx) =>
                    {
                        Blitter.BlitTexture(
                            ctx.cmd, data.source, new Vector4(1, 1, 0, 0), data.material,
                            pass: 1
                        );
                    });
                }
            }

            //----- Pass 2 : SSR 結果の合成
            using (var builder = renderGraph.AddRasterRenderPass<CompositePassData>("SSR_Composite", out var compData))
            {
                compData.source   = tempCopy;
                compData.material = _ssrMaterial;

                builder.UseTexture(tempCopy, AccessFlags.Read);
                builder.UseTexture(ssrFinal, AccessFlags.Read);
                builder.SetRenderAttachment(activeColor, 0);
                builder.AllowPassCulling(false);

                builder.SetRenderFunc(static (CompositePassData data, RasterGraphContext ctx) =>
                {
                    Blitter.BlitTexture(
                        ctx.cmd, data.source, new Vector4(1, 1, 0, 0), data.material,
                        pass: 2
                    );
                });
            }
        }

        //--------------------------------------------------------------
        // Shared
        //--------------------------------------------------------------

        private void SetMaterialProperties(SSR_VolumeComponent ssr)
        {
            _ssrMaterial.SetFloat(ID_Intensity,            ssr.intensity.value);
            _ssrMaterial.SetInt  (ID_MaxSteps,             ssr.maxSteps.value);
            _ssrMaterial.SetFloat(ID_StepSize,             ssr.stepSize.value);
            _ssrMaterial.SetFloat(ID_MaxDistance,          ssr.maxDistance.value);
            _ssrMaterial.SetFloat(ID_Thickness,            ssr.thickness.value);
            _ssrMaterial.SetInt  (ID_BinarySearchSteps,    ssr.binarySearchSteps.value);
            _ssrMaterial.SetFloat(ID_EdgeFade,             ssr.edgeFade.value);
            _ssrMaterial.SetFloat(ID_DistanceFade,         ssr.distanceFade.value);
            _ssrMaterial.SetFloat(ID_SurfaceFadeDistance,  ssr.surfaceFadeDistance.value);
            _ssrMaterial.SetFloat(ID_SurfaceFadePower,     ssr.surfaceFadePower.value);
            _ssrMaterial.SetFloat(ID_BlurRadius,           ssr.smoothness.value);
        }

        private void SetBlurTexelSize(int width, int height)
        {
            _ssrMaterial.SetVector(
                ID_BlurTexelSize,
                new Vector4(1f / width, 1f / height, width, height)
            );
        }
    }
}
