using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace AltoLib.Editor
{
    public class MasterDataImporterWindow : EditorWindow
    {
        const float WindowMinWidth = 280f;
        const float WindowMinHeight = 320f;
        const float ContentMaxWidth = 300f;
        const float ContentHorizontalMargin = 24f;
        static readonly Color HeaderLabelColor = new(1.0f, 0.65f, 0.1f);
        static readonly Color CsvUpdatedStatusColor = new Color32(0x33, 0xee, 0x00, 0xff);

        class SheetState
        {
            public MasterDataSheetConfig config;
            public bool selected = true;
            public string status;
            public MessageType statusType = MessageType.None;
            public bool highlightCsvUpdated;
        }

        MasterDataImporterConfig _config;
        readonly List<SheetState> _sheetStates = new();
        Vector2 _scrollPosition;
        string _configError;
        bool _isImporting;

        public static void Open()
        {
            var window = GetWindow<MasterDataImporterWindow>("Master Data Importer");
            window.minSize = new Vector2(WindowMinWidth, WindowMinHeight);
            window.LoadConfig();
        }

        void OnEnable()
        {
            LoadConfig();
        }

        void OnGUI()
        {
            DrawHeader();

            if (_config == null)
            {
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(GetContentWidth())))
                {
                    EditorGUILayout.HelpBox(_configError, MessageType.Error);
                }
                DrawConfigHelp();
                return;
            }

            DrawActions();
            EditorGUILayout.Space(4f);
            DrawSheetList();
        }

        void DrawHeader()
        {
            var headerLabelStyle = new GUIStyle(EditorStyles.boldLabel);
            headerLabelStyle.normal.textColor = HeaderLabelColor;

            using (new EditorGUILayout.VerticalScope(GUILayout.Width(GetContentWidth())))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Config", headerLabelStyle);
                    using (new EditorGUI.DisabledScope(_isImporting))
                    {
                        if (GUILayout.Button("Reload", GUILayout.Width(70f)))
                        {
                            LoadConfig();
                        }
                    }
                }
                DrawPath(MasterDataImporterConfigLoader.ConfigPath);

                if (_config != null)
                {
                    EditorGUILayout.LabelField("CSV Output", headerLabelStyle);
                    DrawPath(_config.csvOutputDirectory);
                    EditorGUILayout.LabelField("Data Table Output", headerLabelStyle);
                    DrawPath(_config.dataTableOutputDirectory);
                }
            }
        }

        void DrawActions()
        {
            bool hasSelectedSheet = _sheetStates.Any(state => state.selected);
            using (new EditorGUI.DisabledScope(_isImporting))
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(GetContentWidth())))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Select All", GUILayout.Width(80f)))
                    {
                        SetAllSelected(true);
                    }
                    if (GUILayout.Button("Clear", GUILayout.Width(60f)))
                    {
                        SetAllSelected(false);
                    }
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(!hasSelectedSheet))
                    {
                        if (GUILayout.Button("Import Selected", GUILayout.Width(130f), GUILayout.Height(28f)))
                        {
                            StartImport(_sheetStates.Where(state => state.selected));
                        }
                    }
                    if (GUILayout.Button("Import All", GUILayout.Width(100f), GUILayout.Height(28f)))
                    {
                        StartImport(_sheetStates);
                    }
                }
            }
        }

        void DrawSheetList()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            foreach (var state in _sheetStates)
            {
                using (new EditorGUILayout.VerticalScope(
                    EditorStyles.helpBox,
                    GUILayout.Width(GetContentWidth())
                ))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        using (new EditorGUI.DisabledScope(_isImporting))
                        {
                            state.selected = EditorGUILayout.Toggle(
                                state.selected,
                                GUILayout.Width(18f)
                            );
                        }

                        EditorGUILayout.LabelField(
                            new GUIContent(state.config.name, state.config.name),
                            EditorStyles.boldLabel,
                            GUILayout.MinWidth(0f),
                            GUILayout.ExpandWidth(true)
                        );

                        using (new EditorGUI.DisabledScope(_isImporting))
                        {
                            if (GUILayout.Button("Import", GUILayout.Width(70f)))
                            {
                                StartImport(new[] { state });
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(state.status))
                    {
                        DrawStatus(state);
                    }
                }
            }
            EditorGUILayout.EndScrollView();
        }

        void DrawStatus(SheetState state)
        {
            if (!state.highlightCsvUpdated)
            {
                EditorGUILayout.HelpBox(state.status, state.statusType);
                return;
            }

            var style = new GUIStyle(EditorStyles.helpBox);
            style.normal.textColor = CsvUpdatedStatusColor;
            var content = new GUIContent(
                state.status,
                EditorGUIUtility.IconContent("console.infoicon").image
            );
            GUILayout.Label(content, style);
        }

        void DrawConfigHelp()
        {
            EditorGUILayout.Space();
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(GetContentWidth())))
            {
                EditorGUILayout.HelpBox(
                    "Create the project-specific JSON config at the path above, then press Reload.",
                    MessageType.Info
                );
            }
        }

        void DrawPath(string path)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var pingButton = new GUIContent("◎", $"Highlight in Project View: { path }");
                if (GUILayout.Button(
                    pingButton,
                    GUILayout.Width(20f),
                    GUILayout.Height(EditorGUIUtility.singleLineHeight)
                ))
                {
                    PingAsset(path);
                }

                EditorGUILayout.LabelField(
                    new GUIContent(path, path),
                    EditorStyles.miniLabel,
                    GUILayout.MinWidth(0f)
                );
            }
        }

        void PingAsset(string assetPath)
        {
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (asset == null)
            {
                Debug.LogWarning($"Asset not found: { assetPath }");
                return;
            }
            EditorGUIUtility.PingObject(asset);
        }

        float GetContentWidth()
        {
            return Mathf.Clamp(
                position.width - ContentHorizontalMargin,
                WindowMinWidth - ContentHorizontalMargin,
                ContentMaxWidth
            );
        }

        void LoadConfig()
        {
            _sheetStates.Clear();
            if (!MasterDataImporterConfigLoader.TryLoad(out _config, out _configError))
            {
                _config = null;
                Repaint();
                return;
            }

            foreach (var sheet in _config.sheets)
            {
                _sheetStates.Add(new SheetState { config = sheet });
            }
            Repaint();
        }

        void SetAllSelected(bool selected)
        {
            foreach (var state in _sheetStates)
            {
                state.selected = selected;
            }
        }

        void StartImport(IEnumerable<SheetState> states)
        {
            ImportAsync(states.ToList()).Forget();
        }

        async UniTask ImportAsync(List<SheetState> states)
        {
            _isImporting = true;
            try
            {
                foreach (var state in states)
                {
                    await ImportSheetAsync(state);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                _isImporting = false;
                Repaint();
            }
        }

        async UniTask ImportSheetAsync(SheetState state)
        {
            try
            {
                state.status = "Downloading...";
                state.statusType = MessageType.Info;
                state.highlightCsvUpdated = false;
                Repaint();

                var downloadResult = await MasterDataCsvDownloader.DownloadAsync(
                    _config,
                    state.config
                );
                if (!downloadResult.succeeded)
                {
                    state.status = downloadResult.error;
                    state.statusType = MessageType.Error;
                    return;
                }

                AssetDatabase.ImportAsset(
                    downloadResult.csvAssetPath,
                    ImportAssetOptions.ForceUpdate
                );

                bool importSucceeded = MasterDataCsvImporter.ImportCsv(
                    downloadResult.csvAssetPath,
                    _config.GetDataName(state.config),
                    _config.GetDataTableDirectory(),
                    _config.GetDataType
                );

                if (!importSucceeded)
                {
                    state.status = "ScriptableObject import failed. See the Console for details.";
                    state.statusType = MessageType.Error;
                }
                else if (downloadResult.changed)
                {
                    state.status = "CSV updated and ScriptableObject imported.";
                    state.statusType = MessageType.Info;
                    state.highlightCsvUpdated = true;
                }
                else
                {
                    state.status = "CSV unchanged. ScriptableObject reimported.";
                    state.statusType = MessageType.Info;
                }
            }
            catch (Exception exception)
            {
                state.status = exception.Message;
                state.statusType = MessageType.Error;
                Debug.LogException(exception);
            }
            finally
            {
                Repaint();
            }
        }
    }
}
