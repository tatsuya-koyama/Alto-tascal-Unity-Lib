using AltoLib.ShaderGUI;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

namespace AltoLib.ShaderGUI
{
    public class SoyokazeShaderGUI : SimpleLitGUIBase
    {
        class CustomProperties : ICustomProperties
        {
            public MaterialProperty worldSpaceUVOn;
            public MaterialProperty heightMap;
            public MaterialProperty heightScale;
            public MaterialProperty windMap;
            public MaterialProperty windMapScale;
            public MaterialProperty windPower;
            public MaterialProperty windDirectionX;
            public MaterialProperty windDirectionZ;
            public MaterialProperty windSpecularPower;
            public MaterialProperty windAlbedoPower;
            public MaterialProperty uvDistortionOn;
            public MaterialProperty uvDistortionParams;

            public MaterialProperty noisePattern;
            public MaterialProperty ditherPattern;
            public MaterialProperty ditherAlpha;
            public MaterialProperty ditherMinAlpha;
            public MaterialProperty ditherCameraDistanceFrom;
            public MaterialProperty ditherCameraDistanceTo;
            public MaterialProperty ditherCull;
            public MaterialProperty heightDitherYFrom;
            public MaterialProperty heightDitherHeight;

            public MaterialProperty shadeContrast;
            public MaterialProperty rimLightingOn;
            public MaterialProperty rimBurnOn;
            public MaterialProperty rimColor;
            public MaterialProperty rimPower;
            public MaterialProperty coloredShadowOn;
            public MaterialProperty shadowColor;
            public MaterialProperty shadowPower;
            public MaterialProperty coloredShadePower;
            public MaterialProperty hsvShiftOn;
            public MaterialProperty hue;
            public MaterialProperty saturation;
            public MaterialProperty brightness;

            public MaterialProperty heightFogOn;
            public MaterialProperty heightFogColor;
            public MaterialProperty heightFogYFrom;
            public MaterialProperty heightFogHeight;

            public CustomProperties(MaterialProperty[] properties)
            {
                worldSpaceUVOn           = BaseShaderGUI.FindProperty("_WorldSpaceUVOn", properties);
                heightMap                = BaseShaderGUI.FindProperty("_HeightMap", properties);
                heightScale              = BaseShaderGUI.FindProperty("_HeightScale", properties);
                windMap                  = BaseShaderGUI.FindProperty("_WindMap", properties);
                windMapScale             = BaseShaderGUI.FindProperty("_WindMapScale", properties);
                windPower                = BaseShaderGUI.FindProperty("_WindPower", properties);
                windDirectionX           = BaseShaderGUI.FindProperty("_WindDirectionX", properties);
                windDirectionZ           = BaseShaderGUI.FindProperty("_WindDirectionZ", properties);
                windSpecularPower        = BaseShaderGUI.FindProperty("_WindSpecularPower", properties);
                windAlbedoPower          = BaseShaderGUI.FindProperty("_WindAlbedoPower", properties);
                uvDistortionOn           = BaseShaderGUI.FindProperty("_UVDistortionOn", properties);
                uvDistortionParams       = BaseShaderGUI.FindProperty("_UVDistortionParams", properties);

                noisePattern             = BaseShaderGUI.FindProperty("_NoisePattern", properties);
                ditherPattern            = BaseShaderGUI.FindProperty("_DitherPattern", properties);
                ditherAlpha              = BaseShaderGUI.FindProperty("_DitherAlpha", properties);
                ditherMinAlpha           = BaseShaderGUI.FindProperty("_DitherMinAlpha", properties);
                ditherCameraDistanceFrom = BaseShaderGUI.FindProperty("_DitherCameraDistanceFrom", properties);
                ditherCameraDistanceTo   = BaseShaderGUI.FindProperty("_DitherCameraDistanceTo", properties);
                ditherCull               = BaseShaderGUI.FindProperty("_DitherCull", properties);
                heightDitherYFrom        = BaseShaderGUI.FindProperty("_HeightDitherYFrom", properties);
                heightDitherHeight       = BaseShaderGUI.FindProperty("_HeightDitherHeight", properties);

                shadeContrast            = BaseShaderGUI.FindProperty("_ShadeContrast", properties);
                rimLightingOn            = BaseShaderGUI.FindProperty("_RimLightingOn", properties);
                rimBurnOn                = BaseShaderGUI.FindProperty("_RimBurnOn", properties);
                rimColor                 = BaseShaderGUI.FindProperty("_RimColor", properties);
                rimPower                 = BaseShaderGUI.FindProperty("_RimPower", properties);
                coloredShadowOn          = BaseShaderGUI.FindProperty("_ColoredShadowOn", properties);
                shadowColor              = BaseShaderGUI.FindProperty("_ShadowColor", properties);
                shadowPower              = BaseShaderGUI.FindProperty("_ShadowPower", properties);
                coloredShadePower        = BaseShaderGUI.FindProperty("_ColoredShadePower", properties);
                hsvShiftOn               = BaseShaderGUI.FindProperty("_HSVShiftOn", properties);
                hue                      = BaseShaderGUI.FindProperty("_Hue", properties);
                saturation               = BaseShaderGUI.FindProperty("_Saturation", properties);
                brightness               = BaseShaderGUI.FindProperty("_Brightness", properties);

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
        bool _showWindProps   = true;
        bool _showDitherProps     = true;
        bool _showShadingProps    = true;
        bool _showRimProps        = true;
        bool _showShadowProps     = true;
        bool _showHsvProps        = true;
        bool _showHeightFogProps  = true;

        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);
            _customProperties = new CustomProperties(properties);
            _util = new ShaderGUIUtil(_customProperties);
        }

        public override void OnGUI(MaterialEditor materialEditorIn, MaterialProperty[] properties)
        {
            base.OnGUI(materialEditorIn, properties);
            CoreEditorUtils.DrawHeaderFoldout("AltoShader Params", true);
            DrawAdditionalFoldouts();
        }

        public void DrawAdditionalFoldouts()
        {
            GUIStyle labelStyle = new GUIStyle() { fontStyle = FontStyle.Bold };
            labelStyle.normal.textColor = EditorStyles.label.normal.textColor;

            DrawWindProps();
            DrawShadingProps();
            DrawDitherProps();
            DrawRimProps();
            DrawShadowProps();
            DrawHsvProps();
            DrawHeightFogProps();
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

        void DrawWindProps()
        {
            _showWindProps = _util.Foldout(_showWindProps, "Parallax Mapping");
            if (!_showWindProps) { return; }

            _util.DrawToggle("World Space UV", "worldSpaceUVOn");
            materialEditor.TextureProperty(_customProperties.heightMap, "Height Map (R)");
            _util.DrawSlider("Height Scale", "heightScale", 0f, 1f);
            materialEditor.TextureProperty(_customProperties.windMap, "Wind Map (R)");
            _util.DrawSlider("Wind Map Scale", "windMapScale", 0f, 1000f);
            _util.DrawSlider("Wind Power", "windPower", 0f, 1000f);
            _util.DrawFloat("Wind Direction X", "windDirectionX");
            _util.DrawFloat("Wind Direction Z", "windDirectionZ");
            _util.DrawSlider("Wind Specular Power", "windSpecularPower", 0f, 1f);
            _util.DrawSlider("Wind Albedo Power", "windAlbedoPower", 0f, 1f);

            _util.DrawToggle("UV Distortion", "uvDistortionOn");
            _util.DrawVector4("UV Distortion Params", "uvDistortionParams");
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
                _util.DrawSlider("Shade Power", "coloredShadePower", 0f, 2f);
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
