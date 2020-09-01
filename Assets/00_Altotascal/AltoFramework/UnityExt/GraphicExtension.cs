using UnityEngine;
using UnityEngine.UI;

namespace AltoFramework.UnityExt
{
    public static class GraphicExtension
    {
        public static void SetAlpha<T>(this T graphic, float alpha) where T : Graphic
        {
            Color color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }
    }
}
