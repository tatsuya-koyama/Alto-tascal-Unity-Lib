using System;
using NUnit.Framework;

namespace AltoFramework.Tests
{
    public class TweenTest
    {
        [Test]
        public void TestTweenBasic()
        {
            float value = 0;
            var tween = new AltoTween(
                3f, 6.5f, 1.0f, AltoEase.Linear,
                x => value = x
            );

            tween.Init();
            Assert.That(value, Is.EqualTo(3f));

            tween.Update(0.1f);
            Assert.That(value, Is.EqualTo(3.35f));

            tween.Update(0.4f);  // passed 0.5 sec
            Assert.That(value, Is.EqualTo(4.75f));

            tween.Update(0.4f);  // passed 0.9 sec
            Assert.That(value, Is.EqualTo(6.15f));
            Assert.That(tween.IsCompleted(), Is.False);

            tween.Update(0.2f);  // passed 1.1 sec
            Assert.That(value, Is.EqualTo(6.5f));
            Assert.That(tween.IsCompleted(), Is.True);
        }

        // ログ目視確認デバッグ用
        // [Test]
        // public void TestTween()
        // {
        //     float value = 0;
        //     var tween = new AltoTween(
        //         0f, 1f, 1.0f, AltoEase.Out5,
        //         x => value = x
        //     );
        //     CLog.Clear();
        //     for (var i = 0; i < 20; ++i)
        //     {
        //         tween.Update(0.05f);
        //         CLog.Info(((i + 1) * 0.05).ToString("0.00") + " : " + value);
        //     }
        // }
    }
}
