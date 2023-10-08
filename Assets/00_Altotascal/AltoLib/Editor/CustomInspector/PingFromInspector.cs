#if ALTO_DEV
// Define "ALTO_DEV" symbol to enable this inspector extension.
using System;
using UnityEngine;
using UnityEditor;

/// <summary>
/// インスペクタに、そのアセットを Project View でハイライトするボタンを設置
/// </summary>
namespace AltoLib
{
    [InitializeOnLoad]
    public static class PingFromInspector
    {
        static PingFromInspector()
        {
            Editor.finishedDefaultHeaderGUI += editor =>
            {
                var path = AssetDatabase.GetAssetPath(editor.target);
                if (String.IsNullOrEmpty(path)) { return; }

                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Ping", GUILayout.MaxWidth(90)))
                    {
                        var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                        EditorGUIUtility.PingObject(asset);
                    }
                    if (GUILayout.Button("Copy Path", GUILayout.MaxWidth(120)))
                    {
                        EditorGUIUtility.systemCopyBuffer = path;
                    }
                }
                GUILayout.EndHorizontal();

                EditorGUILayout.HelpBox(path, MessageType.Info);
            };
        }
    }
}
#endif
