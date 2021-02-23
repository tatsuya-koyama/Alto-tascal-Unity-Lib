using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace AltoLib.ShaderGUI
{
    /// <summary>
    /// Shader GUI for Altotascal/URP 7.3.1/Custom Simple Lit
    /// </summary>
    public class CustomSimpleLitGUI : SimpleLitGUIBase
    {
        class CustomProperties : ICustomProperties
        {
            // common props
            public MaterialProperty shadeContrast;
            public MaterialProperty toonShadingOn;
            public MaterialProperty toonShadeStep1;
            public MaterialProperty toonShadeStep2;
            public MaterialProperty toonShadeSmoothness;
            public MaterialProperty rimLightingOn;
            public MaterialProperty rimBurnOn;
            public MaterialProperty rimColor;
            public MaterialProperty rimPower;
            public MaterialProperty coloredShadowOn;
            public MaterialProperty shadowColor;
            public MaterialProperty shadowPower;
            public MaterialProperty hsvShiftOn;
            public MaterialProperty hue;
            public MaterialProperty saturation;
            public MaterialProperty brightness;

            // outline props
            public MaterialProperty outlineColor;
            public MaterialProperty outlineWidth;
            public MaterialProperty mixOutlineColorOn;

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
                coloredShadowOn     = BaseShaderGUI.FindProperty("_ColoredShadowOn", properties);
                shadowColor         = BaseShaderGUI.FindProperty("_ShadowColor", properties);
                shadowPower         = BaseShaderGUI.FindProperty("_ShadowPower", properties);
                hsvShiftOn          = BaseShaderGUI.FindProperty("_HSVShiftOn", properties);
                hue                 = BaseShaderGUI.FindProperty("_Hue", properties);
                saturation          = BaseShaderGUI.FindProperty("_Saturation", properties);
                brightness          = BaseShaderGUI.FindProperty("_Brightness", properties);

                outlineColor        = BaseShaderGUI.FindProperty("_OutlineColor", properties, false);
                outlineWidth        = BaseShaderGUI.FindProperty("_OutlineWidth", properties, false);
                mixOutlineColorOn   = BaseShaderGUI.FindProperty("_MixOutlineColorOn", properties, false);
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
        bool _showShadingProps = true;
        bool _showRimProps     = true;
        bool _showShadowProps  = true;
        bool _showHsvProps     = true;
        bool _showOutlineProps = true;

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

            DrawShadingProps();
            DrawRimProps();
            DrawShadowProps();
            DrawHsvProps();

            DrawOutlineProps();
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

        void DrawOutlineProps()
        {
            if (_customProperties.outlineColor == null) { return; }

            _showOutlineProps = _util.Foldout(_showOutlineProps, "Outline");
            if (!_showOutlineProps) { return; }

            materialEditor.ColorProperty(_customProperties.outlineColor, "Outline Color");
            _util.DrawSlider("Outline Width", "outlineWidth", 0f, 8f);
            _util.DrawToggle("Mix Outline Color with Diffuse", "mixOutlineColorOn");
        }

        /// [Note]
        /// Shader Keyword を ON / OFF してバリアントを作る場合は以下のように記述する。
        /// ただしバリアントを作ると SRP Batcher が効かなくなって描画効率が下がる。
        /// シェーダでは if 文が避けられるが、静的な分岐で if 文を使うならそんなに負荷には
        /// ならないと思うので、オプションの ON / OFF はシェーダ側の if 文で処理している。
        ///
        // public override void MaterialChanged(Material material)
        // {
        //     base.MaterialChanged(material);
        //
        //     if (material.HasProperty("_RimLightingOn"))
        //     {
        //         CoreUtils.SetKeyword(material, "_RIM_LIGHTING_ON", material.GetFloat("_RimLightingOn") == 1f);
        //     }
        // }
    }
}

