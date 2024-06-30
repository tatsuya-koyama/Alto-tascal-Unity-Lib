using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace AltoLib.ShaderGUI
{
    /// <summary>
    /// Shader GUI base class for Cubic color shader
    /// </summary>
    public class UIGradientMapShaderGUI : BaseShaderGUI
    {
        class CustomProperties : ICustomProperties
        {
            // common props
            public MaterialProperty color0;
            public MaterialProperty color1;
            public MaterialProperty color2;
            public MaterialProperty color3;
            public MaterialProperty color4;
            public MaterialProperty offset0;
            public MaterialProperty offset1;
            public MaterialProperty offset2;
            public MaterialProperty offset3;
            public MaterialProperty gammaCorrection;

            public CustomProperties(MaterialProperty[] properties)
            {
                color0          = BaseShaderGUI.FindProperty("_Color0", properties);
                color1          = BaseShaderGUI.FindProperty("_Color1", properties);
                color2          = BaseShaderGUI.FindProperty("_Color2", properties);
                color3          = BaseShaderGUI.FindProperty("_Color3", properties);
                color4          = BaseShaderGUI.FindProperty("_Color4", properties);
                offset0         = BaseShaderGUI.FindProperty("_Offset0", properties);
                offset1         = BaseShaderGUI.FindProperty("_Offset1", properties);
                offset2         = BaseShaderGUI.FindProperty("_Offset2", properties);
                offset3         = BaseShaderGUI.FindProperty("_Offset3", properties);
                gammaCorrection = BaseShaderGUI.FindProperty("_GammaCorrection", properties);
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

        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);
            _customProperties = new CustomProperties(properties);
            _util = new ShaderGUIUtil(_customProperties);
        }

        public override void OnGUI(MaterialEditor materialEditorIn, MaterialProperty[] properties)
        {
            materialEditor = materialEditorIn;
            FindProperties(properties);
            DrawColorProps();
            DrawColorSampleTexture();
        }

        void DrawColorProps()
        {
            materialEditor.ColorProperty(_customProperties.color0, "Color 0");
            materialEditor.ColorProperty(_customProperties.color1, "Color 1");
            materialEditor.ColorProperty(_customProperties.color2, "Color 2");
            materialEditor.ColorProperty(_customProperties.color3, "Color 3");
            materialEditor.ColorProperty(_customProperties.color4, "Color 4");
            _util.DrawSlider("Offset 0", "offset0", -1f, 1f);
            _util.DrawSlider("Offset 1", "offset1", -1f, 1f);
            _util.DrawSlider("Offset 2", "offset2", -1f, 1f);
            _util.DrawSlider("Offset 3", "offset3", -1f, 1f);
            _util.DrawToggle("Gamma Correction", "gammaCorrection");
        }

        void DrawColorSampleTexture()
        {
            GUILayout.Space(10);
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(50));
            rect.width -= 20f;
            GUI.DrawTexture(rect, MakeGradientTexture());
        }

        Texture2D MakeGradientTexture(int resolution = 512)
        {
            Texture2D texture = new(resolution, 1);
            Color color0  = _customProperties.color0.colorValue;
            Color color1  = _customProperties.color1.colorValue;
            Color color2  = _customProperties.color2.colorValue;
            Color color3  = _customProperties.color3.colorValue;
            Color color4  = _customProperties.color4.colorValue;
            float offset0 = _customProperties.offset0.floatValue;
            float offset1 = _customProperties.offset1.floatValue;
            float offset2 = _customProperties.offset2.floatValue;
            float offset3 = _customProperties.offset3.floatValue;

            for (int i = 0; i < resolution; ++i)
            {
                float t = (float)(i + 1) / resolution * 4;
                Color color =
                    LerpColor(
                        LerpColor(
                            LerpColor(
                                LerpColor(
                                    color0,
                                    color1, t, offset0, 1 + offset1
                                ),
                                color2, t, 1 + offset1, 2 + offset2
                            ),
                            color3, t, 2 + offset2, 3 + offset3
                        ),
                        color4, t, 3 + offset3, 4
                    );
                texture.SetPixel(i, 0, color);
            }
            texture.Apply();
            return texture;
        }

        Color LerpColor(Color colorA, Color colorB, float t, float min, float max)
        {
            t = Mathf.Clamp01((t - min) / (max - min));
            return Color.Lerp(colorA, colorB, t);
        }
    }
}

