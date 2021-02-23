using UnityEngine;

namespace AltoLib.UnityExt
{
    public static class SpriteRendererExtension
    {
        public static void SetAlpha(this SpriteRenderer renderer, float alpha)
        {
            Color color = renderer.color;
            color.a = alpha;
            renderer.color = color;
        }
    }
}
