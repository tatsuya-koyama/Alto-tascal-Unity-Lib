using UnityEditor;
using UnityEngine;

namespace AltoEditor
{
    public class AltoEditorWindow : EditorWindow
    {
        protected Color White       => Color.white;
        protected Color Gray        => new(0.5f, 0.5f, 0.5f);
        protected Color Orange      => new(1.0f, 0.65f, 0.1f);
        protected Color Yellow      => new(1.0f, 1.0f, 0.2f);
        protected Color Red         => new(1.0f, 0.2f, 0.2f);
        protected Color Green       => new(0.2f, 1.0f, 0.2f);
        protected Color Blue        => new(0.2f, 0.3f, 1.0f);
        protected Color LightGray   => new(0.75f, 0.75f, 0.75f);
        protected Color LightRed    => new(1.0f, 0.65f, 0.65f);
        protected Color LightGreen  => new(0.5f, 1.0f, 0.5f);
        protected Color LightBlue   => new(0.65f, 0.85f, 1.0f);
        protected Color LightYellow => new(1.0f, 1.0f, 0.5f);
        protected Color DarkGray    => new(0.35f, 0.35f, 0.35f);
        protected Color DarkYellow  => new(0.75f, 0.75f, 0.2f);
        protected Color DarkOrange  => new(0.75f, 0.55f, 0.25f);
        protected Color DarkAqua    => new(0.3f, 0.65f, 0.75f);
        protected Color DarkGreen   => new(0.1f, 0.65f, 0.1f);
        protected Color DarkLGreen  => new(0.53f, 0.69f, 0.26f);

        protected Color _defaultLabelColor = Color.white;
        protected Color _defaultValueColor = Color.white;
        protected float _defaultButtonMaxWidth  = 60f;
        protected float _defaultButtonMinHeight = 30f;
        protected float _defaultSpacing = 10f;

        void OnEnable()
        {
            _defaultLabelColor = White;
            _defaultValueColor = White;
        }

        //----------------------------------------------------------------------------
        // Save / Load Helpder
        //----------------------------------------------------------------------------

        protected virtual string SaveKey => $"AltoEditor-{GetType().Name}";

        protected virtual void SaveSettings(object obj)
        {
            string json = JsonUtility.ToJson(obj);
            EditorUserSettings.SetConfigValue(SaveKey, json);
        }

        protected T LoadSettings<T>()
        {
            var json = EditorUserSettings.GetConfigValue(SaveKey);
            if (json == null) { return default; }

            var settings = JsonUtility.FromJson<T>(json);
            if (settings == null)
            {
                Debug.LogError("Settings Load Error");
                return default;
            }
            return settings;
        }

        //----------------------------------------------------------------------------
        // Button Helper
        //----------------------------------------------------------------------------

        protected float _prevButtonMaxWidth;

        protected void ChangeDefaultButtonMaxWidth(float width)
        {
          _prevButtonMaxWidth = _defaultButtonMaxWidth;
          _defaultButtonMaxWidth = width;
        }

        protected void RevertDefaultButtonMaxWidth()
        {
          _defaultButtonMaxWidth = _prevButtonMaxWidth;
        }

        protected bool Button(
            string label, float maxWidth = 0f, float minHeight = 0f,
            string tooltip = "",
            Color? color = null
        )
        {
            Color originalColor = GUI.color;
            Color specifiedColor = color ?? Color.white;
            if (color != null)
            {
                GUI.color = specifiedColor;
            }

            if (maxWidth  == 0f) { maxWidth  = _defaultButtonMaxWidth; }
            if (minHeight == 0f) { minHeight = _defaultButtonMinHeight; }
            var content = new GUIContent(label, tooltip);
            bool pressed = GUILayout.Button(content, GUILayout.MaxWidth(maxWidth), GUILayout.MinHeight(minHeight));

            if (color != null)
            {
                GUI.color = originalColor;
            }
            return pressed;
        }

        //----------------------------------------------------------------------------
        // Rendering Helper
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
            float labelWidth = 150f,
            float? valueWidth = null,
            bool withoutScope = false
        )
        {
            Color labelColor = _labelColor ?? _defaultLabelColor;
            Color valueColor = _valueColor ?? _defaultValueColor;

            if (!withoutScope) { GUILayout.BeginHorizontal(); }
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

                GUILayout.Label(label, labelStyle, GUILayout.Width(labelWidth));
                GUILayout.Label("", GUILayout.Width(1f));

                if (valueWidth != null) {
                  GUILayout.Label(value, valueStyle, GUILayout.Width(valueWidth ?? 0f));
                } else {
                  GUILayout.Label(value, valueStyle);
                }
            }
            if (!withoutScope) { GUILayout.EndHorizontal(); }
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

        protected void Gauge(
          float ratio, string label = "", float height = 16f,
          Color? _fgColor = null, Color? _bgColor = null
        )
        {
            var fgColor = _fgColor ?? DarkLGreen;
            var bgColor = _bgColor ?? DarkGray;

            // 背景
            Rect rect = GUILayoutUtility.GetRect(16, height, "TextField");
            EditorGUI.DrawRect(rect, bgColor);

            // ゲージ部分
            Rect fill = new(
                rect.x + 1f,
                rect.y + 1f,
                rect.width * Mathf.Clamp01(ratio) - 2f,
                rect.height - 2f
            );
            EditorGUI.DrawRect(fill, fgColor);

            // テキスト
            GUIStyle style = new(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            EditorGUI.LabelField(rect, label, style);
        }
    }
}
