using UnityEngine;

namespace AltoLib
{
    public class RectTransformUtil
    {
        /// <summary>
        /// 位置を変えずに pivot だけを変更
        /// ※ 現状は回転していない rect のみが対象
        /// </summary>
        public static void SetPivot(ref RectTransform rect, Vector2 pivot)
        {
            Vector2 size = rect.rect.size;
            Vector2 deltaPivot = rect.pivot - pivot;
            Vector3 deltaPosition = new Vector3(deltaPivot.x * size.x, deltaPivot.y * size.y);
            rect.pivot = pivot;
            rect.localPosition -= deltaPosition;
        }
    }
}
