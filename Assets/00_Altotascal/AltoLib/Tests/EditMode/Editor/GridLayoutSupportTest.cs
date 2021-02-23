using System;
using NUnit.Framework;
using UnityEngine;

namespace AltoLib.Tests
{
    public class GridLyaoutSupportTest
    {
        float Epsilon = 0.0001f;

        [Test, Description("セルサイズに応じたカラム数が求まる")]
        public void Test_CalcNumColumn()
        {
            {
                var support = new GridLayoutSupport(new Vector2(30f, 70f), 100f);
                Assert.That(support.numColumn, Is.EqualTo(3));
            }
            {
                var support = new GridLayoutSupport(new Vector2(80f, 60f), 100f);
                Assert.That(support.numColumn, Is.EqualTo(1));
            }
            {
                var support = new GridLayoutSupport(new Vector2(120f, 60f), 100f);
                Assert.That(support.numColumn, Is.EqualTo(1),
                            "セルの width が areaWidth を越えていた場合はカラム数 = 1");
            }
        }

        [Test, Description("spacing, padding も考慮してカラム数が求まる")]
        public void Test_CalcNumColumn_2()
        {
            {
                var support = new GridLayoutSupport(
                    new Vector2(30f, 70f), 111f, new Vector2(10f, 20f), new Vector4(0f, 0f, 0f, 0f)
                );
                Assert.That(support.numColumn, Is.EqualTo(3));
            }
            {
                var support = new GridLayoutSupport(
                    new Vector2(30f, 70f), 109f, new Vector2(10f, 20f), new Vector4(0f, 0f, 0f, 0f)
                );
                Assert.That(support.numColumn, Is.EqualTo(2));
            }
            {
                var support = new GridLayoutSupport(
                    new Vector2(30f, 70f), 151f, new Vector2(10f, 20f), new Vector4(0f, 20f, 0f, 20f)
                );
                Assert.That(support.numColumn, Is.EqualTo(3));
            }
            {
                var support = new GridLayoutSupport(
                    new Vector2(30f, 70f), 200f, new Vector2(10f, 20f), new Vector4(0f, 40f, 0f, 20f)
                );
                Assert.That(support.numColumn, Is.EqualTo(3));
            }
        }

        [Test, Description("セルサイズに応じた高さが取得できる")]
        public void Test_CalcHeight()
        {
            {
                var support = new GridLayoutSupport(new Vector2(30f, 70f), 100f);
                Assert.That(support.CalcHeight(0), Is.EqualTo(0f).Within(Epsilon));
                Assert.That(support.CalcHeight(1), Is.EqualTo(70f).Within(Epsilon));
                Assert.That(support.CalcHeight(3), Is.EqualTo(70f).Within(Epsilon));
                Assert.That(support.CalcHeight(4), Is.EqualTo(140f).Within(Epsilon));
                Assert.That(support.CalcHeight(10), Is.EqualTo(280f).Within(Epsilon));
            }
        }

        [Test, Description("セルサイズ, spacing, padding に応じた高さが取得できる")]
        public void Test_CalcHeight_2()
        {
            {
                // areaWidth : 200 - (40 + 20) = 140
                // 30 + (10 + 30)*3 > 140 なのでカラム数 = 3
                var support = new GridLayoutSupport(
                    new Vector2(30f, 70f), 200f, new Vector2(10f, 20f), new Vector4(3f, 40f, 4f, 20f)
                );
                Assert.That(support.CalcHeight(0), Is.EqualTo(7f).Within(Epsilon));
                Assert.That(support.CalcHeight(1), Is.EqualTo(77f).Within(Epsilon));
                Assert.That(support.CalcHeight(3), Is.EqualTo(77f).Within(Epsilon));
                Assert.That(support.CalcHeight(4), Is.EqualTo(167f).Within(Epsilon));
            }
        }

        [Test, Description("セルの座標が取得できる")]
        public void Test_CalcCellPos()
        {
            {
                // areaWidth : 200 - (40 + 20) = 140
                // 30 + (10 + 30)*3 > 140 なのでカラム数 = 3
                var support = new GridLayoutSupport(
                    new Vector2(30f, 70f), 200f, new Vector2(10f, 20f), new Vector4(3f, 40f, 4f, 20f)
                );
                // 左上アンカー
                Vector2 anchor = new Vector2(0f, 1f);
                Assert.That(support.CalcCellPos(0, anchor), Is.EqualTo(new Vector2(20f, -3f)));
                Assert.That(support.CalcCellPos(1, anchor), Is.EqualTo(new Vector2(60f, -3f)));
                Assert.That(support.CalcCellPos(2, anchor), Is.EqualTo(new Vector2(100f, -3f)));
                Assert.That(support.CalcCellPos(3, anchor), Is.EqualTo(new Vector2(20f, -93f)));
                // 中央アンカー
                Assert.That(support.CalcCellPos(0), Is.EqualTo(new Vector2(20f + 15f, -3f - 35f)));
                Assert.That(support.CalcCellPos(1), Is.EqualTo(new Vector2(60f + 15f, -3f - 35f)));
                Assert.That(support.CalcCellPos(2), Is.EqualTo(new Vector2(100f + 15f, -3f - 35f)));
                Assert.That(support.CalcCellPos(3), Is.EqualTo(new Vector2(20f + 15f, -93f - 35f)));
            }
        }
    }
}
