using UnityEngine;

namespace AltoLib
{
    /// <summary>
    /// UnityEngine.Debug には標準だと DrawLine と DrawRay しか無いので
    /// 四角形や楕円の描画をサポート
    /// </summary>
    public class DebugDrawUtil
    {
        /// <summary>
        /// 指定位置を中心とする正方形を XY 平面に描画
        /// </summary>
        public static void DrawRect(Vector3 pos, float size, Color color, float duration = 0f)
        {
            float x1 = pos.x - size / 2;
            float x2 = pos.x + size / 2;
            float y1 = pos.y - size / 2;
            float y2 = pos.y + size / 2;
            Debug.DrawLine(new Vector2(x1, y1), new Vector2(x2, y1), color, duration);
            Debug.DrawLine(new Vector2(x2, y1), new Vector2(x2, y2), color, duration);
            Debug.DrawLine(new Vector2(x2, y2), new Vector2(x1, y2), color, duration);
            Debug.DrawLine(new Vector2(x1, y2), new Vector2(x1, y1), color, duration);
        }

        /// <summary>
        /// 四角形の描画、4 頂点を渡ずバージョン。
        /// cornerPoints は時計回りまたは反時計回りに並んでいる想定
        /// </summary>
        public static void DrawRect(Vector3[] cornerPoints, Color color, float duration = 0f)
        {
            Debug.DrawLine(cornerPoints[0], cornerPoints[1], color, duration);
            Debug.DrawLine(cornerPoints[1], cornerPoints[2], color, duration);
            Debug.DrawLine(cornerPoints[2], cornerPoints[3], color, duration);
            Debug.DrawLine(cornerPoints[3], cornerPoints[0], color, duration);
        }

        /// <summary>
        /// 四角形 (AABB) の描画、左上と右下の 2 頂点を渡ずバージョン
        /// </summary>
        public static void DrawRect(Vector3 leftTop, Vector3 rightBottom, Color color, float duration = 0f)
        {
            float x1 = leftTop.x;
            float x2 = rightBottom.x;
            float y1 = leftTop.y;
            float y2 = rightBottom.y;
            Debug.DrawLine(new Vector2(x1, y1), new Vector2(x2, y1), color, duration);
            Debug.DrawLine(new Vector2(x2, y1), new Vector2(x2, y2), color, duration);
            Debug.DrawLine(new Vector2(x2, y2), new Vector2(x1, y2), color, duration);
            Debug.DrawLine(new Vector2(x1, y2), new Vector2(x1, y1), color, duration);
        }

        public static void DrawDiamond3D(Vector3 pos, float size, Color color, float duration = 0f)
        {
            float d = size / 2;
            Vector3[] posList = new Vector3[4];

            posList[0] = pos; posList[0].x -= d;
            posList[1] = pos; posList[1].y -= d;
            posList[2] = pos; posList[2].x += d;
            posList[3] = pos; posList[3].y += d;
            DrawRect(posList, color, duration);

            posList[0] = pos; posList[0].y -= d;
            posList[1] = pos; posList[1].z -= d;
            posList[2] = pos; posList[2].y += d;
            posList[3] = pos; posList[3].z += d;
            DrawRect(posList, color, duration);

            posList[0] = pos; posList[0].z -= d;
            posList[1] = pos; posList[1].x -= d;
            posList[2] = pos; posList[2].z += d;
            posList[3] = pos; posList[3].x += d;
            DrawRect(posList, color, duration);
        }

        public static void DrawCircle(
            Vector3 pos, float radius, Color color, float duration = 0f, int segments = 16
        )
        {
            DrawEllipse(pos, Vector3.forward, Vector3.up, radius, radius, segments, color, duration);
        }

        public static void DrawEllipse(
            Vector3 pos, Vector3 forward, Vector3 up,
            float radiusX, float radiusY, int segments,
            Color color, float duration = 0f
        )
        {
            float angle;
            Quaternion rotation = Quaternion.LookRotation(forward, up);
            Vector3 prev    = Vector3.zero;
            Vector3 current = Vector3.zero;

            for (int i = 0; i < segments + 1; ++i)
            {
                angle = 360f / segments * i;
                current.x = Mathf.Sin(Mathf.Deg2Rad * angle) * radiusX;
                current.y = Mathf.Cos(Mathf.Deg2Rad * angle) * radiusY;

                if (i > 0)
                {
                    Debug.DrawLine(
                        rotation * prev + pos,
                        rotation * current + pos,
                        color, duration
                    );
                }
                prev = current;
            }
        }
    }
}
