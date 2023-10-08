#if ALTO_DEV
// Define "ALTO_DEV" symbol to enable this inspector extension.
using UnityEngine;
using UnityEditor;

/// <summary>
/// Mesh のインスペクタ上部にポリゴン数を表示する。
/// Prefab や GameObject 選択時にポリゴン数をパッと確認できて便利
/// </summary>
namespace AltoLib
{
    [CustomEditor(typeof(MeshFilter))]
    public class MeshPolygonsDisplay : Editor
    {
        public override void OnInspectorGUI()
        {
            MeshFilter filter = target as MeshFilter;
            string polygons = $"{filter.sharedMesh.triangles.Length / 3} Tris";
            EditorGUILayout.LabelField( polygons );

            base.OnInspectorGUI();
        }
    }


    [CustomEditor(typeof(SkinnedMeshRenderer))]
    public class SkinnedMEshPolygonsDisplay : Editor
    {
        public override void OnInspectorGUI()
        {
            SkinnedMeshRenderer skin = target as SkinnedMeshRenderer;
            string polygons = $"{skin.sharedMesh.triangles.Length / 3} Tris";
            EditorGUILayout.LabelField( polygons );

            base.OnInspectorGUI();
        }
    }
}
#endif
