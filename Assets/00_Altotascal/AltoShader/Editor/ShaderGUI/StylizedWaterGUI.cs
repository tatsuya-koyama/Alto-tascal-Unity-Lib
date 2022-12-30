using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

namespace AltoLib.ShaderGUI
{
    /// <summary>
    /// Shader GUI for Altotascal/URP 7.3.1/Stylized Water
    /// </summary>
    public class StylizedWaterGUI : SimpleLitGUIBase
    {
        class CustomProperties : ICustomProperties
        {
            public MaterialProperty waterColorDepth;
            public MaterialProperty depthDebug;
            public MaterialProperty foamColor;
            public MaterialProperty foamSharpness;
            public MaterialProperty foamFactor;
            public MaterialProperty underwaterColor;
            public MaterialProperty multiplyUnderwater;
            public MaterialProperty waterDistortion;

            public MaterialProperty waveCycle;
            public MaterialProperty waveSpeed;
            public MaterialProperty wavePower;
            public MaterialProperty riseAndFall;
            public MaterialProperty surfaceSpecular;
            public MaterialProperty surfaceNoise;
            public MaterialProperty surfaceParams;
            public MaterialProperty fixSmoothness;

            public MaterialProperty edgeFadeOutOn;
            public MaterialProperty edgeFadeOutOrigin;
            public MaterialProperty edgeFadeOutDistance;
            public MaterialProperty edgeSharpness;

            public MaterialProperty dissolveAreaSize;
            public MaterialProperty dissolveOrigin;
            public MaterialProperty dissolveSlow;
            public MaterialProperty dissolveDistance;
            public MaterialProperty dissolveRoughness;
            public MaterialProperty dissolveNoise;
            public MaterialProperty dissolveEdgeSharpness;
            public MaterialProperty dissolveEdgeAddColor;
            public MaterialProperty dissolveEdgeSubColor;

            public MaterialProperty ditherPattern;
            public MaterialProperty ditherCull;

            public CustomProperties(MaterialProperty[] properties)
            {
                waterColorDepth       = BaseShaderGUI.FindProperty("_WaterColorDepth", properties);
                depthDebug            = BaseShaderGUI.FindProperty("_DepthDebug", properties);
                foamColor             = BaseShaderGUI.FindProperty("_FoamColor", properties);
                foamSharpness         = BaseShaderGUI.FindProperty("_FoamSharpness", properties);
                foamFactor            = BaseShaderGUI.FindProperty("_FoamFactor", properties);
                underwaterColor       = BaseShaderGUI.FindProperty("_UnderwaterColor", properties);
                multiplyUnderwater    = BaseShaderGUI.FindProperty("_MultiplyUnderwaterColor", properties);
                waterDistortion       = BaseShaderGUI.FindProperty("_WaterDistortion", properties);

                waveCycle             = BaseShaderGUI.FindProperty("_WaveCycle", properties);
                waveSpeed             = BaseShaderGUI.FindProperty("_WaveSpeed", properties);
                wavePower             = BaseShaderGUI.FindProperty("_WavePower", properties);
                riseAndFall           = BaseShaderGUI.FindProperty("_RiseAndFall", properties);
                surfaceSpecular       = BaseShaderGUI.FindProperty("_SurfaceSpecular", properties);
                surfaceNoise          = BaseShaderGUI.FindProperty("_SurfaceNoise", properties);
                surfaceParams         = BaseShaderGUI.FindProperty("_SurfaceParams", properties);
                fixSmoothness         = BaseShaderGUI.FindProperty("_FixSmoothness", properties);

                edgeFadeOutOn         = BaseShaderGUI.FindProperty("_EdgeFadeOutOn", properties);
                edgeFadeOutOrigin     = BaseShaderGUI.FindProperty("_EdgeFadeOutOrigin", properties);
                edgeFadeOutDistance   = BaseShaderGUI.FindProperty("_EdgeFadeOutDistance", properties);
                edgeSharpness         = BaseShaderGUI.FindProperty("_EdgeSharpness", properties);

                dissolveAreaSize      = BaseShaderGUI.FindProperty("_DissolveAreaSize", properties);
                dissolveOrigin        = BaseShaderGUI.FindProperty("_DissolveOrigin", properties);
                dissolveSlow          = BaseShaderGUI.FindProperty("_DissolveSlow", properties);
                dissolveDistance      = BaseShaderGUI.FindProperty("_DissolveDistance", properties);
                dissolveRoughness     = BaseShaderGUI.FindProperty("_DissolveRoughness", properties);
                dissolveNoise         = BaseShaderGUI.FindProperty("_DissolveNoise", properties);
                dissolveEdgeSharpness = BaseShaderGUI.FindProperty("_DissolveEdgeSharpness", properties);
                dissolveEdgeAddColor  = BaseShaderGUI.FindProperty("_DissolveEdgeAddColor", properties);
                dissolveEdgeSubColor  = BaseShaderGUI.FindProperty("_DissolveEdgeSubColor", properties);

                ditherPattern         = BaseShaderGUI.FindProperty("_DitherPattern", properties);
                ditherCull            = BaseShaderGUI.FindProperty("_DitherCull", properties);
            }

            //------------------------------------------------------------------
            // ICustomProperties
            //------------------------------------------------------------------

            public object this[string propertyName]
            {
                get { return typeof(CustomProperties).GetField(propertyName).GetValue(this); }

                set { typeof(CustomProperties).GetField(propertyName).SetValue(this, value); }
            }
        }
        CustomProperties _customProperties;

        ShaderGUIUtil _util;
        bool _showWaterColorProps  = true;
        bool _showSurfaceProps     = true;
        bool _showEdgeFadeOutProps = true;
        bool _showDissolveProps    = true;
        bool _showDitherProps      = true;

        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);
            _customProperties = new CustomProperties(properties);
            _util = new ShaderGUIUtil(_customProperties);
        }

        public override void FillAdditionalFoldouts(MaterialHeaderScopeList materialScopesList)
        {
            DrawDitherProps();
            DrawWaterColorProps();
            DrawSurfaceProps();
            DrawEdgeFadeOutProps();
            DrawDissolveProps();
        }

        void DrawDitherProps()
        {
            _showDitherProps = _util.Foldout(_showDitherProps, "Dithering");
            if (!_showDitherProps) { return; }

            materialEditor.TextureProperty(_customProperties.ditherPattern, "Dithering Pattern");
            _util.DrawSlider("Dithering Cull", "ditherCull", 0f, 20f);
        }

        void DrawWaterColorProps()
        {
            _showWaterColorProps = _util.Foldout(_showWaterColorProps, "Water & Underwater Color");
            if (!_showWaterColorProps) { return; }

            _util.DrawSlider("Color Depth", "waterColorDepth", 0f, 10f);
            _util.DrawToggle("Depth Debug", "depthDebug");
            materialEditor.ColorProperty(_customProperties.foamColor, "Foam Color");
            _util.DrawSlider("Foam Sharpness", "foamSharpness", 0f, 10f);
            _util.DrawSlider("Foam Factor", "foamFactor", -2f, 2f);
            materialEditor.ColorProperty(_customProperties.underwaterColor, "Underwater Color");
            _util.DrawToggle("Multiply Underwater", "multiplyUnderwater");
            _util.DrawSlider("Water Distortion", "waterDistortion", 0f, 10f);
        }

        void DrawSurfaceProps()
        {
            _showSurfaceProps = _util.Foldout(_showSurfaceProps, "Surface Wave");
            if (!_showSurfaceProps) { return; }

            _util.DrawSlider("Wave Cycle", "waveCycle", 0f, 4f);
            _util.DrawSlider("Wave Speed", "waveSpeed", 0f, 10f);
            _util.DrawSlider("Wave Power", "wavePower", 0f, 10f);
            _util.DrawSlider("Rise and Fall", "riseAndFall", 0f, 10f);
            _util.DrawSlider("Surface Specular", "surfaceSpecular", 0f, 10f);
            _util.DrawSlider("Surface Noise", "surfaceNoise", 0f, 10f);
            _util.DrawVector4("Surface Diversity Params", "surfaceParams");
            _util.DrawSlider("Fix Smoothness", "fixSmoothness", 0f, 1000f);
        }

        void DrawEdgeFadeOutProps()
        {
            _showEdgeFadeOutProps = _util.Foldout(_showEdgeFadeOutProps, "Edge Fade Out");
            if (!_showEdgeFadeOutProps) { return; }

            _util.DrawToggle("Edge Fade Out", "edgeFadeOutOn");
            _util.DrawVector3("Fade Out Origin", "edgeFadeOutOrigin");
            _util.DrawFloat("Distance", "edgeFadeOutDistance");
            _util.DrawSlider("Sharpness", "edgeSharpness", 0f, 1f);
        }

        void DrawDissolveProps()
        {
            _showDissolveProps = _util.Foldout(_showDissolveProps, "Dissolve Clip Effect");
            if (!_showDissolveProps) { return; }

            _util.DrawSlider("Dissolve Area Size", "dissolveAreaSize", 0f, 100f);
            _util.DrawVector3("Origin", "dissolveOrigin");
            _util.DrawVector3("Slow Factor", "dissolveSlow");
            _util.DrawSlider("Distance to Clip", "dissolveDistance", 0f, 100f);
            _util.DrawSlider("Noise Level", "dissolveNoise", 0f, 10f);
            _util.DrawSlider("Roughness", "dissolveRoughness", 0f, 10f);
            _util.DrawSlider("Edge Sharpness", "dissolveEdgeSharpness", 0f, 10f);
            materialEditor.ColorProperty(_customProperties.dissolveEdgeAddColor, "Edge Add Color");
            materialEditor.ColorProperty(_customProperties.dissolveEdgeSubColor, "Edge Subtract Color");
        }
    }
}
