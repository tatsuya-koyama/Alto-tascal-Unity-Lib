using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace AltoLib.Tests
{
    public class ObjectUtilTest
    {
        public class TestClass1
        {
            public string text;
            public int num;
            public List<int> nums;

            public string textProp    { get; set; }
            public int numProp        { get; set; }
            public List<int> numsProp { get; set; }
        }

        (TestClass1 obj1, TestClass1 obj2) GetTestObjs()
        {
            var obj1 = new TestClass1()
            {
                text = "alice",
                num  = 123,
                nums = new List<int>() { 1, 2, 3 },
                textProp = "carol",
                numProp = 67,
                numsProp = new List<int>() { 6, 7 },
            };
            var obj2 = new TestClass1()
            {
                text = "bob",
                num  = 45,
                nums = new List<int>() { 4, 5 },
                textProp = "dave",
                numProp = 890,
                numsProp = new List<int>() { 8, 9, 0 },
            };
            return (obj1, obj2);
        }

        [Test]
        public void Test_CopyFields()
        {
            var (obj1, obj2) = GetTestObjs();
            ObjectUtil.CopyFields(obj1, obj2);

            Assert.That(obj2.text, Is.EqualTo("alice"));
            Assert.That(obj2.num, Is.EqualTo(123));
            Assert.That(obj2.nums, Is.EqualTo(new List<int>() { 1, 2, 3 }));

            Assert.That(obj2.textProp, Is.EqualTo("dave"));
            Assert.That(obj2.numProp, Is.EqualTo(890));
            Assert.That(obj2.numsProp, Is.EqualTo(new List<int>() { 8, 9, 0 }));
        }

        [Test]
        public void Test_CopyProps()
        {
            var (obj1, obj2) = GetTestObjs();
            ObjectUtil.CopyProps(obj1, obj2);

            Assert.That(obj2.text, Is.EqualTo("bob"));
            Assert.That(obj2.num, Is.EqualTo(45));
            Assert.That(obj2.nums, Is.EqualTo(new List<int>() { 4, 5 }));

            Assert.That(obj2.textProp, Is.EqualTo("carol"));
            Assert.That(obj2.numProp, Is.EqualTo(67));
            Assert.That(obj2.numsProp, Is.EqualTo(new List<int>() { 6, 7 }));
        }
    }
}
