#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace AltoLib
{
    public class ToggleCompileLock
    {
        [MenuItem("Alto/自動コンパイルを OFF にする")]
        static void Lock ()
        {
        Debug.Log("自動コンパイルを無効化。Ctrl + R で手動コンパイルできます");
        EditorApplication.LockReloadAssemblies();
        }

        [MenuItem("Alto/自動コンパイルを ON に戻す")]
        static void Unlock ()
        {
        Debug.Log("自動コンパイルを有効化（Unity デフォルトの挙動）");
        EditorApplication.UnlockReloadAssemblies();
        }
    }
}
#endif
