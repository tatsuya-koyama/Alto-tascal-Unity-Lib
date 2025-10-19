using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace AltoEditor
{
    public class CustomUnityStatsWindow : AltoEditorWindow
    {
        [MenuItem(AltoMenuPath.EditorWindow + "Performance Stats Window")]
        static void ShowWindow()
        {
            GetWindow<CustomUnityStatsWindow>("Stats");
        }

        Vector2 _scrollView;
        bool _autoRefresh = true;
        bool _showDetails = true;
        bool _showGauge = true;
        bool _showSettings = true;
        double _nextRepaintAt;

        int _frameTimeMax = 100;
        int _trisMax = 300000;
        int _batchMax = 300;
        int _texMemoryMax = 500;
        int _miscCountMax = 100;
        int _memoryGaugeMax = 2000;

        const string UnitColor = "<color=#77c030>";

        //----------------------------------------------------------------------
        // Auto Refresh
        //----------------------------------------------------------------------

        const float RefreshInterval = 0.25f;

        void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        void OnEditorUpdate()
        {
            double currentTime = EditorApplication.timeSinceStartup;
            if (_autoRefresh && currentTime >= _nextRepaintAt)
            {
                Repaint();
                _nextRepaintAt = currentTime + RefreshInterval;
            }
        }

        //----------------------------------------------------------------------
        // Draw GUI
        //----------------------------------------------------------------------

        void OnGUI()
        {
            _scrollView = GUILayout.BeginScrollView(_scrollView);
            {
                using (new GUILayout.HorizontalScope())
                {
                    _showDetails = GUILayout.Toggle(_showDetails, "Show Details");
                    _showGauge = GUILayout.Toggle(_showGauge, "Show Gauge");
                    GUILayout.FlexibleSpace();
                    _autoRefresh = GUILayout.Toggle(_autoRefresh, "Auto Refresh");
                }
                DrawRenderingStats();
                DrawMemoryUsage();
                DrawParams();
            }
            GUILayout.EndScrollView();
        }

        void DrawRenderingStats()
        {
            this._defaultLabelColor = this.Orange;

            float fps = 1 / UnityStats.frameTime;
            int fpsMax = Application.targetFrameRate;
            LabelWithGauge("FPS", $"{Num(fps)} / {fpsMax}", fps, fpsMax, DarkAqua);

            float frameTimeMSec = UnityStats.frameTime * 1000;
            float renderTimeMSec = UnityStats.renderTime * 1000;
            LabelWithGauge("Frame Time", $"{Num(frameTimeMSec)} {UnitColor}ms</color>", frameTimeMSec, _frameTimeMax);
            LabelWithGauge("Render Time", $"{Num(renderTimeMSec)} {UnitColor}ms</color>", renderTimeMSec, _frameTimeMax);

            HR();
            float tris = UnityStats.triangles;
            float verts = UnityStats.vertices;
            LabelWithGauge("Tris", $"{Num(tris / 10000f)} {UnitColor}万</color>", tris, _trisMax);
            LabelWithGauge("Verts", $"{Num(verts / 10000f)} {UnitColor}万</color>", verts, _trisMax);

            if (_showDetails)
            {
                LabelWithGauge("Draw Calls", $"{UnityStats.drawCalls}", UnityStats.drawCalls, _batchMax, DarkYellow);
                LabelWithGauge("Set Pass Calls", $"{UnityStats.setPassCalls}", UnityStats.setPassCalls, _batchMax, DarkYellow);
            }

            LabelWithGauge("Batches", $"{UnityStats.batches}", UnityStats.batches, _batchMax, DarkYellow);
            int savedByBatching = (UnityStats.staticBatchedDrawCalls - UnityStats.staticBatches)
                                + (UnityStats.dynamicBatchedDrawCalls - UnityStats.dynamicBatches)
                                + (UnityStats.instancedBatchedDrawCalls - UnityStats.instancedBatches);
            LabelWithGauge("Saved by Batching", $"{savedByBatching}", savedByBatching, _batchMax, DarkAqua);

            if (_showDetails)
            {
                Label("Static Batches / Calls", $"{UnityStats.staticBatches} / {UnityStats.staticBatchedDrawCalls}", LightGray);
                Label("Dynamic Batches / Calls", $"{UnityStats.dynamicBatches} / {UnityStats.dynamicBatchedDrawCalls}", LightGray);
                Label("Instanced Batches / Calls", $"{UnityStats.instancedBatches} / {UnityStats.instancedBatchedDrawCalls}", LightGray);
            }

            HR();
            float texBytes = UnityStats.renderTextureBytes / 1024f / 1024f;
            LabelWithGauge("Texture Bytes", $"{Num(texBytes)} {UnitColor}MB</color>", texBytes, _texMemoryMax);

            if (_showDetails)
            {
                LabelWithGauge("Texture Count / Changes", $"{UnityStats.renderTextureCount} / {UnityStats.renderTextureChanges}", UnityStats.renderTextureCount, _miscCountMax, DarkOrange);
                LabelWithGauge("Shadow Casters", $"{UnityStats.shadowCasters}", UnityStats.shadowCasters, _miscCountMax, DarkOrange);
                LabelWithGauge("Animator Playing", $"{UnityStats.animatorComponentsPlaying}", UnityStats.animatorComponentsPlaying, _miscCountMax, DarkOrange);
                LabelWithGauge("Skinned Meshes", $"{UnityStats.visibleSkinnedMeshes}", UnityStats.visibleSkinnedMeshes, _miscCountMax, DarkOrange);

                string resolution = UnityStats.screenRes.Replace("x", " x ");
                Label("Screen Resolution", $"{resolution}", LightGray);
                Label("Screen Bytes", $"{Num(UnityStats.screenBytes / 1024f / 1024f)} {UnitColor}MB<color>", LightGray);
            }
        }

        void LabelWithGauge(
          string label, string valueText, float value, float valueMax, Color? _color = null
        )
        {
            Color color = _color ?? DarkLGreen;

            using (new GUILayout.HorizontalScope())
            {
                float fps = 1 / UnityStats.frameTime;
                int fpsMax = Application.targetFrameRate;
                Label(label, valueText, valueWidth: 100f, withoutScope: true);
                if (_showGauge)
                {
                    Gauge(value / valueMax, _fgColor: color);
                }
            }
        }

        void DrawMemoryUsage()
        {
            Header("Memory Usage");
            this._defaultLabelColor = new Color(0.57f, 0.82f, 0.2f, 1f);
            MemoryRow("Total Reserved", Profiler.GetTotalReservedMemoryLong());
            MemoryRow("Total Allocated", Profiler.GetTotalAllocatedMemoryLong());
            MemoryRow("Graphics", Profiler.GetAllocatedMemoryForGraphicsDriver());
            MemoryRow("Mono Heap", Profiler.GetMonoHeapSizeLong());
            MemoryRow("Mono Used", Profiler.GetMonoUsedSizeLong());
        }

        void MemoryRow(string label, long bytes)
        {
            using (new GUILayout.HorizontalScope())
            {
                float value = bytes / 1024f / 1024f;
                Label(label, $"{Num(value)} {UnitColor}MB</color>", valueWidth: 100f, withoutScope: true);
                if (_showGauge)
                {
                    Gauge(value / _memoryGaugeMax);
                }
            }
        }

        void DrawParams()
        {
            BR();
            HR();
            using (new GUILayout.HorizontalScope())
            {
                Header("Bar Max Settings");
                GUILayout.FlexibleSpace();
                _showSettings = GUILayout.Toggle(_showSettings, "Show");
            }

            if (_showSettings)
            {
                _frameTimeMax   = EditorGUILayout.IntField("Frame Time [MSec]", _frameTimeMax);
                _trisMax        = EditorGUILayout.IntField("Tris / Verts", _trisMax);
                _batchMax       = EditorGUILayout.IntField("Batches / Draw Calls", _batchMax);
                _texMemoryMax   = EditorGUILayout.IntField("Texture Bytes [MB]", _texMemoryMax);
                _miscCountMax   = EditorGUILayout.IntField("Misc (Orange) Counts", _miscCountMax);
                _memoryGaugeMax = EditorGUILayout.IntField("Memory [MB]", _memoryGaugeMax);
            }
        }

        /// <summary>
        /// 数値を小数点以下 2 位までの見やすい文字列に変換
        /// （小数点以下は暗い色で表示）
        /// </summary>
        string Num(float num)
        {
            string numStr = num.ToString("F2");
            if (!numStr.Contains(".")) { return numStr; }
            return numStr.Replace(".", "<color=#94A1AF>.") + "</color>";
        }
    }
}
