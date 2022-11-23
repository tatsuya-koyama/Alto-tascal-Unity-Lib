using UnityEditor;
using UnityEngine;

namespace AltoLib
{
    public class EmptyWindow : EditorWindow
    {
        [MenuItem("Alto/Editor/Empty Window")]
        static void ShowWindow()
        {
            var window = CreateInstance<EmptyWindow>();
            window.titleContent = new GUIContent("---");
            window.Show();
        }
    }
}
