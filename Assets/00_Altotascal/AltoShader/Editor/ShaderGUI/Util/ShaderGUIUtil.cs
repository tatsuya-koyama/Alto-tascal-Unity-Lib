﻿using UnityEditor;
using UnityEngine;

namespace AltoLib.ShaderGUI
{
    public class ShaderGUIUtil
    {
        ICustomProperties _customProperties;

        public ShaderGUIUtil(ICustomProperties customProperties)
        {
            _customProperties = customProperties;
        }

        public float DrawFloat(string label, string propName, float factor = 1.0f)
        {
            EditorGUI.BeginChangeCheck();
            var prop = GetProperty(propName);
            float value = prop.floatValue / factor;

            MaterialEditor.BeginProperty(prop);
            value = EditorGUILayout.FloatField(label, value);
            MaterialEditor.EndProperty();

            if (EditorGUI.EndChangeCheck())
            {
                prop.floatValue = value * factor;
            }
            return value;
        }

        public float DrawSlider(string label, string propName, float min, float max, float factor = 1.0f)
        {
            EditorGUI.BeginChangeCheck();
            var prop = GetProperty(propName);
            float value = prop.floatValue / factor;

            MaterialEditor.BeginProperty(prop);
            value = EditorGUILayout.Slider(label, value, min, max);
            MaterialEditor.EndProperty();

            if (EditorGUI.EndChangeCheck())
            {
                prop.floatValue = value * factor;
            }
            return value;
        }

        public bool DrawToggle(string label, string propName)
        {
            EditorGUI.BeginChangeCheck();
            var prop = GetProperty(propName);

            MaterialEditor.BeginProperty(prop);
            bool isOn = EditorGUILayout.Toggle(label, prop.floatValue == 1.0f);
            MaterialEditor.EndProperty();

            if (EditorGUI.EndChangeCheck())
            {
                prop.floatValue = isOn ? 1.0f : 0f;
            }
            return isOn;
        }

        public Vector2 DrawVector2(string label, string propName)
        {
            EditorGUI.BeginChangeCheck();
            var prop = GetProperty(propName);
            Vector4 value = prop.vectorValue;

            MaterialEditor.BeginProperty(prop);
            value = EditorGUILayout.Vector2Field(label, value);
            MaterialEditor.EndProperty();

            if (EditorGUI.EndChangeCheck())
            {
                prop.vectorValue = value;
            }
            return value;
        }

        public Vector3 DrawVector3(string label, string propName)
        {
            EditorGUI.BeginChangeCheck();
            var prop = GetProperty(propName);
            Vector4 value = prop.vectorValue;

            MaterialEditor.BeginProperty(prop);
            value = EditorGUILayout.Vector3Field(label, value);
            MaterialEditor.EndProperty();

            if (EditorGUI.EndChangeCheck())
            {
                prop.vectorValue = value;
            }
            return value;
        }

        public Vector4 DrawVector4(string label, string propName)
        {
            EditorGUI.BeginChangeCheck();
            var prop = GetProperty(propName);
            Vector4 value = prop.vectorValue;

            MaterialEditor.BeginProperty(prop);
            value = EditorGUILayout.Vector4Field(label, value);
            MaterialEditor.EndProperty();

            if (EditorGUI.EndChangeCheck())
            {
                prop.vectorValue = value;
            }
            return value;
        }

        public Vector4 DrawVector4AsSliders(
            string propName,
            string label1, float min1, float max1,
            string label2, float min2, float max2,
            string label3, float min3, float max3,
            string label4, float min4, float max4
        )
        {
            EditorGUI.BeginChangeCheck();
            var prop = GetProperty(propName);
            Vector4 value = prop.vectorValue;

            MaterialEditor.BeginProperty(prop);
            value.x = EditorGUILayout.Slider(label1, value.x, min1, max1);
            value.y = EditorGUILayout.Slider(label2, value.y, min2, max2);
            value.z = EditorGUILayout.Slider(label3, value.z, min3, max3);
            value.w = EditorGUILayout.Slider(label4, value.w, min4, max4);
            MaterialEditor.EndProperty();

            if (EditorGUI.EndChangeCheck())
            {
                prop.vectorValue = value;
            }
            return value;
        }

        MaterialProperty GetProperty(string propName)
        {
            MaterialProperty prop = null;
            try
            {
                prop = _customProperties[propName] as MaterialProperty;
            }
            catch
            {
                Debug.Log($"<color=#ff0099>Exception by prop name : </color>{ propName }");
            }
            return prop;
        }

        /// <summary>
        /// Shuriken の Inspector で使われているようなバーっぽい見た目の Foldout
        /// </summary>
        public bool Foldout(bool isShown, string title)
        {
            var style = new GUIStyle("ShurikenModuleTitle");
            style.font = new GUIStyle(EditorStyles.label).font;
            style.fontStyle = FontStyle.Bold;
            style.border = new RectOffset(15, 7, 4, 4);
            style.fixedHeight = 22;
            style.margin.top = 6;
            style.contentOffset = new Vector2(20f, -2f);

            var rect = GUILayoutUtility.GetRect(16f, 22f, style);
            GUI.Box(rect, title, style);

            var e = Event.current;
            var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
            if (e.type == EventType.Repaint)
            {
                EditorStyles.foldout.Draw(toggleRect, false, false, isShown, false);
            }

            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                isShown = !isShown;
                e.Use();
            }
            return isShown;
        }

        public Texture2D MakeGradientTexture(Color color1, Color color2, int size=50)
        {
            Texture2D texture;
            Color[] colors = new Color[size];

            for (int i = 0; i < size; ++i)
            {
                colors[i] = Color.Lerp(color2, color1, (float)i / (size - 1));
            }
            texture = new Texture2D(1, size);
            texture.SetPixels(colors);
            texture.Apply();
            return texture;
        }
    }
}
