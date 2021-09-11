using System;

namespace AltoLib
{
    /// <summary>
    /// なんかどこかにありそうだけどパッと見つからなかった
    /// 汎用的な数学的関数をまとめておくところ
    /// </summary>
    public class MathUtil
    {
        /// <summary>
        /// value が min 〜 max の間でどの割合の位置にいるかを 0 〜 1 で返す。
        /// 例 : min = 100, max = 110 のとき、value = 103 なら 0.3 が返る
        /// </summary>
        public static float GetRatio(float value, float min, float max)
        {
            return (value - min) / (max - min);
        }

        /// <summary>
        /// int 版の Clamp.
        /// Math.Min / Math.Max によるイディオムはよく書き間違えそうになるので
        /// </summary>
        public static int Clamp(int value, int min, int max)
        {
            return Math.Min(Math.Max(value, min), max);
        }
    }
}
