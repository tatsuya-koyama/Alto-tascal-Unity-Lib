using UnityEngine;

namespace AltoLib
{
    public class RandUtil
    {
        /// <summary>
        /// min 〜 max の int の乱数を返す。（max の値も含む）
        /// </summary>
        public static int Range(int min, int max)
        {
            return Random.Range(min, max + 1);
        }
    }
}
