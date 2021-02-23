using UnityEngine;

namespace AltoLib
{
    public class ColorUtil
    {
        public static Color ColorByHex(uint rgba)
        {
            float r, g, b, a;
            a = (rgba % 256) / 255f;

            rgba >>= 8;
            b = (rgba % 256) / 255f;

            rgba >>= 8;
            g = (rgba % 256) / 255f;

            rgba >>= 8;
            r = (rgba % 256) / 255f;

            return new Color(r, g, b, a);
        }
    }
}
