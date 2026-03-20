using System;
using System.Collections.Generic;
using UnityEngine;

namespace AltoLib
{
    public class RandUtil
    {
        /// <summary>
        /// [min, max] 範囲の整数値をランダムに返す（min と max 自身も範囲に含まれる）
        /// </summary>
        public static int Range(int min, int max)
        {
            return UnityEngine.Random.Range(min, max + 1);
        }

        /// <summary>
        /// percent（0 〜 100 で指定）の確率で当たるくじに当たったら true
        /// </summary>
        public static bool LotInPercent(int percent)
        {
            return Range(1, 100) <= percent;
        }

        /// <summary>
        /// percent（0.0f 〜 100.0f で指定）の確率で当たるくじに当たったら true
        /// </summary>
        public static bool LotInPercent(float percent)
        {
            return UnityEngine.Random.Range(0f, 100f) < percent;
        }

        /// <summary>
        /// rate（0f 〜 1f で指定）の確率で当たるくじに当たったら true
        /// </summary>
        public static bool LotInRate(float rate)
        {
            return UnityEngine.Random.Range(0f, 1f) < rate;
        }

        /// <summary>
        /// List からランダムに 1 要素選んで返す
        /// </summary>
        public static T SampleAtRandom<T>(List<T> list)
        {
            if (list.Count == 0) { return default; }
            int randomIndex = Range(0, list.Count - 1);
            return list[randomIndex];
        }

        /// <summary>
        /// weight だけで抽選するシンプルな抽選ロジック
        /// 【使用例】
        /// var data = RandUtil.SimpleLot<DataType>(dataList, _ => _.weight);
        /// </summary>
        public static T SimpleLot<T>(List<T> list, Func<T, int> weightGetter) where T : class
        {
            if (list == null || list.Count == 0)
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
            int lotPos = Range(1, totalWeight);
            foreach (var item in list)
            {
                accWeight += weightGetter(item);
                if (lotPos <= accWeight) { return item; }
            }

            Debug.LogError("[SimpleLot] 不慮のエラー");
            return null;
        }
    }
}
