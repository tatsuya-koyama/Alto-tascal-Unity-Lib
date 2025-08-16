using UnityEditor;
using UnityEngine;

namespace AltoLib
{
    public class AltoEditorWindow : EditorWindow
    {
        protected Color White      => Color.white;
        protected Color LightGray  => new(0.75f, 0.75f, 0.75f);
        protected Color Orange     => new(1.0f, 0.65f, 0.1f);

        protected Color _defaultLabelColor;
        protected Color _defaultValueColor;

        void OnEnable()
        {
            _defaultLabelColor = White;
            _defaultValueColor = White;
        }

        //----------------------------------------------------------------------------
        // 描画ヘルパー
        //----------------------------------------------------------------------------

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

        protected bool Button(string label, float minHeight = 30f)
        {
            return GUILayout.Button(label, GUILayout.MinHeight(minHeight));
        }

        protected bool ButtonMinW(string label, float minWidth = 60f, float minHeight = 30f)
        {
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
    }
}
