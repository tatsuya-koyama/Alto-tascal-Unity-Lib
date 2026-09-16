using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace AltoLib.Tests
{
    public class LifetimeListTest
    {
        class TestItem : ILifetimeTickable
        {
            public string name;
            public bool isDead;
            public int tickCount;
            public float lastDeltaTime;
            public Action onTick;

            public TestItem(string name, bool isDead = false)
            {
                this.name = name;
                this.isDead = isDead;
            }

            public bool Tick(float deltaTime)
            {
                ++tickCount;
                lastDeltaTime = deltaTime;
                onTick?.Invoke();
                return isDead;
            }
        }

        void AssertItems(LifetimeList<TestItem> list, params TestItem[] expected)
        {
            Assert.That(list.Count, Is.EqualTo(expected.Length));

            int index = 0;
            foreach (var item in list)
            {
                Assert.That(item, Is.SameAs(expected[index]));
                Assert.That(list[index], Is.SameAs(expected[index]));
                ++index;
            }
            Assert.That(index, Is.EqualTo(expected.Length));
        }

        [Test, Description("空のリストを Tick, Clear できる")]
        public void Test_EmptyList()
        {
            var list = new LifetimeList<TestItem>(8);

            Assert.That(list.Count, Is.EqualTo(0));
            Assert.That(list.Tick(0.1f), Is.EqualTo(0));

            list.Clear();
            Assert.That(list.Count, Is.EqualTo(0));
        }

        [Test, Description("追加した要素を追加順に取得できる")]
        public void Test_Add()
        {
            var item1 = new TestItem("item1");
            var item2 = new TestItem("item2");
            var list = new LifetimeList<TestItem>();

            list.Add(item1);
            list.Add(item2);

            AssertItems(list, item1, item2);
        }

        [Test, Description("null は追加できない")]
        public void Test_AddNull()
        {
            var list = new LifetimeList<TestItem>();

            Assert.Throws<ArgumentNullException>(() => list.Add(null));
        }

        [Test, Description("生存要素をすべて Tick し、順序を維持する")]
        public void Test_TickAllAlive()
        {
            var item1 = new TestItem("item1");
            var item2 = new TestItem("item2");
            var item3 = new TestItem("item3");
            var list = new LifetimeList<TestItem>();
            list.Add(item1);
            list.Add(item2);
            list.Add(item3);

            int removedCount = list.Tick(0.25f);

            Assert.That(removedCount, Is.EqualTo(0));
            AssertItems(list, item1, item2, item3);
            foreach (var item in list)
            {
                Assert.That(item.tickCount, Is.EqualTo(1));
                Assert.That(item.lastDeltaTime, Is.EqualTo(0.25f));
            }
        }

        [Test, Description("死亡要素だけを削除し、生存要素の順序を維持する")]
        public void Test_TickRemovesDeadItems()
        {
            var item1 = new TestItem("item1");
            var item2 = new TestItem("item2", isDead: true);
            var item3 = new TestItem("item3");
            var item4 = new TestItem("item4", isDead: true);
            var item5 = new TestItem("item5");
            var removedItems = new List<TestItem>();
            var list = new LifetimeList<TestItem>(onRemoved: item => removedItems.Add(item));
            list.Add(item1);
            list.Add(item2);
            list.Add(item3);
            list.Add(item4);
            list.Add(item5);

            int removedCount = list.Tick(0.1f);

            Assert.That(removedCount, Is.EqualTo(2));
            AssertItems(list, item1, item3, item5);
            Assert.That(removedItems, Is.EqualTo(new[] { item2, item4 }));
            Assert.That(item1.tickCount, Is.EqualTo(1));
            Assert.That(item2.tickCount, Is.EqualTo(1));
            Assert.That(item3.tickCount, Is.EqualTo(1));
            Assert.That(item4.tickCount, Is.EqualTo(1));
            Assert.That(item5.tickCount, Is.EqualTo(1));
        }

        [Test, Description("全要素が死亡したときリストが空になる")]
        public void Test_TickRemovesAllItems()
        {
            var item1 = new TestItem("item1", isDead: true);
            var item2 = new TestItem("item2", isDead: true);
            var item3 = new TestItem("item3", isDead: true);
            var list = new LifetimeList<TestItem>();
            list.Add(item1);
            list.Add(item2);
            list.Add(item3);

            int removedCount = list.Tick(0.1f);

            Assert.That(removedCount, Is.EqualTo(3));
            AssertItems(list);
        }

        [Test, Description("Tick 中に追加した要素は次の Tick から走査される")]
        public void Test_AddDuringTick()
        {
            var item1 = new TestItem("item1");
            var addedItem = new TestItem("addedItem");
            var list = new LifetimeList<TestItem>();
            item1.onTick = () =>
            {
                list.Add(addedItem);
                item1.onTick = null;
            };
            list.Add(item1);

            list.Tick(0.1f);

            AssertItems(list, item1, addedItem);
            Assert.That(item1.tickCount, Is.EqualTo(1));
            Assert.That(addedItem.tickCount, Is.EqualTo(0));

            list.Tick(0.1f);
            Assert.That(item1.tickCount, Is.EqualTo(2));
            Assert.That(addedItem.tickCount, Is.EqualTo(1));
        }

        [Test, Description("削除コールバック中に追加した要素も次の Tick から走査される")]
        public void Test_AddFromOnRemoved()
        {
            var deadItem = new TestItem("deadItem", isDead: true);
            var addedItem = new TestItem("addedItem");
            LifetimeList<TestItem> list = null;
            list = new(onRemoved: _ => list.Add(addedItem));
            list.Add(deadItem);

            int removedCount = list.Tick(0.1f);

            Assert.That(removedCount, Is.EqualTo(1));
            AssertItems(list, addedItem);
            Assert.That(addedItem.tickCount, Is.EqualTo(0));

            list.Tick(0.1f);
            Assert.That(addedItem.tickCount, Is.EqualTo(1));
        }

        [Test, Description("Clear ですべての要素を削除し、削除コールバックを呼ぶ")]
        public void Test_Clear()
        {
            var item1 = new TestItem("item1");
            var item2 = new TestItem("item2");
            var item3 = new TestItem("item3");
            var removedItems = new List<TestItem>();
            var list = new LifetimeList<TestItem>(onRemoved: item => removedItems.Add(item));
            list.Add(item1);
            list.Add(item2);
            list.Add(item3);

            list.Clear();

            AssertItems(list);
            Assert.That(removedItems, Is.EquivalentTo(new[] { item1, item2, item3 }));
            Assert.That(item1.tickCount, Is.EqualTo(0));
            Assert.That(item2.tickCount, Is.EqualTo(0));
            Assert.That(item3.tickCount, Is.EqualTo(0));
        }

        [Test, Description("Tick 中に Tick, Clear を再入実行できない")]
        public void Test_CannotTickOrClearRecursively()
        {
            var item = new TestItem("item");
            var list = new LifetimeList<TestItem>();
            item.onTick = () =>
            {
                Assert.Throws<InvalidOperationException>(() => list.Tick(0.1f));
                Assert.Throws<InvalidOperationException>(() => list.Clear());
            };
            list.Add(item);

            Assert.That(list.Tick(0.1f), Is.EqualTo(0));
            AssertItems(list, item);
        }

        [Test, Description("Clear 中に要素を追加できない")]
        public void Test_CannotAddDuringClear()
        {
            var item = new TestItem("item");
            var addedItem = new TestItem("addedItem");
            LifetimeList<TestItem> list = null;
            list = new(onRemoved: _ =>
            {
                Assert.Throws<InvalidOperationException>(() => list.Add(addedItem));
            });
            list.Add(item);

            list.Clear();

            AssertItems(list);
        }
    }
}
