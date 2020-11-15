using UnityEditor;
using UnityEngine;

namespace AltoLib.ShaderGUI
{
    public class AltoShaderLiteGUI : CubicColorGUIBase
    {
        class CustomProperties : ICustomProperties
        {
            public MaterialProperty billboardOn;

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
            public MaterialProperty ditherAlpha;
            public MaterialProperty ditherMinAlpha;
            public MaterialProperty ditherCameraDistanceFrom;
            public MaterialProperty ditherCameraDistanceTo;
            public MaterialProperty ditherCullOn;

            public MaterialProperty windStrength;
            public MaterialProperty windSpeed;
            public MaterialProperty windBigWave;
            public MaterialProperty windNoise;
            public MaterialProperty windPhaseShift;
            public MaterialProperty windRotateSpeed;
            public MaterialProperty windBaseAngle;

            public MaterialProperty shadeContrast;
            public MaterialProperty rimLightingOn;
            public MaterialProperty rimBurnOn;
            public MaterialProperty rimColor;
            public MaterialProperty rimPower;
            public MaterialProperty cubicRimOn;
            public MaterialProperty coloredShadowOn;
            public MaterialProperty shadowColor;
            public MaterialProperty shadowPower;
            public MaterialProperty hsvShiftOn;
            public MaterialProperty hue;
            public MaterialProperty saturation;
            public MaterialProperty brightness;

            public MaterialProperty multipleFogOn;
            public MaterialProperty fogColor1;
            public MaterialProperty fogColor2;
            public MaterialProperty fogDistance1;
            public MaterialProperty fogDistance2;

            public MaterialProperty heightFogOn;
            public MaterialProperty heightFogColor;
            public MaterialProperty heightFogYFrom;
            public MaterialProperty heightFogHeight;

            public CustomProperties(MaterialProperty[] properties)
            {
                billboardOn              = BaseShaderGUI.FindProperty("_BillboardOn", properties);

                dissolveAreaSize         = BaseShaderGUI.FindProperty("_DissolveAreaSize", properties);
                dissolveOrigin           = BaseShaderGUI.FindProperty("_DissolveOrigin", properties);
                dissolveSlow             = BaseShaderGUI.FindProperty("_DissolveSlow", properties);
                dissolveDistance         = BaseShaderGUI.FindProperty("_DissolveDistance", properties);
                dissolveRoughness        = BaseShaderGUI.FindProperty("_DissolveRoughness", properties);
                dissolveNoise            = BaseShaderGUI.FindProperty("_DissolveNoise", properties);
                dissolveEdgeSharpness    = BaseShaderGUI.FindProperty("_DissolveEdgeSharpness", properties);
                dissolveEdgeAddColor     = BaseShaderGUI.FindProperty("_DissolveEdgeAddColor", properties);
                dissolveEdgeSubColor     = BaseShaderGUI.FindProperty("_DissolveEdgeSubColor", properties);

                ditherPattern            = BaseShaderGUI.FindProperty("_DitherPattern", properties);
                ditherAlpha              = BaseShaderGUI.FindProperty("_DitherAlpha", properties);
                ditherMinAlpha           = BaseShaderGUI.FindProperty("_DitherMinAlpha", properties);
                ditherCameraDistanceFrom = BaseShaderGUI.FindProperty("_DitherCameraDistanceFrom", properties);
                ditherCameraDistanceTo   = BaseShaderGUI.FindProperty("_DitherCameraDistanceTo", properties);
                ditherCullOn             = BaseShaderGUI.FindProperty("_DitherCullOn", properties);

                windStrength             = BaseShaderGUI.FindProperty("_WindStrength", properties);
                windSpeed                = BaseShaderGUI.FindProperty("_WindSpeed", properties);
                windBigWave              = BaseShaderGUI.FindProperty("_WindBigWave", properties);
                windNoise                = BaseShaderGUI.FindProperty("_WindNoise", properties);
                windPhaseShift           = BaseShaderGUI.FindProperty("_WindPhaseShift", properties);
                windRotateSpeed          = BaseShaderGUI.FindProperty("_WindRotateSpeed", properties);
                windBaseAngle            = BaseShaderGUI.FindProperty("_WindBaseAngle", properties);

                shadeContrast            = BaseShaderGUI.FindProperty("_ShadeContrast", properties);
                rimLightingOn            = BaseShaderGUI.FindProperty("_RimLightingOn", properties);
                rimBurnOn                = BaseShaderGUI.FindProperty("_RimBurnOn", properties);
                rimColor                 = BaseShaderGUI.FindProperty("_RimColor", properties);
                rimPower                 = BaseShaderGUI.FindProperty("_RimPower", properties);
                cubicRimOn               = BaseShaderGUI.FindProperty("_CubicRimOn", properties);
                coloredShadowOn          = BaseShaderGUI.FindProperty("_ColoredShadowOn", properties);
                shadowColor              = BaseShaderGUI.FindProperty("_ShadowColor", properties);
                shadowPower              = BaseShaderGUI.FindProperty("_ShadowPower", properties);
                hsvShiftOn               = BaseShaderGUI.FindProperty("_HSVShiftOn", properties);
                hue                      = BaseShaderGUI.FindProperty("_Hue", properties);
                saturation               = BaseShaderGUI.FindProperty("_Saturation", properties);
                brightness               = BaseShaderGUI.FindProperty("_Brightness", properties);

                multipleFogOn            = BaseShaderGUI.FindProperty("_MultipleFogOn", properties);
                fogColor1                = BaseShaderGUI.FindProperty("_FogColor1", properties);
                fogColor2                = BaseShaderGUI.FindProperty("_FogColor2", properties);
                fogDistance1             = BaseShaderGUI.FindProperty("_FogDistance1", properties);
                fogDistance2             = BaseShaderGUI.FindProperty("_FogDistance2", properties);

                heightFogOn              = BaseShaderGUI.FindProperty("_HeightFogOn", properties);
                heightFogColor           = BaseShaderGUI.FindProperty("_HeightFogColor", properties);
                heightFogYFrom           = BaseShaderGUI.FindProperty("_HeightFogYFrom", properties);
                heightFogHeight          = BaseShaderGUI.FindProperty("_HeightFogHeight", properties);
            }

            public object this[string propertyName]
            {
                get { return typeof(CustomProperties).GetField(propertyName).GetValue(this); }

                set { typeof(CustomProperties).GetField(propertyName).SetValue(this, value); }
            }
        }
        CustomProperties _customProperties;
        ShaderGUIUtil _util;
        bool _showDissolveProps   = true;
        bool _showDitherProps     = true;
        bool _showWindProps       = true;
        bool _showShadingProps    = true;
        bool _showRimProps        = true;
        bool _showShadowProps     = true;
        bool _showHsvProps        = true;
        bool _showFogProps        = true;
        bool _showHeightFogProps  = true;

        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);
            _customProperties = new CustomProperties(properties);
            _util = new ShaderGUIUtil(_customProperties);
        }

        protected override void DrawCustomPropAtTop(Material material)
        {
            _util.DrawToggle("Billboard", "billboardOn");
            DrawDitherProps();
            DrawWindProps();
        }

        protected override void DrawCustomPropAtBottom(Material material)
        {
            DrawShadingProps();
            DrawRimProps();
            DrawShadowProps();
            DrawHsvProps();
            DrawFogProps();
            DrawHeightFogProps();
            DrawDissolveProps();
        }

        void DrawDitherProps()
        {
            _showDitherProps = _util.Foldout(_showDitherProps, "Dithering");
            if (!_showDitherProps) { return; }

            materialEditor.TextureProperty(_customProperties.ditherPattern, "Dithering Pattern");
            _util.DrawSlider("Dithering Alpha", "ditherAlpha", 0f, 1f);
            _util.DrawSlider("Minimum Alpha", "ditherMinAlpha", 0f, 1f);
            _util.DrawSlider("Camera Distance Hide", "ditherCameraDistanceTo", 0f, 20f);
            _util.DrawSlider("Camera Distance Start", "ditherCameraDistanceFrom", 0f, 20f);
            _util.DrawToggle("Dithering Cull", "ditherCullOn");
        }

        void DrawWindProps()
        {
            _showWindProps = _util.Foldout(_showWindProps, "Wind Animation");
            if (!_showWindProps) { return; }

            _util.DrawSlider("Wind Strength", "windStrength", 0f, 10f);
            _util.DrawSlider("Wind Speed", "windSpeed", 0f, 10f);
            _util.DrawSlider("Wind Big Wave", "windBigWave", 0f, 10f);
            _util.DrawSlider("Wind Noise", "windNoise", 0f, 10f);
            _util.DrawSlider("Phase Shift", "windPhaseShift", 0f, 10f);
            _util.DrawSlider("Rotate Speed", "windRotateSpeed", 0f, 10f);
            _util.DrawSlider("Base Angle", "windBaseAngle", 0, 360, Mathf.Deg2Rad);
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

        void DrawShadingProps()
        {
            _showShadingProps = _util.Foldout(_showShadingProps, "Basic Shading");
            if (!_showShadingProps) { return; }

            _util.DrawSlider("Shade Contrast", "shadeContrast", -2f, 8f);
        }

        void DrawRimProps()
        {
            _showRimProps = _util.Foldout(_showRimProps, "Rim Lighting");
            if (!_showRimProps) { return; }

            bool rimLightingOn = _util.DrawToggle("Rim Lighting", "rimLightingOn");
            bool rimBurnOn     = _util.DrawToggle("Rim Burn", "rimBurnOn");

            EditorGUI.BeginDisabledGroup(!rimLightingOn && !rimBurnOn);
            {
                materialEditor.ColorProperty(_customProperties.rimColor, "Rim Color");
                _util.DrawSlider("Rim Power", "rimPower", 0f, 8f);
                _util.DrawToggle("Use Cubic Color as Rim", "cubicRimOn");
            }
            EditorGUI.EndDisabledGroup();
        }

        void DrawShadowProps()
        {
            _showShadowProps = _util.Foldout(_showShadowProps, "Colored Shadow");
            if (!_showShadowProps) { return; }

            bool coloredShadowOn = _util.DrawToggle("Colored Shadow", "coloredShadowOn");

            EditorGUI.BeginDisabledGroup(!coloredShadowOn);
            {
                materialEditor.ColorProperty(_customProperties.shadowColor, "Shadow Color");
                _util.DrawSlider("Shadow Power", "shadowPower", 0f, 2f);
            }
            EditorGUI.EndDisabledGroup();
        }

        void DrawHsvProps()
        {
            _showHsvProps = _util.Foldout(_showHsvProps, "HSV Shift");
            if (!_showHsvProps) { return; }

            bool hsvShiftOn = _util.DrawToggle("HSV Shift", "hsvShiftOn");
            EditorGUI.BeginDisabledGroup(!hsvShiftOn);
            {
                _util.DrawSlider("Hue", "hue", 0f, 360f);
                _util.DrawSlider("Saturation", "saturation", -8f, 8f);
                _util.DrawSlider("Brightness (Value)", "brightness", -8f, 8f);
            }
            EditorGUI.EndDisabledGroup();
        }

        void DrawFogProps()
        {
            _showFogProps = _util.Foldout(_showFogProps, "Multiple Fog");
            if (!_showFogProps) { return; }

            bool fogOn = _util.DrawToggle("Multiple Fog", "multipleFogOn");
            EditorGUI.BeginDisabledGroup(!fogOn);
            {
                materialEditor.ColorProperty(_customProperties.fogColor1, "Fog Color 1");
                materialEditor.ColorProperty(_customProperties.fogColor2, "Fog Color 2");
                _util.DrawSlider("Distance 1", "fogDistance1", 0f, 100f);
                _util.DrawSlider("Distance 2", "fogDistance2", 0f, 100f);
            }
            EditorGUI.EndDisabledGroup();
        }

        void DrawHeightFogProps()
        {
            _showHeightFogProps = _util.Foldout(_showHeightFogProps, "Height Fog");
            if (!_showHeightFogProps) { return; }

            bool fogOn = _util.DrawToggle("Height Fog", "heightFogOn");
            EditorGUI.BeginDisabledGroup(!fogOn);
            {
                materialEditor.ColorProperty(_customProperties.heightFogColor, "Height Fog Color");
                _util.DrawSlider("Y From", "heightFogYFrom", -100f, 100f);
                _util.DrawSlider("Height", "heightFogHeight", 0f, 100f);
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
