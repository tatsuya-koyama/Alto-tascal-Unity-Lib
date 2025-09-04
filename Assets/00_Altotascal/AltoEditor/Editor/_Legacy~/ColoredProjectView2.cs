#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AltoEditor
{
    public static class ColoredProjectView2
    {
        const string MenuPath = AltoMenuPath.EditorExt + "ColoredProjectView (Block)";

        static Dictionary<string, int> _colorMap = new Dictionary<string, int>();
        static int _idCount;

        [MenuItem(MenuPath)]
        private static void ToggleEnabled()
        {
            Menu.SetChecked(MenuPath, !Menu.GetChecked(MenuPath));
            _colorMap.Clear();
            _idCount = 0;
        }

        [InitializeOnLoadMethod]
        private static void SetEvent()
        {
            EditorApplication.projectWindowItemOnGUI += OnGUI;
        }

        private static void OnGUI(string guid, Rect selectionRect)
        {
            if (!Menu.GetChecked(MenuPath))
            {
                return;
            }

            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string[] folderNames = assetPath.Split('/');
            int basePathLevel = 1;
            string basePath = (folderNames.Length > basePathLevel) ? folderNames[basePathLevel] : "";
            int pathLevel = CountWord(assetPath, "/");

            int colorId = 0;
            if (!_colorMap.TryGetValue(basePath, out colorId))
            {
                _colorMap.Add(basePath, _idCount);
                ++_idCount;
            }

            var originalColor = GUI.color;
            GUI.color = GetColor(colorId, pathLevel);
            GUI.Box(selectionRect, string.Empty);
            GUI.color = originalColor;
        }

        private static int CountWord(string source, string word)
        {
            return source.Length - source.Replace(word, "").Length;
        }

        private static Color GetColor(int id, int pathLevel)
        {
            Color color = (EditorGUIUtility.isProSkin)
                ? GetColorForDarkSkin(id)
                : GetColorForLightSkin(id);

            float alphaFactor = 1.0f - (pathLevel - 1) * 0.25f;
            alphaFactor = Mathf.Clamp(alphaFactor, 0, 1f);
            color.a *= alphaFactor;
            return color;
        }

        private static Color GetColorForDarkSkin(int id)
        {
            switch (id % 7)
            {
                case 0: return new Color(8.4f, 8.4f, 0.0f, 0.45f);
                case 1: return new Color(9.6f, 0.5f, 0.5f, 0.50f);
                case 2: return new Color(0.0f, 9.6f, 0.0f, 0.40f);
                case 3: return new Color(4.8f, 0.0f, 8.4f, 0.50f);
                case 4: return new Color(9.6f, 3.0f, 0.0f, 0.50f);
                case 5: return new Color(0.0f, 4.8f, 9.6f, 0.40f);
                case 6: return new Color(4.0f, 4.0f, 4.0f, 0.50f);
            }
            return new Color(0, 0, 0, 0);
        }

        private static Color GetColorForLightSkin(int id)
        {
            switch (id % 7)
            {
                case 0: return new Color(1.4f, 1.4f, 0.0f, 0.15f);
                case 1: return new Color(1.6f, 0.0f, 0.0f, 0.15f);
                case 2: return new Color(0.0f, 1.6f, 0.0f, 0.15f);
                case 3: return new Color(0.8f, 0.0f, 1.4f, 0.15f);
                case 4: return new Color(1.6f, 0.5f, 0.0f, 0.15f);
                case 5: return new Color(0.0f, 0.8f, 1.6f, 0.15f);
                case 6: return new Color(0.0f, 0.0f, 0.0f, 0.15f);
            }
            return new Color(0, 0, 0, 0);
        }
    }
}
#endif
