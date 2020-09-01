using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace AltoLib.ShaderGUI
{
    /// <summary>
    /// Shader GUI for Altotascal/URP 7.3.1/Cubic Color
    /// </summary>
    public class CubicColorGUI : SimpleLitGUIBase
    {
        class CustomProperties : ICustomProperties
        {
            // common props
            public MaterialProperty topColor1;
            public MaterialProperty topColor2;
            public MaterialProperty rightColor1;
            public MaterialProperty rightColor2;
            public MaterialProperty frontColor1;
            public MaterialProperty frontColor2;
            public MaterialProperty leftColor1;
            public MaterialProperty leftColor2;
            public MaterialProperty backColor1;
            public MaterialProperty backColor2;
            public MaterialProperty bottomColor1;
            public MaterialProperty bottomColor2;
            public MaterialProperty mixCubicColorOn;
            public MaterialProperty cubicColorPower;
            public MaterialProperty worldSpaceNormal;
            public MaterialProperty worldSpaceGradient;

            public MaterialProperty gradOrigin_T;
            public MaterialProperty gradOrigin_R;
            public MaterialProperty gradOrigin_F;
            public MaterialProperty gradOrigin_L;
            public MaterialProperty gradOrigin_B;
            public MaterialProperty gradOrigin_D;

            public MaterialProperty gradHeight_T;
            public MaterialProperty gradHeight_R;
            public MaterialProperty gradHeight_F;
            public MaterialProperty gradHeight_L;
            public MaterialProperty gradHeight_B;
            public MaterialProperty gradHeight_D;

            public MaterialProperty gradRotate_T;
            public MaterialProperty gradRotate_R;
            public MaterialProperty gradRotate_F;
            public MaterialProperty gradRotate_L;
            public MaterialProperty gradRotate_B;
            public MaterialProperty gradRotate_D;

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
                topColor1           = BaseShaderGUI.FindProperty("_TopColor1", properties);
                topColor2           = BaseShaderGUI.FindProperty("_TopColor2", properties);
                rightColor1         = BaseShaderGUI.FindProperty("_RightColor1", properties);
                rightColor2         = BaseShaderGUI.FindProperty("_RightColor2", properties);
                frontColor1         = BaseShaderGUI.FindProperty("_FrontColor1", properties);
                frontColor2         = BaseShaderGUI.FindProperty("_FrontColor2", properties);
                leftColor1          = BaseShaderGUI.FindProperty("_LeftColor1", properties);
                leftColor2          = BaseShaderGUI.FindProperty("_LeftColor2", properties);
                backColor1          = BaseShaderGUI.FindProperty("_BackColor1", properties);
                backColor2          = BaseShaderGUI.FindProperty("_BackColor2", properties);
                bottomColor1        = BaseShaderGUI.FindProperty("_BottomColor1", properties);
                bottomColor2        = BaseShaderGUI.FindProperty("_BottomColor2", properties);
                mixCubicColorOn     = BaseShaderGUI.FindProperty("_MixCubicColorOn", properties);
                cubicColorPower     = BaseShaderGUI.FindProperty("_CubicColorPower", properties);
                worldSpaceNormal    = BaseShaderGUI.FindProperty("_WorldSpaceNormal", properties);
                worldSpaceGradient  = BaseShaderGUI.FindProperty("_WorldSpaceGradient", properties);

                gradOrigin_T        = BaseShaderGUI.FindProperty("_GradOrigin_T", properties);
                gradOrigin_R        = BaseShaderGUI.FindProperty("_GradOrigin_R", properties);
                gradOrigin_F        = BaseShaderGUI.FindProperty("_GradOrigin_F", properties);
                gradOrigin_L        = BaseShaderGUI.FindProperty("_GradOrigin_L", properties);
                gradOrigin_B        = BaseShaderGUI.FindProperty("_GradOrigin_B", properties);
                gradOrigin_D        = BaseShaderGUI.FindProperty("_GradOrigin_D", properties);

                gradHeight_T        = BaseShaderGUI.FindProperty("_GradHeight_T", properties);
                gradHeight_R        = BaseShaderGUI.FindProperty("_GradHeight_R", properties);
                gradHeight_F        = BaseShaderGUI.FindProperty("_GradHeight_F", properties);
                gradHeight_L        = BaseShaderGUI.FindProperty("_GradHeight_L", properties);
                gradHeight_B        = BaseShaderGUI.FindProperty("_GradHeight_B", properties);
                gradHeight_D        = BaseShaderGUI.FindProperty("_GradHeight_D", properties);

                gradRotate_T        = BaseShaderGUI.FindProperty("_GradRotate_T", properties);
                gradRotate_R        = BaseShaderGUI.FindProperty("_GradRotate_R", properties);
                gradRotate_F        = BaseShaderGUI.FindProperty("_GradRotate_F", properties);
                gradRotate_L        = BaseShaderGUI.FindProperty("_GradRotate_L", properties);
                gradRotate_B        = BaseShaderGUI.FindProperty("_GradRotate_B", properties);
                gradRotate_D        = BaseShaderGUI.FindProperty("_GradRotate_D", properties);

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
        bool _showCubicColorProps = true;
        bool _showShadingProps    = true;
        bool _showRimProps        = true;
        bool _showShadowProps     = true;
        bool _showHsvProps        = true;
        bool _showFogProps        = true;
        bool _showHeightFogProps  = true;

        bool _showBlock_T = true;
        bool _showBlock_R = true;
        bool _showBlock_F = true;
        bool _showBlock_L = true;
        bool _showBlock_B = true;
        bool _showBlock_D = true;

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
            EditorGUILayout.LabelField("Custom Properties", labelStyle);

            DrawCubicColorProps();
            DrawShadingProps();
            DrawRimProps();
            DrawShadowProps();
            DrawHsvProps();
            DrawFogProps();
            DrawHeightFogProps();
        }

        void DrawCubicColorProps()
        {
            _showCubicColorProps = _util.Foldout(_showCubicColorProps, "Cubic Color");
            if (!_showCubicColorProps) { return; }

            DrawCubicColorSwapButtons();

            DrawCubicColorBlock(
                ref _showBlock_T, "Top", "T",
                ref _customProperties.topColor1,
                ref _customProperties.topColor2
            );
            DrawCubicColorBlock(
                ref _showBlock_R, "Right", "R",
                ref _customProperties.rightColor1,
                ref _customProperties.rightColor2
            );
            DrawCubicColorBlock(
                ref _showBlock_F, "Front", "F",
                ref _customProperties.frontColor1,
                ref _customProperties.frontColor2
            );
            DrawCubicColorBlock(
                ref _showBlock_L, "Left", "L",
                ref _customProperties.leftColor1,
                ref _customProperties.leftColor2
            );
            DrawCubicColorBlock(
                ref _showBlock_B, "Back", "B",
                ref _customProperties.backColor1,
                ref _customProperties.backColor2
            );
            DrawCubicColorBlock(
                ref _showBlock_D, "Bottom", "D",
                ref _customProperties.bottomColor1,
                ref _customProperties.bottomColor2
            );
            EditorGUILayout.Space();

            _util.DrawToggle("Mix Cubic Color", "mixCubicColorOn");
            _util.DrawSlider("Cubic Color Power", "cubicColorPower", -1f, 1f);
            _util.DrawSlider("World Space Normal", "worldSpaceNormal", 0f, 1f);
            _util.DrawSlider("World Space Gradient", "worldSpaceGradient", 0f, 1f);
        }

        void DrawCubicColorSwapButtons()
        {
            var p = _customProperties;

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Swap LR"))
                {
                    Color tempColor1 = p.leftColor1.colorValue;
                    Color tempColor2 = p.leftColor2.colorValue;
                    p.leftColor1.colorValue = p.rightColor1.colorValue;
                    p.leftColor2.colorValue = p.rightColor2.colorValue;
                    p.rightColor1.colorValue = tempColor1;
                    p.rightColor2.colorValue = tempColor2;
                }
                if (GUILayout.Button("Swap FB"))
                {
                    Color tempColor1 = p.frontColor1.colorValue;
                    Color tempColor2 = p.frontColor2.colorValue;
                    p.frontColor1.colorValue = p.backColor1.colorValue;
                    p.frontColor2.colorValue = p.backColor2.colorValue;
                    p.backColor1.colorValue = tempColor1;
                    p.backColor2.colorValue = tempColor2;
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Same as Top"))
                {
                    Color color1 = p.topColor1.colorValue;
                    Color color2 = p.topColor2.colorValue;
                    p.rightColor1.colorValue = color1;
                    p.rightColor2.colorValue = color2;
                    p.frontColor1.colorValue = color1;
                    p.frontColor2.colorValue = color2;
                    p.leftColor1.colorValue = color1;
                    p.leftColor2.colorValue = color2;
                    p.backColor1.colorValue = color1;
                    p.backColor2.colorValue = color2;
                    p.bottomColor1.colorValue = color1;
                    p.bottomColor2.colorValue = color2;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        void DrawCubicColorBlock(
            ref bool showBlock, string label, string postfix,
            ref MaterialProperty color1,
            ref MaterialProperty color2
        )
        {
            showBlock = EditorGUILayout.Foldout(showBlock, label);
            if (!showBlock) { return; }

            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.BeginVertical();
                    {
                        materialEditor.ColorProperty(color1, "");
                        materialEditor.ColorProperty(color2, "");
                    }
                    EditorGUILayout.EndVertical();

                    GUILayout.Space(10);
                    Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(50), GUILayout.Width(50));
                    GUI.DrawTexture(rect, _util.MakeGradientTexture(color1.colorValue, color2.colorValue));
                    GUILayout.Space(10);

                    EditorGUILayout.BeginVertical();
                    {
                        if (GUILayout.Button("Swap"))
                        {
                            Color tempColor = color1.colorValue;
                            color1.colorValue = color2.colorValue;
                            color2.colorValue = tempColor;
                        }
                        EditorGUILayout.Space();
                        if (GUILayout.Button("Same as upper"))
                        {
                            color2.colorValue = color1.colorValue;
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();

                _util.DrawVector3("Gradient Origin Pos", "gradOrigin_" + postfix);
                _util.DrawFloat("Gradient Height", "gradHeight_" + postfix);
                _util.DrawSlider("Gradient Rotation", "gradRotate_" + postfix, 0, 360, Mathf.Deg2Rad);
            }
            EditorGUILayout.EndVertical();
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

