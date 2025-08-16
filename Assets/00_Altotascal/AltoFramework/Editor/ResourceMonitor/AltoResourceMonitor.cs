#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Profiling;

namespace AltoFramework.Editor
{
    /// <summary>
    ///   Alto フレームワークがロード中のリソース一覧をモニタするエディタ拡張
    /// </summary>
    public class AltoResourceMonitor : EditorWindow
    {
        [SerializeField] TreeViewState _treeViewState;
        AltoResourceTreeView _treeView;

        [MenuItem("Alto/00. Alto Framework/Alto Resource Monitor")]
        static void ShowWindow()
        {
            var window = EditorWindow.GetWindow<AltoResourceMonitor>("Alto-Resource");
            window.Show();
        }

        void OnEnable()
        {
            if (_treeViewState == null)
            {
                _treeViewState = new TreeViewState();
            }
            _treeView = new AltoResourceTreeView(_treeViewState);
        }

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("All"))
            {
                _treeView.WatchAll();
            }
            if (GUILayout.Button("Prefab"))
            {
                _treeView.WatchGameObjects();
            }
            if (GUILayout.Button("Scriptable Object"))
            {
                _treeView.WatchScriptableObjects();
            }
            if (GUILayout.Button("Sprite Atlas"))
            {
                _treeView.WatchSpriteAtlas();
            }
            if (GUILayout.Button("Audio Clip"))
            {
                _treeView.WatchAudioClips();
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Label(GetMemoryUsage());

            var treeRect = EditorGUILayout.GetControlRect(new GUILayoutOption[]
            {
                GUILayout.ExpandHeight(true),
                GUILayout.ExpandWidth(true)
            });
            _treeView.OnGUI(treeRect);
        }

        string GetMemoryUsage()
        {
            float totalMemory  = Profiler.GetTotalReservedMemoryLong()  / 1024f / 1024f;
            float usedMemory   = Profiler.GetTotalAllocatedMemoryLong() / 1024f / 1024f;
            float altoMemory   = _treeView.TotalMemory()                / 1024f / 1024f;

            return $"[Total Reserved Memory] : {totalMemory.ToString("0.0")} MB  "
                 + $"[Used] : {usedMemory.ToString("0.0")} MB  "
                 + $"[Alto] : {altoMemory.ToString("0.0")} MB";
        }
    }
}
#endif
