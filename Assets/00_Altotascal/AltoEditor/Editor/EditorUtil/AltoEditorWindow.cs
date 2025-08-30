using UnityEditor;
using UnityEngine;

namespace AltoEditor
{
    public class AltoEditorWindow : EditorWindow
    {
        protected Color White      => Color.white;
        protected Color LightGray  => new(0.75f, 0.75f, 0.75f);
        protected Color Gray       => new(0.5f, 0.5f, 0.5f);
        protected Color Orange     => new(1.0f, 0.65f, 0.1f);
        protected Color Yellow     => new(1.0f, 1.0f, 0.2f);
        protected Color DarkYellow => new(0.75f, 0.75f, 0.2f);
        protected Color DarkOrange => new(0.75f, 0.55f, 0.25f);
        protected Color DarkAqua   => new(0.3f, 0.65f, 0.75f);

        protected Color _defaultLabelColor = Color.white;
        protected Color _defaultValueColor = Color.white;
        protected float _defaultButtonMinWidth  = 60f;
        protected float _defaultButtonMinHeight = 30f;
        protected float _defaultSpacing = 10f;

        void OnEnable()
        {
            _defaultLabelColor = White;
            _defaultValueColor = White;
        }

        //----------------------------------------------------------------------------
        // 描画ヘルパー
        //----------------------------------------------------------------------------

        protected void Header(
            string text, Color? _color = null, FontStyle fontStyle = FontStyle.Bold
        )
        {
            Color color = _color ?? DarkYellow;

            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = fontStyle,
                richText  = true,
            };
            labelStyle.normal.textColor = color;

            GUILayout.Label(text, labelStyle);
        }

        protected void Label(
            string label, string value,
            Color? _labelColor = null,
            Color? _valueColor = null,
            float width = 150f
        )
        {
            Color labelColor = _labelColor ?? _defaultLabelColor;
            Color valueColor = _valueColor ?? _defaultValueColor;

            using (new GUILayout.HorizontalScope())
            {
                var labelStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleRight,
                    richText = true,
                };
                labelStyle.normal.textColor = labelColor;

                var valueStyle = new GUIStyle(GUI.skin.label)
                {
                    richText = true,
                };
                valueStyle.normal.textColor = valueColor;

                GUILayout.Label(label, labelStyle, GUILayout.Width(width));
                GUILayout.Label("", GUILayout.Width(1f));
                GUILayout.Label(value, valueStyle);
            }
        }

        protected bool Button(string label, float minHeight = 0f)
        {
            if (minHeight == 0f) { minHeight = _defaultButtonMinHeight; }
            return GUILayout.Button(label, GUILayout.MinHeight(minHeight));
        }

        protected bool ButtonW(string label, float minWidth = 0f, float minHeight = 0f)
        {
            if (minWidth  == 0f) { minWidth  = _defaultButtonMinWidth; }
            if (minHeight == 0f) { minHeight = _defaultButtonMinHeight; }
            return GUILayout.Button(label, GUILayout.MaxWidth(minWidth), GUILayout.MinHeight(minHeight));
        }

        protected void BR()
        {
            EditorGUILayout.Space();
        }

        protected void HR()
        {
            EditorGUILayout.Space(2f);
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(2));
            EditorGUILayout.Space(2f);
        }

        protected void Spacing(float space = 0f)
        {
            if (space == 0f) { space = _defaultSpacing; }
            GUILayout.Space(space);
        }
    }
}
