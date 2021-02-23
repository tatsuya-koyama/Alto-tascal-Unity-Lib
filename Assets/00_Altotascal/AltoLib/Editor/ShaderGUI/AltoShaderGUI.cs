using UnityEditor;
using UnityEngine;

namespace AltoLib.ShaderGUI
{
    public class AltoShaderGUI : CubicColorGUI
    {
        class CustomProperties : ICustomProperties
        {
            public MaterialProperty billboardOn;

            public MaterialProperty mirageOn;
            public MaterialProperty mirage1;
            public MaterialProperty mirage2;

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
            public MaterialProperty ditherCull;

            public MaterialProperty windStrength;
            public MaterialProperty windSpeed;
            public MaterialProperty windBigWave;
            public MaterialProperty windNoise;
            public MaterialProperty windPhaseShift;
            public MaterialProperty windRotateSpeed;
            public MaterialProperty windBaseAngle;

            public CustomProperties(MaterialProperty[] properties)
            {
                billboardOn              = BaseShaderGUI.FindProperty("_BillboardOn", properties);

                mirageOn                 = BaseShaderGUI.FindProperty("_MirageOn", properties);
                mirage1                  = BaseShaderGUI.FindProperty("_Mirage1", properties);
                mirage2                  = BaseShaderGUI.FindProperty("_Mirage2", properties);

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
                ditherCull               = BaseShaderGUI.FindProperty("_DitherCull", properties);

                windStrength             = BaseShaderGUI.FindProperty("_WindStrength", properties);
                windSpeed                = BaseShaderGUI.FindProperty("_WindSpeed", properties);
                windBigWave              = BaseShaderGUI.FindProperty("_WindBigWave", properties);
                windNoise                = BaseShaderGUI.FindProperty("_WindNoise", properties);
                windPhaseShift           = BaseShaderGUI.FindProperty("_WindPhaseShift", properties);
                windRotateSpeed          = BaseShaderGUI.FindProperty("_WindRotateSpeed", properties);
                windBaseAngle            = BaseShaderGUI.FindProperty("_WindBaseAngle", properties);
            }

            public object this[string propertyName]
            {
                get { return typeof(CustomProperties).GetField(propertyName).GetValue(this); }

                set { typeof(CustomProperties).GetField(propertyName).SetValue(this, value); }
            }
        }
        CustomProperties _customProperties;
        ShaderGUIUtil _util;
        bool _showMirageProps   = true;
        bool _showDissolveProps = true;
        bool _showDitherProps   = true;
        bool _showWindProps     = true;

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
            base.DrawCustomPropAtBottom(material);
            DrawMirageProps();
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
            _util.DrawSlider("Dithering Cull", "ditherCull", 0f, 20f);
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

        void DrawMirageProps()
        {
            _showMirageProps = _util.Foldout(_showMirageProps, "Mirage Effect");
            if (!_showMirageProps) { return; }

            bool mirageOn = _util.DrawToggle("Mirage Effect", "mirageOn");
            EditorGUI.BeginDisabledGroup(!mirageOn);
            {
                _util.DrawSlider("Mirage 1", "mirage1", 0f, 200f);
                _util.DrawSlider("Mirage 2", "mirage2", 0f, 200f);
            }
            EditorGUI.EndDisabledGroup();
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
