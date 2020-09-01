using NUnit.Framework;
using System;
using UnityEngine;

namespace AltoFramework.Tests
{
    public class GameContextTest
    {
        [SetUp]
        public void SetUp()
        {
            Alto.context = new AltoFramework.Testing.GlobalContextMock();
            Alto.context.Init();
        }

        class Signal_A : AltoSignal {}

        [Test]
        public void TestGetSignal()
        {
            var signal = Alto.Signal<Signal_A>();
            Assert.That(signal, Is.InstanceOf<Signal_A>());
        }

        [Test]
        public void TestEmitSignal()
        {
            string seq = "";
            Action action1 = () => { seq += "a"; };
            Action action2 = () => { seq += "b"; };
            Alto.Signal<Signal_A>().Connect(action1);
            Alto.Signal<Signal_A>().Connect(action2);

            Alto.Signal<Signal_A>().Emit();
            Assert.That(seq, Is.EqualTo("ab"));

            Alto.Signal<Signal_A>().Emit();
            Assert.That(seq, Is.EqualTo("abab"));
        }

        class MyBehaviour_1 : PoolableBehaviour {}
        class MyBehaviour_2 : PoolableBehaviour {}

        [Test]
        public void TestGetObjectPool()
        {
            var originalGameObj_1 = new GameObject();
            originalGameObj_1.AddComponent<MyBehaviour_1>();

            var originalGameObj_2 = new GameObject();
            originalGameObj_2.AddComponent<MyBehaviour_2>();

            Alto.CreateObjectPool<MyBehaviour_1>(originalGameObj_1, 16);
            Alto.CreateObjectPool<MyBehaviour_2>(originalGameObj_2,  8);

            MyBehaviour_1 obj_1 = Alto.ObjectPool<MyBehaviour_1>().Get();
            MyBehaviour_2 obj_2 = Alto.ObjectPool<MyBehaviour_2>().Get();

            Assert.That(obj_1.gameObject.activeSelf, Is.True);
            Assert.That(obj_2.gameObject.activeSelf, Is.True);
            Assert.That(Alto.ObjectPool<MyBehaviour_1>().RemainCount, Is.EqualTo(15));
            Assert.That(Alto.ObjectPool<MyBehaviour_2>().RemainCount, Is.EqualTo(7));
        }
    }
}
