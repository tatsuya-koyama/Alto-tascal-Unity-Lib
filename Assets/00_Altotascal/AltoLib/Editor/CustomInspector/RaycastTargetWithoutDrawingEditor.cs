using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

namespace AltoLib
{
    [CanEditMultipleObjects, CustomEditor(typeof(RaycastTargetWithoutDrawing), false)]
    public class RaycastTargetWithoutDrawingEditor : GraphicEditor
    {
        public override void OnInspectorGUI()
        {
            base.serializedObject.Update();
            EditorGUILayout.PropertyField(base.m_Script, new GUILayoutOption[0]);
            base.RaycastControlsGUI();
            base.serializedObject.ApplyModifiedProperties();
        }
    }
}
