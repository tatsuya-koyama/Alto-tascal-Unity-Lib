using UnityEditor;
using UnityEngine;

namespace AltoLib.ShaderGUI
{
    public class KiraKiraShaderGUI_v10 : SimpleLitGUIBase
    {
        class CustomProperties : ICustomProperties
        {
            public MaterialProperty noisePattern;
            public MaterialProperty ditherPattern;
            public MaterialProperty ditherAlpha;
            public MaterialProperty ditherMinAlpha;
            public MaterialProperty ditherCameraDistanceFrom;
            public MaterialProperty ditherCameraDistanceTo;
            public MaterialProperty ditherCull;
            public MaterialProperty heightDitherYFrom;
            public MaterialProperty heightDitherHeight;

            public MaterialProperty illusionOn;
            public MaterialProperty invertRim;
            public MaterialProperty rimMax;
            public MaterialProperty illusionClip;
            public MaterialProperty illusionClipOffset;
            public MaterialProperty illusionRimPower;
            public MaterialProperty illusionRimColor;
            public MaterialProperty illusionNoiseDensity;
            public MaterialProperty illusionNoiseSpeed;

            public MaterialProperty ignoreFog;
            public MaterialProperty neonOn;
            public MaterialProperty neonFactorX;
            public MaterialProperty neonFactorY;
            public MaterialProperty neonFactorZ;
            public MaterialProperty neonBlinkColor;
            public MaterialProperty neonBlinkSpeed;
            public MaterialProperty emissionNeonOn;
            public MaterialProperty minEmissionLevel;
            public MaterialProperty hueShiftSpeed;
            public MaterialProperty hueShiftOffset;

            public MaterialProperty flickerOn;
            public MaterialProperty flickerTimeOffset;
            public MaterialProperty flickerLow;
            public MaterialProperty flickerHigh;

            public CustomProperties(MaterialProperty[] properties)
            {
                noisePattern             = BaseShaderGUI.FindProperty("_NoisePattern", properties);
                ditherPattern            = BaseShaderGUI.FindProperty("_DitherPattern", properties);
                ditherAlpha              = BaseShaderGUI.FindProperty("_DitherAlpha", properties);
                ditherMinAlpha           = BaseShaderGUI.FindProperty("_DitherMinAlpha", properties);
                ditherCameraDistanceFrom = BaseShaderGUI.FindProperty("_DitherCameraDistanceFrom", properties);
                ditherCameraDistanceTo   = BaseShaderGUI.FindProperty("_DitherCameraDistanceTo", properties);
                ditherCull               = BaseShaderGUI.FindProperty("_DitherCull", properties);
                heightDitherYFrom        = BaseShaderGUI.FindProperty("_HeightDitherYFrom", properties);
                heightDitherHeight       = BaseShaderGUI.FindProperty("_HeightDitherHeight", properties);

                illusionOn               = BaseShaderGUI.FindProperty("_IllusionOn", properties);
                invertRim                = BaseShaderGUI.FindProperty("_InvertRim", properties);
                rimMax                   = BaseShaderGUI.FindProperty("_RimMax", properties);
                illusionClip             = BaseShaderGUI.FindProperty("_IllusionClip", properties);
                illusionClipOffset       = BaseShaderGUI.FindProperty("_IllusionClipOffset", properties);
                illusionRimPower         = BaseShaderGUI.FindProperty("_IllusionRimPower", properties);
                illusionRimColor         = BaseShaderGUI.FindProperty("_IllusionRimColor", properties);
                illusionNoiseDensity     = BaseShaderGUI.FindProperty("_IllusionNoiseDensity", properties);
                illusionNoiseSpeed       = BaseShaderGUI.FindProperty("_IllusionNoiseSpeed", properties);

                ignoreFog                = BaseShaderGUI.FindProperty("_IgnoreFog", properties);
                neonOn                   = BaseShaderGUI.FindProperty("_NeonOn", properties);
                neonFactorX              = BaseShaderGUI.FindProperty("_NeonFactorX", properties);
                neonFactorY              = BaseShaderGUI.FindProperty("_NeonFactorY", properties);
                neonFactorZ              = BaseShaderGUI.FindProperty("_NeonFactorZ", properties);
                neonBlinkColor           = BaseShaderGUI.FindProperty("_NeonBlinkColor", properties);
                neonBlinkSpeed           = BaseShaderGUI.FindProperty("_NeonBlinkSpeed", properties);
                emissionNeonOn           = BaseShaderGUI.FindProperty("_EmissionNeonOn", properties);
                minEmissionLevel         = BaseShaderGUI.FindProperty("_MinEmissionLevel", properties);
                hueShiftSpeed            = BaseShaderGUI.FindProperty("_HueShiftSpeed", properties);
                hueShiftOffset           = BaseShaderGUI.FindProperty("_HueShiftOffset", properties);

                flickerOn                = BaseShaderGUI.FindProperty("_FlickerOn", properties);
                flickerTimeOffset        = BaseShaderGUI.FindProperty("_FlickerTimeOffset", properties);
                flickerLow               = BaseShaderGUI.FindProperty("_FlickerLow", properties);
                flickerHigh              = BaseShaderGUI.FindProperty("_FlickerHigh", properties);
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
        bool _showDitherProps   = true;
        bool _showIllusionProps = true;
        bool _showNeonProps     = true;
        bool _showFlickerProps  = true;

        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);
            _customProperties = new CustomProperties(properties);
            _util = new ShaderGUIUtil(_customProperties);
        }

        public override void DrawAdditionalFoldouts(Material material)
        {
            GUIStyle labelStyle = new GUIStyle() { fontStyle = FontStyle.Bold };
            labelStyle.normal.textColor = EditorStyles.label.normal.textColor;
            EditorGUILayout.LabelField("KiraKira Properties", labelStyle);

            DrawDitherProps();
            DrawIllusionProps();
            DrawNeonProps();
            DrawFlickerProps();
        }

        void DrawDitherProps()
        {
            _showDitherProps = _util.Foldout(_showDitherProps, "Dithering");
            if (!_showDitherProps) { return; }

            materialEditor.TextureProperty(_customProperties.ditherPattern, "Dithering Pattern");
            materialEditor.TextureProperty(_customProperties.noisePattern, "Noise Pattern");
            _util.DrawSlider("Dithering Alpha", "ditherAlpha", 0f, 1f);
            _util.DrawSlider("Minimum Alpha", "ditherMinAlpha", 0f, 1f);
            _util.DrawSlider("Camera Distance Hide", "ditherCameraDistanceTo", 0f, 20f);
            _util.DrawSlider("Camera Distance Start", "ditherCameraDistanceFrom", 0f, 20f);
            _util.DrawSlider("Dithering Cull", "ditherCull", 0f, 20f);
            _util.DrawSlider("Dither Y From", "heightDitherYFrom", -100f, 100f);
            _util.DrawSlider("Dither Height", "heightDitherHeight", 0f, 100f);
        }

        void DrawIllusionProps()
        {
            _showIllusionProps = _util.Foldout(_showIllusionProps, "Illusion");
            if (!_showIllusionProps) { return; }

            bool illusionOn = _util.DrawToggle("Illusion", "illusionOn");
            EditorGUI.BeginDisabledGroup(!illusionOn);
            {
                _util.DrawSlider("Invert Rim", "invertRim", 0f, 1f);
                _util.DrawSlider("RimMax", "rimMax", 0f, 1f);
                _util.DrawSlider("Clip level", "illusionClip", 0f, 8f);
                _util.DrawSlider("Clip offset", "illusionClipOffset", -2f, 2f);
                materialEditor.ColorProperty(_customProperties.illusionRimColor, "Illusion Rim Color");
                _util.DrawSlider("Rim Power", "illusionRimPower", 0f, 8f);
                _util.DrawSlider("Noise Density", "illusionNoiseDensity", 0f, 100f);
                _util.DrawSlider("Noise Speed", "illusionNoiseSpeed", -100f, 100f);
            }
            EditorGUI.EndDisabledGroup();
        }

        void DrawNeonProps()
        {
            _showNeonProps = _util.Foldout(_showNeonProps, "Neon");
            if (!_showNeonProps) { return; }

            _util.DrawToggle("Ignore Fog", "ignoreFog");

            bool neonOn = _util.DrawToggle("Neon", "neonOn");
            _util.DrawSlider("Factor X", "neonFactorX", -100f, 100f);
            _util.DrawSlider("Factor Y", "neonFactorY", -100f, 100f);
            _util.DrawSlider("Factor Z", "neonFactorZ", -100f, 100f);
            materialEditor.ColorProperty(_customProperties.neonBlinkColor, "Blink Color");
            _util.DrawSlider("Blink Speed", "neonBlinkSpeed", -100f, 100f);

            bool emissionNeonOn = _util.DrawToggle("Neon is Emission", "emissionNeonOn");
            _util.DrawSlider("Min Emission Level", "minEmissionLevel", -1f, 1f);

            _util.DrawSlider("Hue Shift Speed", "hueShiftSpeed", 0f, 999f);
            _util.DrawSlider("Hue Shift Offset", "hueShiftOffset", 0f, 999f);
        }

        void DrawFlickerProps()
        {
            _showFlickerProps = _util.Foldout(_showFlickerProps, "Neon Flicker");
            if (!_showFlickerProps) { return; }

            _util.DrawToggle("Flicker", "flickerOn");
            _util.DrawSlider("Time Offset", "flickerTimeOffset", -999f, 999f);
            _util.DrawSlider("Low Level", "flickerLow", -2f, 2f);
            _util.DrawSlider("High Level", "flickerHigh", -2f, 2f);
        }
    }
}
