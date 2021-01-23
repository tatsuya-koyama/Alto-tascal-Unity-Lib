using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace AltoLib
{
    public class FavoritesWindow : EditorWindow
    {
        [MenuItem("Alto/Favorites Window")]
        static void ShowWindow()
        {
            GetWindow<FavoritesWindow>("★ Favorites");
        }

        Vector2 _scrollView;
        AssetInfo _lastOpenedAsset;

        //----------------------------------------------------------------------
        // Data structure
        //----------------------------------------------------------------------

        [System.Serializable]
        class AssetInfo
        {
            public string guid;
            public string path;
            public string name;
            public string type;
        }

        [System.Serializable]
        class AssetInfoList
        {
            public List<AssetInfo> infoList = new List<AssetInfo>();
        }

        [SerializeField] static AssetInfoList _assetsCache = null;
        static AssetInfoList _assets
        {
            get
            {
                if (_assetsCache == null)
                {
                    _assetsCache = LoadPrefs();
                }
                return _assetsCache;
            }
        }

        //----------------------------------------------------------------------
        // Save and Load
        //----------------------------------------------------------------------

        static string PrefsKey()
        {
            return $"{Application.productName}-Alto-Favs";
        }

        static void SavePrefs()
        {
            string prefsJson = JsonUtility.ToJson(_assets);
            EditorPrefs.SetString(PrefsKey(), prefsJson);
        }

        static AssetInfoList LoadPrefs()
        {
            Debug.Log("Loading Favorites Prefs...");
            string prefsKey = PrefsKey();
            if (!EditorPrefs.HasKey(prefsKey)) { return new AssetInfoList(); }

            string prefsJson = EditorPrefs.GetString(prefsKey);
            var assets = JsonUtility.FromJson<AssetInfoList>(prefsJson);
            if (assets == null)
            {
                Debug.LogError("Favorites Prefs Load Error");
                return new AssetInfoList();
            }

            return assets;
        }

        //----------------------------------------------------------------------
        // Draw GUI
        //----------------------------------------------------------------------

        void OnGUI()
        {
            GUILayout.BeginHorizontal();
            {
                var content = new GUIContent("★ Fav", "Bookmark selected asset");
                if (GUILayout.Button(content, GUILayout.Width(100), GUILayout.Height(40)))
                {
                    BookmarkAsset();
                }
                GUILayout.BeginVertical();
                {
                    if (GUILayout.Button("▼ Sort by Type", GUILayout.MaxWidth(200)))
                    {
                        SortByType();
                    }
                    if (GUILayout.Button("▼ Sort by Name", GUILayout.MaxWidth(200)))
                    {
                        SortByName();
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();

            _scrollView = GUILayout.BeginScrollView(_scrollView);
            {
                foreach (var info in _assets.infoList)
                {
                    GUILayout.BeginHorizontal();
                    {
                        bool isCanceled = DrawAssetRow(info);
                        if (isCanceled) { break; }
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndScrollView();
        }

        bool DrawAssetRow(AssetInfo info)
        {
            bool isCanceled = false;
            {
                var content = new GUIContent(" ◎ ", "Highlight asset");
                if (GUILayout.Button(content, GUILayout.ExpandWidth(false)))
                {
                    HighlightAsset(info);
                }
            }
            {
                DrawAssetItemButton(info);
            }
            {
                var content = new GUIContent("X", "Remove from Favs");
                if (GUILayout.Button(content, GUILayout.ExpandWidth(false)))
                {
                    RemoveAsset(info);
                    isCanceled = true;
                }
            }
            return isCanceled;
        }

        void DrawAssetItemButton(AssetInfo info)
        {
            var content = new GUIContent($" {info.name}", AssetDatabase.GetCachedIcon(info.path));
            var style = GUI.skin.button;
            var originalAlignment = style.alignment;
            var originalFontStyle = style.fontStyle;
            var originalTextColor = style.normal.textColor;
            style.alignment = TextAnchor.MiddleLeft;
            if (info == _lastOpenedAsset)
            {
                style.fontStyle = FontStyle.Bold;
                style.normal.textColor = Color.yellow;
            }

            float width = position.width - 70f;
            if (GUILayout.Button(content, style, GUILayout.MaxWidth(width), GUILayout.Height(18)))
            {
                OpenAsset(info);
            }

            style.alignment        = originalAlignment;
            style.fontStyle        = originalFontStyle;
            style.normal.textColor = originalTextColor;
        }

        //----------------------------------------------------------------------
        // Private logic
        //----------------------------------------------------------------------

        void BookmarkAsset()
        {
            foreach (string assetGuid in Selection.assetGUIDs)
            {
                if (_assets.infoList.Exists(x => x.guid == assetGuid)) { continue; }

                var info = new AssetInfo();
                info.guid = assetGuid;
                info.path = AssetDatabase.GUIDToAssetPath(assetGuid);
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(info.path);
                info.name = asset.name;
                info.type = asset.GetType().ToString();
                _assets.infoList.Add(info);
            }
            SavePrefs();
        }

        void RemoveAsset(AssetInfo info)
        {
            _assets.infoList.Remove(info);
            SavePrefs();
        }

        void HighlightAsset(AssetInfo info)
        {
            var asset = AssetDatabase.LoadAssetAtPath<Object>(info.path);
            EditorGUIUtility.PingObject(asset);
        }

        void OpenAsset(AssetInfo info)
        {
            // Mark last-opened non-folder asset
            if (info.type != "UnityEditor.DefaultAsset")
            {
                _lastOpenedAsset = info;
            }

            // Open scene asset
            if (Path.GetExtension(info.path).Equals(".unity"))
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(info.path, OpenSceneMode.Single);
                }
                return;
            }

            // Open other type asset
            var asset = AssetDatabase.LoadAssetAtPath<Object>(info.path);
            AssetDatabase.OpenAsset(asset);
        }

        void SortByType()
        {
            SortByName();
            _assets.infoList.Sort((a, b) => {
                return a.type.CompareTo(b.type);
            });
        }

        void SortByName()
        {
            _assets.infoList.Sort((a, b) => {
                return a.name.CompareTo(b.name);
            });
        }
    }
}
