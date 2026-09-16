using System;
using UnityRandom = UnityEngine.Random;
using MathRandom = Unity.Mathematics.Random;

namespace AltoLib
{
    /// <summary>
    /// seed 指定可能な乱数生成器。RandomStreamExtension にて便利な拡張メソッドあり。
    /// 使い方としては App.Rand といったアクセスポイントに再現が必要な単位で new しておき、
    ///
    ///   App.Rand.Session  = new RandomStream();  // ゲーム全体で使う汎用乱数
    ///   App.Rand.DropItem = new RandomStream();  // 特定の機能関連の確率
    ///
    ///   float randomValue = App.Rand.Session.Range(0f, 1f);
    ///
    ///   App.Rand.DropItem.Reset(seed);
    ///   bool isDropped = App.Rand.DropItem.Chance(0.3f);
    ///
    /// のように使う想定
    /// </summary>
    public class RandomStream
    {
        MathRandom _random;
        uint _seed;
        long _drawCount;

        public uint Seed => _seed;
        public long DrawCount => _drawCount;

        //----------------------------------------------------------------------
        // 初期化
        //----------------------------------------------------------------------

        public RandomStream()
        {
            uint seed = (uint)UnityRandom.Range(0, Int32.MaxValue - 1);
            Reset(seed);
        }

        public RandomStream(uint seed)
        {
            Reset(seed);
        }

        public void Reset(uint seed)
        {
            // CreateFromIndex() で生成する場合は MaxValue を指定できない
            // （その代わり、通常は利用不可能な seed = 0 は利用可能になる）
            if (seed == uint.MaxValue)
            {
                throw new ArgumentException(nameof(seed), "uint.MaxValue is invalid random seed.");
            }

            // UnityEngine.Random が xorshift128 であるのに対し
            // Unity.Mathematics.Random は xorshift32 で
            // 1, 2, 3 といった小さい seed を用いると初期の値が小さくなりやすいといった偏りが生じる。
            // CreateFromIndex() は内部で hash を通してから初期化するためこの問題を回避する
            _seed = seed;
            _random = MathRandom.CreateFromIndex(seed);
            _drawCount = 0;
        }

        //----------------------------------------------------------------------
        // 基本の乱数取得処理
        //----------------------------------------------------------------------

        /// <summary>
        /// [0f, 1f) の乱数を返す
        /// </summary>
        public float Value()
        {
            ++_drawCount;
            return _random.NextFloat();
        }

        public float Range(float minInclusive, float maxExclusive)
        {
            ++_drawCount;
            return _random.NextFloat(minInclusive, maxExclusive);
        }

        public int RangeInt(int minInclusive, int maxExclusive)
        {
            ++_drawCount;
            return _random.NextInt(minInclusive, maxExclusive);
        }

        //----------------------------------------------------------------------
        // 状態の保存と復元
        //----------------------------------------------------------------------

        [Serializable]
        public struct State
        {
            public int version;
            public uint seed;
            public uint randomState;
            public long drawCount;
        }

        const int StateVersion = 1;

        public State CaptureState()
        {
            return new State
            {
                version     = StateVersion,
                seed        = _seed,
                randomState = _random.state,
                drawCount   = _drawCount,
            };
        }

        public void RestoreState(State state)
        {
            if (state.version != StateVersion)
            {
                throw new ArgumentException("State version not matched.");
            }

            _seed = state.seed;
            _drawCount = state.drawCount;
            // * コンストラクタで内部状態が 1 つ進むので new() の後に state をセット
            _random = new(1u);
            _random.state = state.randomState;
        }
    }
}
