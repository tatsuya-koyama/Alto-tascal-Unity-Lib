using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace AltoLib.ShaderGUI
{
    /// <summary>
    /// Shader GUI base class for Cubic color shader
    /// </summary>
    public class CubicColorGUIBase : SimpleLitGUIBase
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
            public MaterialProperty multiplyCubicDiffuseOn;
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

            public CustomProperties(MaterialProperty[] properties)
            {
                topColor1              = BaseShaderGUI.FindProperty("_TopColor1", properties);
                topColor2              = BaseShaderGUI.FindProperty("_TopColor2", properties);
                rightColor1            = BaseShaderGUI.FindProperty("_RightColor1", properties);
                rightColor2            = BaseShaderGUI.FindProperty("_RightColor2", properties);
                frontColor1            = BaseShaderGUI.FindProperty("_FrontColor1", properties);
                frontColor2            = BaseShaderGUI.FindProperty("_FrontColor2", properties);
                leftColor1             = BaseShaderGUI.FindProperty("_LeftColor1", properties);
                leftColor2             = BaseShaderGUI.FindProperty("_LeftColor2", properties);
                backColor1             = BaseShaderGUI.FindProperty("_BackColor1", properties);
                backColor2             = BaseShaderGUI.FindProperty("_BackColor2", properties);
                bottomColor1           = BaseShaderGUI.FindProperty("_BottomColor1", properties);
                bottomColor2           = BaseShaderGUI.FindProperty("_BottomColor2", properties);
                mixCubicColorOn        = BaseShaderGUI.FindProperty("_MixCubicColorOn", properties);
                multiplyCubicDiffuseOn = BaseShaderGUI.FindProperty("_MultiplyCubicDiffuseOn", properties);
                cubicColorPower        = BaseShaderGUI.FindProperty("_CubicColorPower", properties);
                worldSpaceNormal       = BaseShaderGUI.FindProperty("_WorldSpaceNormal", properties);
                worldSpaceGradient     = BaseShaderGUI.FindProperty("_WorldSpaceGradient", properties);

                gradOrigin_T = BaseShaderGUI.FindProperty("_GradOrigin_T", properties);
                gradOrigin_R = BaseShaderGUI.FindProperty("_GradOrigin_R", properties);
                gradOrigin_F = BaseShaderGUI.FindProperty("_GradOrigin_F", properties);
                gradOrigin_L = BaseShaderGUI.FindProperty("_GradOrigin_L", properties);
                gradOrigin_B = BaseShaderGUI.FindProperty("_GradOrigin_B", properties);
                gradOrigin_D = BaseShaderGUI.FindProperty("_GradOrigin_D", properties);

                gradHeight_T = BaseShaderGUI.FindProperty("_GradHeight_T", properties);
                gradHeight_R = BaseShaderGUI.FindProperty("_GradHeight_R", properties);
                gradHeight_F = BaseShaderGUI.FindProperty("_GradHeight_F", properties);
                gradHeight_L = BaseShaderGUI.FindProperty("_GradHeight_L", properties);
                gradHeight_B = BaseShaderGUI.FindProperty("_GradHeight_B", properties);
                gradHeight_D = BaseShaderGUI.FindProperty("_GradHeight_D", properties);

                gradRotate_T = BaseShaderGUI.FindProperty("_GradRotate_T", properties);
                gradRotate_R = BaseShaderGUI.FindProperty("_GradRotate_R", properties);
                gradRotate_F = BaseShaderGUI.FindProperty("_GradRotate_F", properties);
                gradRotate_L = BaseShaderGUI.FindProperty("_GradRotate_L", properties);
                gradRotate_B = BaseShaderGUI.FindProperty("_GradRotate_B", properties);
                gradRotate_D = BaseShaderGUI.FindProperty("_GradRotate_D", properties);
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

        public override void FillAdditionalFoldouts(MaterialHeaderScopeList materialScopesList)
        {
            GUIStyle labelStyle = new GUIStyle() { fontStyle = FontStyle.Bold };
            labelStyle.normal.textColor = EditorStyles.label.normal.textColor;
            EditorGUILayout.LabelField("Custom Properties", labelStyle);

            DrawCustomPropAtTop(materialScopesList);
            DrawCubicColorProps();
            DrawCustomPropAtBottom(materialScopesList);
        }

        protected virtual void DrawCustomPropAtTop(MaterialHeaderScopeList materialScopesList) {}
        protected virtual void DrawCustomPropAtBottom(MaterialHeaderScopeList materialScopesList) {}

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
            _util.DrawToggle("Multiply Cubic & Diffuse", "multiplyCubicDiffuseOn");
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
                if (GUILayout.Button("Rotate"))
                {
                    RotateColor();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        void RotateColor()
        {
            var p = _customProperties;
            Color tempColor1 = p.frontColor1.colorValue;
            Color tempColor2 = p.frontColor2.colorValue;
            p.frontColor1.colorValue = p.rightColor1.colorValue;
            p.frontColor2.colorValue = p.rightColor2.colorValue;
            p.rightColor1.colorValue = p.backColor1.colorValue;
            p.rightColor2.colorValue = p.backColor2.colorValue;
            p.backColor1.colorValue = p.leftColor1.colorValue;
            p.backColor2.colorValue = p.leftColor2.colorValue;
            p.leftColor1.colorValue = tempColor1;
            p.leftColor2.colorValue = tempColor2;
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
    }
}

