using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AltoLib
{
    public class ListUtil
    {
        /// <summary>
        /// ランダムに 1 つ要素をピックして返す
        /// </summary>
        public static T RandomPick<T>(List<T> list)
        {
            if (list.Count == 0) { return default; }
            return list[Random.Range(0, list.Count)];
        }

        /// <summary>
        /// List を破壊的にシャッフル
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
