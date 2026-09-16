using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AltoLib
{
    /// <summary>
    /// 空 GameObject を Scene View 上で視覚的にわかりやすくするデバッグコンポーネント
    /// </summary>
    public class AltoDebugLabelGizmo : MonoBehaviour
    {
        public enum GizmoType
        {
            Sphere, Cube, SolidRect,
        }

        [SerializeField] GizmoType _gizmoType = GizmoType.SolidRect;
        [SerializeField] Color _color1 = Color.green;
        [SerializeField] Color _color2 = Color.black;
        [SerializeField] float _size = 0.15f;
        [SerializeField] string _label = string.Empty;
        [SerializeField] Color _labelFgColor = Color.white;
        [SerializeField] Color _labelBgColor = new(0, 0, 0, 0.5f);

    #if UNITY_EDITOR

        void OnDrawGizmos()
        {
            DrawPointer();
            DrawLabel();
        }

        void DrawPointer()
        {
            Gizmos.color = _color1;
            var pos = transform.position;
            var r = _size * 0.5f;

            switch (_gizmoType)
            {
            case GizmoType.Sphere:
                Gizmos.DrawSphere(pos, r);
                break;

            case GizmoType.Cube:
                Gizmos.DrawCube(pos, Vector3.one * _size);
                break;

            case GizmoType.SolidRect:
                Vector3[] verts = new Vector3[]
                {
                    new(pos.x - r, pos.y - r, pos.z),
                    new(pos.x + r, pos.y - r, pos.z),
                    new(pos.x + r, pos.y + r, pos.z),
                    new(pos.x - r, pos.y + r, pos.z),
                };
                Handles.DrawSolidRectangleWithOutline(verts, _color1, _color2);
                break;
            }
        }

        void DrawLabel()
        {
            if (string.IsNullOrEmpty(_label)) { return; }

            var pos = transform.position;
            pos.x += (_size / 2);
            pos.y -= _size;

            var bgTex = new Texture2D(1, 1);
            bgTex.SetPixel(0, 0, _labelBgColor);
            bgTex.Apply();

            var style = new GUIStyle(EditorStyles.boldLabel);
            style.normal.textColor = _labelFgColor;
            style.normal.background = bgTex;
            Handles.Label(pos, _label, style);
        }

    #endif
    }
}
