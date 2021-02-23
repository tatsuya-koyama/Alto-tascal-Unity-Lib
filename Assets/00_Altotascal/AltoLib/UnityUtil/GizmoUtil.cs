using UnityEngine;

namespace AltoLib
{
    public class GizmoUtil
    {
        public static void DrawRotatedCube(Vector3 center, Vector3 eulerAngles, Vector3 size)
        {
            Quaternion rotation = Quaternion.Euler(eulerAngles);
            DrawRotatedCube(center, rotation, size);
        }

        public static void DrawRotatedCube(Vector3 center, Quaternion rotation, Vector3 size)
        {
            var backupMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(center, rotation, size);
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
            Gizmos.matrix = backupMatrix;
        }

        public static void DrawRotatedWireCube(Vector3 center, Vector3 eulerAngles, Vector3 size)
        {
            Quaternion rotation = Quaternion.Euler(eulerAngles);
            DrawRotatedWireCube(center, rotation, size);
        }

        public static void DrawRotatedWireCube(Vector3 center, Quaternion rotation, Vector3 size)
        {
            var backupMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(center, rotation, size);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            Gizmos.matrix = backupMatrix;
        }
    }
}
