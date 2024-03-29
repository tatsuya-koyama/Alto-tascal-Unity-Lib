﻿using UnityEngine;

namespace AltoFramework
{
    /// <summary>
    ///   イージング関数。引数 t には 0 〜 1 の値が渡される想定。
    ///   命名は以下が通例だがここでは In2 のように単純化している：
    ///     - 2 乗 : 〜Quad
    ///     - 3 乗 : 〜Cubic
    ///     - 4 乗 : 〜Quart
    ///     - 5 乗 : 〜Quint
    /// </summary>
    public static class AltoEase
    {
        public static float Linear(float t)
        {
            return t;
        }

        //----------------------------------------------------------------------
        // Ease In
        //----------------------------------------------------------------------

        public static float In2(float t)
        {
            return t * t;
        }

        public static float In3(float t)
        {
            return t * t * t;
        }

        public static float In4(float t)
        {
            return t * t * t * t;
        }

        public static float In5(float t)
        {
            return t * t * t * t * t;
        }

        //----------------------------------------------------------------------
        // Ease Out
        //----------------------------------------------------------------------

        public static float Out2(float t)
        {
            return t * (2f - t);
        }

        public static float Out3(float t)
        {
            float v = t - 1f;
            return 1f + (v * v * v);
        }

        public static float Out4(float t)
        {
            float v = t - 1f;
            return 1f - (v * v * v * v);
        }

        public static float Out5(float t)
        {
            float v = t - 1f;
            return 1f + (v * v * v * v * v);
        }

        //----------------------------------------------------------------------
        // Ease In-Out
        //----------------------------------------------------------------------

        public static float InOut2(float t)
        {
            return t * (2 - t);
        }

        public static float InOut3(float t)
        {
            if (t < 0.5)
            {
                return 4 * t * t * t;
            }
            return (t - 1) * (2*t - 2) * (2*t - 2) + 1;
        }

        public static float InOut4(float t)
        {
            if (t < 0.5)
            {
                return 8 * t * t * t * t;
            }
            float v = (-2 * t) + 2;
            return 1 - (v * v * v * v) / 2;
        }

        public static float InOut5(float t)
        {
            if (t < 0.5)
            {
                return 16 * t * t * t * t * t;
            }
            float v = (-2 * t) + 2;
            return 1 - (v * v * v * v * v) / 2;
        }

        //----------------------------------------------------------------------
        // Back
        //----------------------------------------------------------------------

        public static float InBack(float t)
        {
            const float s = 1.70158f;
            return t * t * ((s + 1) * t - s);
        }

        public static float OutBack(float t)
        {
            const float s = 1.70158f;
            t = t - 1;
            return t * t * ((s + 1) * t + s) + 1;
        }

        //----------------------------------------------------------------------
        // Elastic
        //----------------------------------------------------------------------

        public static float InElastic(float t)
        {
            return 1 - AltoEase.OutElastic(1 - t);
        }

        public static float OutElastic(float t)
        {
            if (t == 0f) { return 0f; }
            if (t == 1f) { return 1f; }

            float p = 0.3f;
            float s = p / 4;
            return Mathf.Pow(2, -10 * t) * Mathf.Sin((t - s) * (2 * Mathf.PI) / p) + 1;
        }
    }
}
