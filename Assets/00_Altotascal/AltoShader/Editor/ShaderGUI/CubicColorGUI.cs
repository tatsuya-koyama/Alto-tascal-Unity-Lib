using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace AltoLib.ShaderGUI
{
    /// <summary>
    /// Shader GUI for Altotascal/URP 7.3.1/Cubic Color
    /// </summary>
    public class CubicColorGUI : CubicColorGUIBase
    {
        class CustomProperties : ICustomProperties
        {
            public MaterialProperty shadeContrast;
            public MaterialProperty toonShadingOn;
            public MaterialProperty toonShadeStep1;
            public MaterialProperty toonShadeStep2;
            public MaterialProperty toonShadeSmoothness;
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
                shadeContrast       = BaseShaderGUI.FindProperty("_ShadeContrast", properties);
                toonShadingOn       = BaseShaderGUI.FindProperty("_ToonShadingOn", properties);
                toonShadeStep1      = BaseShaderGUI.FindProperty("_ToonShadeStep1", properties);
                toonShadeStep2      = BaseShaderGUI.FindProperty("_ToonShadeStep2", properties);
                toonShadeSmoothness = BaseShaderGUI.FindProperty("_ToonShadeSmoothness", properties);
                rimLightingOn       = BaseShaderGUI.FindProperty("_RimLightingOn", properties);
                rimBurnOn           = BaseShaderGUI.FindProperty("_RimBurnOn", properties);
                rimColor            = BaseShaderGUI.FindProperty("_RimColor", properties);
                rimPower            = BaseShaderGUI.FindProperty("_RimPower", properties);
                cubicRimOn          = BaseShaderGUI.FindProperty("_CubicRimOn", properties);
                coloredShadowOn     = BaseShaderGUI.FindProperty("_ColoredShadowOn", properties);
                shadowColor         = BaseShaderGUI.FindProperty("_ShadowColor", properties);
                shadowPower         = BaseShaderGUI.FindProperty("_ShadowPower", properties);
                hsvShiftOn          = BaseShaderGUI.FindProperty("_HSVShiftOn", properties);
                hue                 = BaseShaderGUI.FindProperty("_Hue", properties);
                saturation          = BaseShaderGUI.FindProperty("_Saturation", properties);
                brightness          = BaseShaderGUI.FindProperty("_Brightness", properties);

                multipleFogOn       = BaseShaderGUI.FindProperty("_MultipleFogOn", properties);
                fogColor1           = BaseShaderGUI.FindProperty("_FogColor1", properties);
                fogColor2           = BaseShaderGUI.FindProperty("_FogColor2", properties);
                fogDistance1        = BaseShaderGUI.FindProperty("_FogDistance1", properties);
                fogDistance2        = BaseShaderGUI.FindProperty("_FogDistance2", properties);

                heightFogOn         = BaseShaderGUI.FindProperty("_HeightFogOn", properties);
                heightFogColor      = BaseShaderGUI.FindProperty("_HeightFogColor", properties);
                heightFogYFrom      = BaseShaderGUI.FindProperty("_HeightFogYFrom", properties);
                heightFogHeight     = BaseShaderGUI.FindProperty("_HeightFogHeight", properties);
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

        protected override void DrawCustomPropAtTop(Material material) {}

        protected override void DrawCustomPropAtBottom(Material material)
        {
            DrawShadingProps();
            DrawRimProps();
            DrawShadowProps();
            DrawHsvProps();
            DrawFogProps();
            DrawHeightFogProps();
        }

        void DrawShadingProps()
        {
            _showShadingProps = _util.Foldout(_showShadingProps, "Basic Shading");
            if (!_showShadingProps) { return; }

            _util.DrawSlider("Shade Contrast", "shadeContrast", -2f, 8f);

            bool toonShadingOn = _util.DrawToggle("Toon Shading", "toonShadingOn");
            EditorGUI.BeginDisabledGroup(!toonShadingOn);
            {
                _util.DrawSlider("Toon Shade Step 1", "toonShadeStep1", 0f, 1f);
                _util.DrawSlider("Toon Shade Step 2", "toonShadeStep2", 0f, 1f);
                _util.DrawSlider("Toon Shade Smoothness", "toonShadeSmoothness", 0f, 0.3f);
            }
            EditorGUI.EndDisabledGroup();
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

