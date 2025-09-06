using System;
using NUnit.Framework;

namespace AltoLib.Tests
{
    public class AltoFSMTest
    {
        public static string sequence;

        class TestController
        {
            public string sequence = "";
            public bool guardFromTalk = false;
        }

        class StandState : AltoFSM<TestController>.AltoState
        {
            public override void Enter()  { context.sequence += "_in1"; }
            public override void Exit()   { context.sequence += "_out1"; }
            public override void Update() { context.sequence += "_up1"; }
        }

        class WalkState : AltoFSM<TestController>.AltoState
        {
            public override void Enter()  { context.sequence += "_in2"; }
            public override void Exit()   { context.sequence += "_out2"; }
            public override void Update() { context.sequence += "_up2"; }
        }

        class RunState : AltoFSM<TestController>.AltoState
        {
            public override void Enter()  { context.sequence += "_in3"; }
            public override void Exit()   { context.sequence += "_out3"; }
            public override void Update() { context.sequence += "_up3"; }
        }

        class TalkState : AltoFSM<TestController>.AltoState
        {
            public override void Enter()  { context.sequence += "_in4"; }
            public override void Exit()   { context.sequence += "_out4"; }
            public override void Update() { context.sequence += "_up4"; }

            public override bool Guard(ValueType eventId)
            {
                return context.guardFromTalk;
            }
        }

        class FlashState : AltoFSM<TestController>.AltoState
        {
            public override void Enter()  { context.sequence += "_in5"; SendEvent(StateEvent.GoNext); }
            public override void Exit()   { context.sequence += "_out5"; }
            public override void Update() { context.sequence += "_up5"; }
        }

        public enum StateEvent
        {
            Stand, Walk, Run, Talk, Flash, GoNext
        }

        void SetTransition(AltoFSM<TestController> fsm)
        {
            // Stand <--> Walk <--> Run
            fsm.AddTransition<StandState, WalkState >(StateEvent.Walk);
            fsm.AddTransition<WalkState , RunState  >(StateEvent.Run);
            fsm.AddTransition<RunState  , WalkState >(StateEvent.Walk);
            fsm.AddTransition<WalkState , StandState>(StateEvent.Stand);

            // [AnyState] -> Talk, Flash
            fsm.AddFreeTransition<TalkState>(StateEvent.Talk);
            fsm.AddFreeTransition<FlashState>(StateEvent.Flash);

            // Talk, Flash -> Stand
            fsm.AddTransition<TalkState, StandState>(StateEvent.Stand);
            fsm.AddTransition<FlashState, StandState>(StateEvent.GoNext);
        }

        [Test]
        public void TestBasic()
        {
            var controller = new TestController();
            var fsm = new AltoFSM<TestController>(controller);
            SetTransition(fsm);

            fsm.SetState<StandState>();
            Assert.That(fsm.currentState.GetType(), Is.EqualTo(typeof(StandState)));

            fsm.SendEvent(StateEvent.Run);
            fsm.SendEvent(StateEvent.Stand);
            fsm.SendEvent(StateEvent.Walk);

            Assert.That(fsm.currentState.GetType(), Is.EqualTo(typeof(WalkState)));
            Assert.That(fsm.IsState<WalkState>, Is.True);
            Assert.That(fsm.IsState<RunState>, Is.False);
        }

        [Test]
        public void TestFreeTransition()
        {
            var controller = new TestController();
            var fsm = new AltoFSM<TestController>(controller);
            SetTransition(fsm);

            fsm.SetState<StandState>();
            fsm.SendEvent(StateEvent.Talk);
            Assert.That(fsm.IsState<TalkState>, Is.True);

            fsm.SetState<WalkState>();
            fsm.SendEvent(StateEvent.Talk);
            Assert.That(fsm.IsState<TalkState>, Is.True);
        }

        [Test]
        public void TestTransitionEventHandlers()
        {
            var controller = new TestController();
            var fsm = new AltoFSM<TestController>(controller);
            SetTransition(fsm);
            fsm.SetState<StandState>();

            fsm.SendEvent(StateEvent.Walk);
            fsm.Update();
            fsm.SendEvent(StateEvent.Run);

            Assert.That(controller.sequence, Is.EqualTo("_out1_in2_up2_out2_in3"));
        }

        [Test]
        public void TestSendEventFromState()
        {
            var controller = new TestController();
            var fsm = new AltoFSM<TestController>(controller);
            SetTransition(fsm);
            fsm.ChangeState<StandState>();

            fsm.SendEvent(StateEvent.Flash);
            fsm.Update();
            fsm.SendEvent(StateEvent.Walk);

            Assert.That(controller.sequence, Is.EqualTo("_in1_out1_in5_out5_in1_up1_out1_in2"));
        }

        [Test]
        public void TestGuard()
        {
            var controller = new TestController();
            var fsm = new AltoFSM<TestController>(controller);
            SetTransition(fsm);
            fsm.SetState<StandState>();

            fsm.SendEvent(StateEvent.Talk);
            Assert.That(fsm.IsState<TalkState>(), Is.True);

            fsm.SendEvent(StateEvent.Stand);
            Assert.That(fsm.IsState<StandState>(), Is.True);

            fsm.SetState<TalkState>();
            controller.guardFromTalk = true;
            fsm.SendEvent(StateEvent.Stand);
            Assert.That(fsm.IsState<StandState>(), Is.False, "ガードされて遷移しない");
        }

        [Test]
        public void TestSequentialTransition()
        {
            var controller = new TestController();
            var fsm = new AltoFSM<TestController>(controller);

            fsm.AddSequentialTransitions<StandState>(StateEvent.GoNext)
               .Then<WalkState>()
               .Then<RunState>()
               .Then<TalkState>();
            fsm.SetState<StandState>();

            fsm.SendEvent(StateEvent.GoNext);
            Assert.That(fsm.IsState<WalkState>(), Is.True);
            fsm.SendEvent(StateEvent.GoNext);
            Assert.That(fsm.IsState<RunState>(), Is.True);
            fsm.SendEvent(StateEvent.GoNext);
            Assert.That(fsm.IsState<TalkState>(), Is.True);
        }
    }
}
