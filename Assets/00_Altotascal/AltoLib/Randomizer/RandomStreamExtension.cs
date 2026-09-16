using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AltoLib
{
    public static class RandomStreamExtension
    {
        //----------------------------------------------------------------------
        // 乱数取得
        //----------------------------------------------------------------------

        /// <summary>
        /// [min, max] 範囲のランダムな int 値を返す
        /// （RangeInt() と異なり、max 自身も範囲に含まれる）
        /// </summary>
        public static int RangeIntInclusive(
            this RandomStream random, int minInclusive, int maxInclusive
        )
        {
            if (maxInclusive == int.MaxValue)
            {
                throw new ArgumentException("Cannot use int.MaxValue as maxInclusive.");
            }
            return random.RangeInt(minInclusive, maxInclusive + 1);
        }

        //----------------------------------------------------------------------
        // 確率
        //----------------------------------------------------------------------

        /// <summary>
        /// rate（0f 〜 1f で指定）の確率で当たるくじに当たったら true
        /// </summary>
        public static bool Chance(this RandomStream random, float rate)
        {
            return random.Value() < rate;
        }

        /// <summary>
        /// percent（0f 〜 100f で指定）の確率で当たるくじに当たったら true
        /// </summary>
        public static bool ChancePercent(this RandomStream random, float percent)
        {
            return random.Chance(percent / 100f);
        }

        /// <summary>
        /// percent（0 〜 100 の int 値で指定）の確率で当たるくじに当たったら true
        /// </summary>
        public static bool ChancePercent(this RandomStream random, int percent)
        {
            return random.Chance(percent / 100f);
        }

        //----------------------------------------------------------------------
        // 値の加工
        //----------------------------------------------------------------------

        /// <summary>
        /// float 値にランダムな揺らぎを加える
        /// 例 : variance に 0.2f を渡したら value の 0.8 〜 1.2 倍が返る
        /// </summary>
        public static float AddVariance(this RandomStream random, float value, float variance)
        {
            return random.Range(value * (1f - variance), value * (1f + variance));
        }

        //----------------------------------------------------------------------
        // コレクション
        //----------------------------------------------------------------------

        /// <summary>
        /// List からランダムに 1 要素選んで返す
        /// </summary>
        public static T SampleAtRandom<T>(this RandomStream random, IReadOnlyList<T> list)
        {
            if (list == null || list.Count == 0) { return default; }
            int randomIndex = random.RangeInt(0, list.Count);
            return list[randomIndex];
        }

        /// <summary>
        /// List をシャッフル
        /// </summary>
        public static void Shuffle<T>(this RandomStream random, IList<T> list)
        {
            // Fisher–Yates shuffle
            for (int i = list.Count - 1; i > 0; i--)
            {
                int swapIndex = random.RangeInt(0, i + 1);
                (list[swapIndex], list[i]) = (list[i], list[swapIndex]);
            }
        }

        //----------------------------------------------------------------------
        // 重み付き抽選
        //----------------------------------------------------------------------

        /// <summary>
        /// weight だけで抽選するシンプルな抽選ロジック
        /// 【使用例】
        /// var selectedMaster = randomStream.SimpleLot<HogeMaster>(masterList, _ => _.Weight);
        /// </summary>
        public static T SimpleLot<T>(
            this RandomStream random, IEnumerable<T> list, Func<T, int> weightGetter
        ) where T : class
        {
            if (list == null || !list.Any())
            {
                Debug.LogError("[SimpleLot] 抽選リストが空");
                return null;
            }

            int totalWeight = 0;
            foreach (var item in list)
            {
                int weight = weightGetter(item);
                if (weight < 0)
                {
                    Debug.LogError("[SimpleLot] weight < 0 が含まれている");
                }
                totalWeight += weight;
            }

            if (totalWeight < 1)
            {
                Debug.LogError("[SimpleLot] weight 合計が 1 未満");
                return null;
            }

            int accWeight = 0;
            int lotPos = random.RangeIntInclusive(1, totalWeight);
            foreach (var item in list)
            {
                accWeight += weightGetter(item);
                if (lotPos <= accWeight) { return item; }
            }

            Debug.LogError("[SimpleLot] 抽選に失敗");
            return null;
        }
    }
}
