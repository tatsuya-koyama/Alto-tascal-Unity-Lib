#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AltoLib
{
    public static class ColoredProjectView
    {
        const string MenuPath = AltoMenuPath.EditorExt + "ColoredProjectView (Depth Level)";

        [MenuItem(MenuPath)]
        private static void ToggleEnabled()
        {
            Menu.SetChecked(MenuPath, !Menu.GetChecked(MenuPath));
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

            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var pathLevel = CountWord(assetPath, "/");

            var originalColor = GUI.color;
            GUI.color = GetColorByPathLevel(pathLevel);
            GUI.Box(selectionRect, string.Empty);
            GUI.color = originalColor;
        }

        private static int CountWord(string source, string word)
        {
            return source.Length - source.Replace(word, "").Length;
        }

        private static Color GetColorByPathLevel(int pathLevel)
        {
            return (EditorGUIUtility.isProSkin)
                ? GetColorForDarkSkin(pathLevel)
                : GetColorForLightSkin(pathLevel);
        }

        private static Color GetColorForDarkSkin(int pathLevel)
        {
            switch (pathLevel)
            {
                case 1: return new Color(8.4f, 8.4f, 0.0f, 0.45f);
                case 2: return new Color(9.6f, 0.5f, 0.5f, 0.45f);
                case 3: return new Color(0.0f, 9.6f, 0.0f, 0.40f);
                case 4: return new Color(4.8f, 0.0f, 8.4f, 0.50f);
                case 5: return new Color(0.0f, 4.8f, 9.6f, 0.40f);
                case 6: return new Color(0.0f, 0.0f, 0.0f, 0.40f);
            }
            return new Color(0, 0, 0, 0);
        }

        private static Color GetColorForLightSkin(int pathLevel)
        {
            switch (pathLevel)
            {
                case 1: return new Color(1.4f, 1.4f, 0.0f, 0.1f);
                case 2: return new Color(1.6f, 0.0f, 0.0f, 0.1f);
                case 3: return new Color(0.0f, 1.6f, 0.0f, 0.1f);
                case 4: return new Color(0.8f, 0.0f, 1.4f, 0.1f);
                case 5: return new Color(0.0f, 0.8f, 1.6f, 0.1f);
                case 6: return new Color(0.0f, 0.0f, 0.0f, 0.1f);
            }
            return new Color(0, 0, 0, 0);
        }
    }
}
#endif
