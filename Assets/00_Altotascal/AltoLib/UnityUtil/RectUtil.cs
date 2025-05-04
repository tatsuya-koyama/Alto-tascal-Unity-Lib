using UnityEngine;

namespace AltoLib
{
    public class RectUtil
    {
        public static Rect GetOverlapRect(Rect a, Rect b)
        {
            float xMin = Mathf.Max(a.xMin, b.xMin);
            float yMin = Mathf.Max(a.yMin, b.yMin);
            float xMax = Mathf.Min(a.xMax, b.xMax);
            float yMax = Mathf.Min(a.yMax, b.yMax);

            // 重なりがないケース
            if (xMin > xMax || yMin > yMax)
            {
                return Rect.zero;
            }

            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }
    }
}
