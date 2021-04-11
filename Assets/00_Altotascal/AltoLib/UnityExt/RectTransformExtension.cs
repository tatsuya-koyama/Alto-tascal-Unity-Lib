using UnityEngine;

namespace AltoLib.UnityExt
{
    public static class RectTransformExtension
    {
        static Vector2 _vec2;

        //---------------------------------------------------------------------
        // AnchoredPosition
        //---------------------------------------------------------------------

        public static void SetAnchoredPosition(this RectTransform rect, float? x, float? y)
        {
            _vec2.Set(
                x ?? rect.anchoredPosition.x,
                y ?? rect.anchoredPosition.y
            );
            rect.anchoredPosition = _vec2;
        }

        public static void AddAnchoredPosition(this RectTransform rect, float x, float y)
        {
            _vec2.Set(
                rect.anchoredPosition.x + x,
                rect.anchoredPosition.y + y
            );
            rect.anchoredPosition = _vec2;
        }

        public static void AddAnchoredPosition(this RectTransform rect, Vector2 vec)
        {
            rect.AddAnchoredPosition(vec.x, vec.y);
        }

        //---------------------------------------------------------------------
        // SizeDelta
        //---------------------------------------------------------------------

        public static void SetSizeDelta(this RectTransform rect, float? x, float? y)
        {
            _vec2.Set(
                x ?? rect.sizeDelta.x,
                y ?? rect.sizeDelta.y
            );
            rect.sizeDelta = _vec2;
        }

        public static void AddSizeDelta(this RectTransform rect, float x, float y)
        {
            _vec2.Set(
                rect.sizeDelta.x + x,
                rect.sizeDelta.y + y
            );
            rect.sizeDelta = _vec2;
        }

        public static void AddSizeDelta(this RectTransform rect, Vector2 vec)
        {
            rect.AddSizeDelta(vec.x, vec.y);
        }
    }
}
