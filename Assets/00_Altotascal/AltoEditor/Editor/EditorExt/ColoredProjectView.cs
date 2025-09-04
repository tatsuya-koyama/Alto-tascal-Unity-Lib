#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ColorSettings = AltoEditor.ColoredProjectViewConfig.ColorSettings;

namespace AltoEditor
{
    public static class ColoredProjectView
    {
        const string MenuPath = AltoMenuPath.EditorExt + "ColoredProjectView";

        public static bool NeedToReloadSettings = false;

        [MenuItem(MenuPath)]
        static void ToggleEnabled()
        {
            Menu.SetChecked(MenuPath, !Menu.GetChecked(MenuPath));
        }


        [InitializeOnLoadMethod]
        static void SetEvent()
        {
            SetUpSettings();
            EditorApplication.projectWindowItemOnGUI += OnGUI;
            EditorSceneManager.sceneOpened += OnSceneOpened;
        }

        static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            // Scene 切り替えで Texture が破棄されてしまうようなので作り直す
            SetUpSettings();
        }

        //----------------------------------------------------------------------
        // Data Model
        //----------------------------------------------------------------------

        static ColorSettings _settings;
        static Dictionary<string, Texture2D> _textureCache = new();

        static void SetUpSettings()
        {
            NeedToReloadSettings = false;
            CleanUpTextures();
            _settings = ColoredProjectViewConfig.GetColorSettings();
            if (_settings == null) { return; }

            foreach (var record in _settings.records)
            {
                if (string.IsNullOrEmpty(record.pattern)) { continue; }
                if (_textureCache.ContainsKey(record.pattern)) { continue; }

                Color color1   = record.GetColor1();
                Color color2   = record.GetColor2();
                string pattern = record.pattern.ToLower();
                if (record.depth > 0) { pattern = $"{pattern}-{record.depth}"; }

                var texture = MakeGradientTexture(color1, color2);
                _textureCache.Add(pattern, texture);
            }
        }

        static void CleanUpTextures()
        {
            foreach (var pair in _textureCache)
            {
                GameObject.DestroyImmediate(pair.Value);
            }
            _textureCache.Clear();
        }

        //----------------------------------------------------------------------
        // Draw GUI
        //----------------------------------------------------------------------

        static void OnGUI(string guid, Rect selectionRect)
        {
            if (!Menu.GetChecked(MenuPath)) { return; }

            if (NeedToReloadSettings) { SetUpSettings(); }
            if (_settings == null) { return; }

            var texture = GetColorTexture(guid);
            if (texture != null)
            {
                GUI.DrawTexture(selectionRect, texture);
            }
        }

        static Texture2D GetColorTexture(string guid)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var depth = CountWord(assetPath, "/");  // root depth is 1

            var depthTex = GetColorTextureForSpecificDepth(assetPath, depth);
            if (depthTex != null) { return depthTex; }

            string[] assetNames = assetPath.Split('/');
            var patterns = _textureCache.Keys;
            foreach (string assetName in assetNames)
            {
                var lowerName = assetName.ToLower();
                foreach (var pattern in patterns)
                {
                    if (lowerName.StartsWith(pattern))
                    {
                        return _textureCache[pattern];
                    }
                }
            }
            return null;
        }

        static Texture2D GetColorTextureForSpecificDepth(string assetPath, int depth)
        {
            string[] assetNames = assetPath.Split('/');
            string targetName = assetNames.ElementAtOrDefault(depth);
            if (targetName == null) { return null; }

            foreach (var record in _settings.records)
            {
                if (string.IsNullOrEmpty(record.pattern)) { continue; }
                if (record.depth <= 0) { continue; }
                if (record.depth != depth) { continue; }

                var lowerName = targetName.ToLower();
                string pattern = record.pattern.ToLower();
                if (pattern == "*" || lowerName.StartsWith(pattern))
                {
                    return _textureCache[$"{pattern}-{depth}"];
                }
            }
            return null;
        }

        static int CountWord(string source, string word)
        {
            return source.Length - source.Replace(word, "").Length;
        }

        static Texture2D MakeGradientTexture(Color color1, Color color2, int size = 50)
        {
            Texture2D texture;
            Color[] colors = new Color[size];

            for (int i = 0; i < size; ++i)
            {
                colors[i] = Color.Lerp(color1, color2, (float)i / (size - 1));
            }
            texture = new Texture2D(size, 1);
            texture.SetPixels(colors);
            texture.Apply();
            return texture;
        }
    }
}
#endif
