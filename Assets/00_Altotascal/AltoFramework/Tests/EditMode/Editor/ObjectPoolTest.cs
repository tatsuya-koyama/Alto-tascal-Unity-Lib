using System;
using NUnit.Framework;
using UnityEngine;

namespace AltoFramework.Tests
{
    public class ObjectPoolTest
    {
        class MyBehaviour : PoolableBehaviour {}

        [Test]
        public void TestObjectPoolBorrow()
        {
            var originalGameObj = new GameObject();
            originalGameObj.AddComponent<MyBehaviour>();

            var objPool = new AltoObjectPool<MyBehaviour>(originalGameObj, 3);
            Assert.That(objPool.RemainCount, Is.EqualTo(3));

            MyBehaviour obj1 = objPool.Get();
            Assert.That(obj1.gameObject.activeSelf, Is.True);
            Assert.That(objPool.RemainCount, Is.EqualTo(2));

            MyBehaviour obj2 = objPool.Get();
            Assert.That(objPool.RemainCount, Is.EqualTo(1));

            MyBehaviour obj3 = objPool.Get();
            Assert.That(objPool.RemainCount, Is.EqualTo(0));

            MyBehaviour obj4 = objPool.Get();
            Assert.That(objPool.RemainCount, Is.EqualTo(0));
        }

        [Test]
        public void TestObjectPoolReturn()
        {
            var originalGameObj = new GameObject();
            originalGameObj.AddComponent<MyBehaviour>();

            var objPool = new AltoObjectPool<MyBehaviour>(originalGameObj, 3);
            Assert.That(objPool.RemainCount, Is.EqualTo(3));

            MyBehaviour obj1 = objPool.Get();
            MyBehaviour obj2 = objPool.Get();
            MyBehaviour obj3 = objPool.Get();
            MyBehaviour obj4 = objPool.Get();

            objPool.Return(obj2);
            Assert.That(obj2.gameObject.activeSelf, Is.False);
            Assert.That(objPool.RemainCount, Is.EqualTo(1));

            objPool.Return(obj1);
            objPool.Return(obj3);
            objPool.Return(obj4);
            Assert.That(objPool.RemainCount, Is.EqualTo(4));
        }
    }
}
