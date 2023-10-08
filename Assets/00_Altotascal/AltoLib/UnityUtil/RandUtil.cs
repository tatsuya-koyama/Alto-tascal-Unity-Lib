using UnityEngine;
using System.Collections.Generic;

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

        /// <summary>
        /// List をシャッフル
        /// </summary>
        public static List<T> Shuffle<T>(List<T> list)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                T tmp = list[i];
                int r = Random.Range(0, list.Count);
                list[i] = list[r];
                list[r] = tmp;
            }
            return list;
        }
    }
}
