using System;
using NUnit.Framework;

namespace AltoFramework.Tests
{
    public class TweenerTest
    {
        [Test]
        public void TestTweener_Basic()
        {
            var tweener = new AltoTweener();
            Assert.That(tweener.count, Is.EqualTo(0));

            object obj = new System.Object();
            float value = 0;
            tweener.Go(obj, 10f, 20f, 0.5f, AltoEase.Linear, x => value = x);

            Assert.That(tweener.count, Is.EqualTo(1));
            Assert.That(value, Is.EqualTo(10f));

            tweener.Update(0.1f);
            Assert.That(value, Is.EqualTo(12f));

            tweener.Update(0.4f);
            Assert.That(value, Is.EqualTo(20f));
            Assert.That(tweener.count, Is.EqualTo(0));

            tweener.Update(1.0f);
            Assert.That(value, Is.EqualTo(20f));
        }

        [Test, Description("登録した複数の tween が並列に処理される")]
        public void TestTweener_MultiTween()
        {
            var tweener = new AltoTweener();

            object obj = new System.Object();
            float value1 = 0;
            float value2 = 0;
            tweener.Go(obj, 20f, 30f, 1.0f, AltoEase.Linear, x => value1 = x);
            tweener.Go(obj, 40f, 50f, 2.0f, AltoEase.Linear, x => value2 = x);

            Assert.That(tweener.count, Is.EqualTo(1));
            Assert.That(value1, Is.EqualTo(20f));
            Assert.That(value2, Is.EqualTo(40f));

            tweener.Update(0.1f);
            Assert.That(value1, Is.EqualTo(21f));
            Assert.That(value2, Is.EqualTo(40.5f));

            tweener.Update(0.9f);
            Assert.That(value1, Is.EqualTo(30f));
            Assert.That(value2, Is.EqualTo(45f));

            tweener.Update(1.3f);
            Assert.That(value1, Is.EqualTo(30f));
            Assert.That(value2, Is.EqualTo(50f));
            Assert.That(tweener.count, Is.EqualTo(0));
        }

        [Test, Description("obj には null を渡してもよい")]
        public void TestTweener_NullObject()
        {
            var tweener = new AltoTweener();

            float value1 = 0;
            float value2 = 0;
            tweener.Go(null, 30f, 10f, 1.0f, AltoEase.Linear, x => value1 = x);
            tweener.Go(null, 30f, 50f, 2.0f, AltoEase.Linear, x => value2 = x);

            Assert.That(tweener.count, Is.EqualTo(1));
            Assert.That(value1, Is.EqualTo(30f));
            Assert.That(value2, Is.EqualTo(30f));

            tweener.Update(3.0f);
            Assert.That(value1, Is.EqualTo(10f));
            Assert.That(value2, Is.EqualTo(50f));
            Assert.That(tweener.count, Is.EqualTo(0));
        }

        [Test, Description("obj を指定して実行中の tween を強制終了できる")]
        public void TestTweener_Finish()
        {
            var tweener = new AltoTweener();

            object obj1 = new System.Object();
            object obj2 = new System.Object();
            float value1 = 0;
            float value2 = 0;
            float value3 = 0;
            tweener.Go(obj1, 10f, -10f, 1.0f, AltoEase.Linear, x => value1 = x);
            tweener.Go(obj2, 30f,  40f, 1.0f, AltoEase.Linear, x => value2 = x);
            tweener.Go(obj2, 20f, 100f, 1.0f, AltoEase.Linear, x => value3 = x);

            Assert.That(tweener.count, Is.EqualTo(2));
            Assert.That(value1, Is.EqualTo(10f));
            Assert.That(value2, Is.EqualTo(30f));
            Assert.That(value3, Is.EqualTo(20f));

            tweener.Update(0.5f);
            Assert.That(value1, Is.EqualTo(0f));
            Assert.That(value2, Is.EqualTo(35f));
            Assert.That(value3, Is.EqualTo(60f));

            tweener.Finish(obj2);
            Assert.That(tweener.count, Is.EqualTo(1));
            Assert.That(value1, Is.EqualTo(0f));
            Assert.That(value2, Is.EqualTo(40f));
            Assert.That(value3, Is.EqualTo(100f));

            tweener.Update(0.5f);
            Assert.That(value1, Is.EqualTo(-10f));
            Assert.That(value2, Is.EqualTo(40f));
            Assert.That(value3, Is.EqualTo(100f));
        }

        [Test, Description("Finish に null を指定すると null で登録した tween を全て強制終了する")]
        public void TestTweener_FinishNull()
        {
            var tweener = new AltoTweener();

            object obj = new System.Object();
            float value1 = 0;
            float value2 = 0;
            float value3 = 0;
            tweener.Go(obj , 1f, 2f, 1.0f, AltoEase.Linear, x => value1 = x);
            tweener.Go(null, 3f, 4f, 1.0f, AltoEase.Linear, x => value2 = x);
            tweener.Go(null, 5f, 6f, 1.0f, AltoEase.Linear, x => value3 = x);

            Assert.That(tweener.count, Is.EqualTo(2));
            Assert.That(value1, Is.EqualTo(1f));
            Assert.That(value2, Is.EqualTo(3f));
            Assert.That(value3, Is.EqualTo(5f));

            tweener.Finish(null);
            Assert.That(tweener.count, Is.EqualTo(1));
            Assert.That(value1, Is.EqualTo(1f));
            Assert.That(value2, Is.EqualTo(4f));
            Assert.That(value3, Is.EqualTo(6f));
        }

        [Test, Description("tween 登録はメソッドチェーンでも記述できる")]
        public void TestTweener_MethodChain()
        {
            var tweener = new AltoTweener();
            float value = 0;
            tweener.NewTween().FromTo(10f, 20f, 1.0f).OnUpdate(x => value = x);

            Assert.That(tweener.count, Is.EqualTo(1));
            Assert.That(value, Is.EqualTo(10f), "OnUpdate の時点で最初の update は呼ばれている");

            tweener.Update(0.1f);
            Assert.That(value, Is.EqualTo(11f));

            tweener.Update(0.9f);
            Assert.That(tweener.count, Is.EqualTo(0));
            Assert.That(value, Is.EqualTo(20f));
        }

        [Test, Description(".OnComplete() で終了時のハンドラが設定できる")]
        public void TestTweener_OnComplete()
        {
            var tweener = new AltoTweener();
            float value1 = 0;
            float value2 = 0;
            tweener.NewTween().FromTo(3f, 7f, 1.0f).OnUpdate(x => value1 = x)
                   .OnComplete(x => value2 = x * 2);

            tweener.Update(0.9f);
            Assert.That(value2, Is.EqualTo(0));

            tweener.Update(0.3f);
            Assert.That(value2, Is.EqualTo(14f));
        }
    }
}
