using UnityEditor;
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

            public CustomProperties(MaterialProperty[] properties)
            {
                waterColorDepth    = BaseShaderGUI.FindProperty("_WaterColorDepth", properties);
                depthDebug         = BaseShaderGUI.FindProperty("_DepthDebug", properties);
                foamColor          = BaseShaderGUI.FindProperty("_FoamColor", properties);
                foamSharpness      = BaseShaderGUI.FindProperty("_FoamSharpness", properties);
                foamFactor         = BaseShaderGUI.FindProperty("_FoamFactor", properties);
                underwaterColor    = BaseShaderGUI.FindProperty("_UnderwaterColor", properties);
                multiplyUnderwater = BaseShaderGUI.FindProperty("_MultiplyUnderwaterColor", properties);
                waterDistortion    = BaseShaderGUI.FindProperty("_WaterDistortion", properties);

                waveCycle          = BaseShaderGUI.FindProperty("_WaveCycle", properties);
                waveSpeed          = BaseShaderGUI.FindProperty("_WaveSpeed", properties);
                wavePower          = BaseShaderGUI.FindProperty("_WavePower", properties);
                riseAndFall        = BaseShaderGUI.FindProperty("_RiseAndFall", properties);
                surfaceSpecular    = BaseShaderGUI.FindProperty("_SurfaceSpecular", properties);
                surfaceNoise       = BaseShaderGUI.FindProperty("_SurfaceNoise", properties);
                surfaceParams      = BaseShaderGUI.FindProperty("_SurfaceParams", properties);
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
        bool _showWaterColorProps = true;
        bool _showSurfaceProps    = true;

        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);
            _customProperties = new CustomProperties(properties);
            _util = new ShaderGUIUtil(_customProperties);
        }

        public override void DrawAdditionalFoldouts(Material material)
        {
            DrawWaterColorProps();
            DrawSurfaceProps();
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
        }
    }
}
