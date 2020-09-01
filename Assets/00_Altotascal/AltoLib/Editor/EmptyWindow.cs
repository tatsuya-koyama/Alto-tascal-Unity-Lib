using UnityEditor;
using UnityEngine;

namespace AltoLib
{
    public class EmptyWindow : EditorWindow
    {
        [MenuItem("Alto/Empty Window")]
        static void ShowWindow()
        {
            var window = CreateInstance<EmptyWindow>();
            window.titleContent = new GUIContent("---");
            window.Show();
        }
    }
}
