using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AltoLib.Rendering
{
    public class SSR_RendererFeature : ScriptableRendererFeature
    {
        const string ShaderName = "Hidden/AltoShader/ScreenSpaceReflections";

        [System.Serializable]
        public class Settings
        {
            [Tooltip("SSR パスの挿入ポイント")]
            public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        }
        public Settings settings = new();

        SSR_RenderPass _ssrPass;
        Material _ssrMaterial;

        public override void Create()
        {
            var shader = Shader.Find(ShaderName);
            if (shader == null)
            {
                Debug.LogWarning($"[SSR] Shader not found: {ShaderName}");
                return;
            }

            _ssrMaterial = CoreUtils.CreateEngineMaterial(shader);
            _ssrPass = new SSR_RenderPass(_ssrMaterial, settings.renderPassEvent);
        }

        public override void AddRenderPasses(
            ScriptableRenderer renderer, ref RenderingData renderingData
        )
        {
            if (_ssrPass == null) { return; }
            if (renderingData.cameraData.cameraType != CameraType.Game &&
                renderingData.cameraData.cameraType != CameraType.SceneView) { return; }

            var stack = VolumeManager.instance.stack;
            var ssr = stack.GetComponent<SSR_VolumeComponent>();
            if (ssr == null || !ssr.IsActive()) { return; }

            renderer.EnqueuePass(_ssrPass);
        }
    }
}
