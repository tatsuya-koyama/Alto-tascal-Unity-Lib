using System;
using NUnit.Framework;

namespace AltoFramework.Tests
{
    public class SignalTest
    {
        class Signal_A : AltoSignal {}
        class Signal_B : AltoSignal {}

        class Signal1 : AltoSignal<int> {}
        class Signal2 : AltoSignal<string, int> {}

        [Test]
        public void TestGetOrCreate()
        {
            var registry = new AltoSignalRegistry();
            var signalA1 = registry.GetOrCreate<Signal_A>();
            var signalA2 = registry.GetOrCreate<Signal_A>();
            var signalB1 = registry.GetOrCreate<Signal_B>();

            Assert.That(signalA1, Is.InstanceOf<Signal_A>());
            Assert.That(signalA2, Is.InstanceOf<Signal_A>());
            Assert.That(signalB1, Is.InstanceOf<Signal_B>());
            Assert.That(signalA1, Is.SameAs(signalA2));
        }

        [Test]
        public void TestClear()
        {
            var registry = new AltoSignalRegistry();
            Assert.That(registry.Count, Is.EqualTo(0));

            var signalA1 = registry.GetOrCreate<Signal_A>();
            var signalA2 = registry.GetOrCreate<Signal_A>();
            var signalB1 = registry.GetOrCreate<Signal_B>();
            Assert.That(registry.Count, Is.EqualTo(2));

            registry.Clear();
            Assert.That(registry.Count, Is.EqualTo(0));
        }

        [Test]
        public void TestEmit()
        {
            int result = 0;
            Action action = () => { result += 10; };

            var registry = new AltoSignalRegistry();
            registry.GetOrCreate<Signal_A>().Connect(action);
            Assert.That(result, Is.EqualTo(0));

            registry.GetOrCreate<Signal_A>().Emit();
            Assert.That(result, Is.EqualTo(10));
        }

        [Test]
        public void TestEmitWith1Arg()
        {
            int result = 0;
            Action<int> action = (int val) => { result += val; };

            var registry = new AltoSignalRegistry();
            registry.GetOrCreate<Signal1>().Connect(action);
            Assert.That(result, Is.EqualTo(0));

            registry.GetOrCreate<Signal1>().Emit(123);
            Assert.That(result, Is.EqualTo(123));
        }

        [Test]
        public void TestEmitWith2Args()
        {
            string seq = "";
            int result = 0;
            Action<string, int> action = (string str, int val) => {
                seq += str;
                result += val;
            };

            var registry = new AltoSignalRegistry();
            registry.GetOrCreate<Signal2>().Connect(action);
            Assert.That(result, Is.EqualTo(0));

            registry.GetOrCreate<Signal2>().Emit("hoge", 456);
            Assert.That(seq, Is.EqualTo("hoge"));
            Assert.That(result, Is.EqualTo(456));
        }

        [Test]
        public void TestConnectOnce()
        {
            string seq = "";
            Action action1 = () => { seq += "1"; };
            Action action2 = () => { seq += "2"; };
            Action action3 = () => { seq += "3"; };

            var signal = new Signal_A();
            signal.Connect(action1);
            signal.ConnectOnce(action2);
            signal.Connect(action3);

            signal.Emit();
            Assert.That(seq, Is.EqualTo("132"), "ConnectOnce の方が後に実行される");

            signal.Emit();
            Assert.That(seq, Is.EqualTo("13213"), "ConnectOnce したものは一度しか実行されない");
        }

        [Test]
        public void TestDisconnect()
        {
            string seq = "";
            Action action1 = () => { seq += "1"; };
            Action action2 = () => { seq += "2"; };
            Action action3 = () => { seq += "3"; };
            Action action4 = () => { seq += "4"; };

            var signal = new Signal_A();
            signal.Connect(action1);
            signal.Connect(action1);
            signal.Connect(action2);

            signal.Disconnect(action1);
            signal.Disconnect(action3);
            signal.Emit();
            Assert.That(seq, Is.EqualTo("12"),
                "同じ delegate が複数登録されていた場合、Disconnect は 1 つずつ削除する");

            signal.Connect(action3);
            signal.ConnectOnce(action3);
            signal.ConnectOnce(action4);
            signal.Disconnect(action3);
            signal.Emit();
            Assert.That(seq, Is.EqualTo("12124"),
                "同じ delegate が Connect & ConnectOnce されていた場合は双方から 1 つずつ削除する");

        }
    }
}
