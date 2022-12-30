using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace AltoLib
{
    public interface IAltoFSM
    {
        IAltoFSM parentFsm { get; }
        bool IsState<TState>();
        void SendEvent(ValueType eventId);
        void Update();
    }

    public partial class AltoFSM<TContext> : IAltoFSM
    {
        public bool logVerbose = false;

        public IAltoFSM parentFsm { get; private set; }

        TContext _context;
        List<AltoState> _registeredStates = new List<AltoState>();
        AltoState _currentState;
        Queue<ValueType> _eventQueue = new Queue<ValueType>();
        bool _isHandlingEvent = false;
        string _logIndent = "";

        public class AnyState : AltoState {}

        public AltoFSM(
            TContext context, IAltoFSM parentFsm = null,
            bool logVerbose = false, int logDepth = 0
        )
        {
            _context = context;
            this.parentFsm = parentFsm;
            this.logVerbose = logVerbose;

            for (int i = 0; i < Math.Min(logDepth, 8); ++i)
            {
                _logIndent += "---";
            }
        }

        //----------------------------------------------------------------------
        // public
        //----------------------------------------------------------------------

        public AltoState currentState => _currentState;

        public bool IsState<TState>()
        {
            if (_currentState == null) { return false; }
            return _currentState.GetType() == typeof(TState);
        }

        /// <summary>
        /// ステートを変更する。 Exit / Enter のハンドラは呼ばれない。
        /// 初期ステートの設定に使う
        /// </summary>
        public void SetState<TState>() where TState : AltoState, new()
        {
            _currentState = GetOrCreateState<TState>();
        }

        /// <summary>
        /// ステートを強制遷移させる。 Exit / Enter のハンドラが呼ばれる。
        /// AddTransition() で設定されていない遷移でも実行される
        /// </summary>
        public void ChangeState<TState>() where TState : AltoState, new()
        {
            var nextState = GetOrCreateState<TState>();
            ChangeStateInternal(nextState);
        }

        /// <summary>
        /// 遷移条件を設定する。 AddTransition<A, B>(E) とすると
        /// SendEvent(E) で A ステートから B ステートに遷移可能になる
        /// </summary>
        public bool AddTransition<TPrevState, TNextState>(ValueType triggerEventId)
            where TPrevState : AltoState, new()
            where TNextState : AltoState, new()
        {
            // Note. Enum を毎回 int にキャストして使うのが煩わしかったので
            //   実装しやすさを優先して ValueType で受け取ったものをキャストしている
            int eventId = (int)triggerEventId;

            var prevState = GetOrCreateState<TPrevState>();
            var nextState = GetOrCreateState<TNextState>();

            if (prevState._transitionTable.ContainsKey(eventId))
            {
                LogError($"Event already registered : {prevState.GetType().Name} / {eventId}");
                return false;
            }

            prevState._transitionTable.Add(eventId, nextState);
            return true;
        }

        /// <summary>
        /// どのステートからでも遷移可能な遷移条件を登録する
        /// </summary>
        public bool AddFreeTransition<TNextState>(ValueType triggerEventId) where TNextState : AltoState, new()
        {
            return AddTransition<AnyState, TNextState>(triggerEventId);
        }

        /// <summary>
        /// 遷移イベントを送り、事前に設定された遷移条件に従ってステート遷移を行う。
        /// 現在ステートが対応するイベントでなかったり、Guard() によってガードされた場合は
        /// 遷移は行われない。SendEvent() によってさらに SendEvent() が呼ばれるケース
        /// （State 自身が Enter 内で SendEvent() を呼ぶなど）は許されているが、
        /// 10 回以上 SendEvent() が連鎖した場合はエラーとしている
        /// </summary>
        public void SendEvent(ValueType eventId)
        {
            if (_isHandlingEvent)
            {
                _eventQueue.Enqueue(eventId);
                return;
            }
            _isHandlingEvent = true;

            SendEventInternal(eventId);
            int tryCount = 10;
            while (_eventQueue.Count > 0)
            {
                --tryCount;
                if (tryCount < 0)
                {
                    LogError("Too many SendEvent loops! Check the transition logic.");
                    _isHandlingEvent = false;
                    return;
                }
                SendEventInternal(_eventQueue.Dequeue());
            }
            _isHandlingEvent = false;
        }

        public void Update()
        {
            if (_currentState == null) { return; }
            _currentState.Update();
        }

        //----------------------------------------------------------------------
        // 単純な一方向遷移を手軽に登録するためのヘルパー。
        // メソッドチェーンで以下のように書ける：
        //
        //     fsm.AddSequentialTransitions<State_1st>(StateEvent.GoNext)
        //        .Then<State_2nd>()
        //        .Then<State_3rd>();
        //----------------------------------------------------------------------

        AltoState _prevSequentialState;
        ValueType _sequentialTrigger;

        public AltoFSM<TContext> AddSequentialTransitions<TFirstState>(ValueType triggerEventId)
            where TFirstState : AltoState, new()
        {
            _prevSequentialState = GetOrCreateState<TFirstState>();
            _sequentialTrigger = triggerEventId;
            return this;
        }

        public AltoFSM<TContext> Then<TNextState>() where TNextState : AltoState, new()
        {
            if (_prevSequentialState == null)
            {
                LogError("Please start from AddSequentialTransitions()");
                return null;
            }

            int eventId = (int)_sequentialTrigger;
            var nextState = GetOrCreateState<TNextState>();

            if (_prevSequentialState._transitionTable.ContainsKey(eventId))
            {
                LogError($"Event already registered : {_prevSequentialState.GetType().Name} / {eventId}");
                return null;
            }

            _prevSequentialState._transitionTable.Add(eventId, nextState);
            _prevSequentialState = nextState;
            return this;
        }

        //----------------------------------------------------------------------
        // private
        //----------------------------------------------------------------------

        [Conditional("ALTO_DEBUG")]
        void Log(string message)
        {
            if (!logVerbose) { return; }
            UnityEngine.Debug.Log($"<color=#9086e9>[AltoFSM]{_logIndent} </color>{message}");
        }

        [Conditional("ALTO_DEBUG")]
        void LogError(string message)
        {
            UnityEngine.Debug.LogError($"<color=#9086e9>[AltoFSM]{_logIndent} </color> [Error] {message}");
        }

        TState GetOrCreateState<TState>() where TState : AltoState, new()
        {
            var stateType = typeof(TState);
            foreach (var state in _registeredStates)
            {
                if (state.GetType() == stateType)
                {
                    return (TState)state;
                }
            }

            var newState = new TState();
            newState.context = _context;
            newState.fsm = this;
            newState.Init();
            _registeredStates.Add(newState);

            return newState;
        }

        /// <summary>
        /// 現在のステート、または AnyState から eventId に対応する次のステートを取得。
        /// もし同じイベントが AddTransition() と AddFreeTransition() の双方で登録されていた場合は
        /// 現在ステートからの遷移条件の方が優先される
        /// </summary>
        AltoState GetNextState(ValueType _eventId)
        {
            int eventId = (int)_eventId;
            var anyState = GetOrCreateState<AnyState>();
            AltoState nextState;

            // まずガード条件を見る
            if (_currentState != null && _currentState.Guard(_eventId)) { return null; }

            // 初期ステートが未設定だった場合は AnyState からの遷移を見る
            if (_currentState == null)
            {
                nextState = anyState.GetNextState(eventId);
                if (nextState != null) { return nextState; }
            }

            // 現在ステートからの次ステートを得る
            nextState = _currentState.GetNextState(eventId);
            if (nextState != null) { return nextState; }

            return anyState.GetNextState(eventId);
        }

        bool SendEventInternal(ValueType _eventId)
        {
            var nextState = GetNextState(_eventId);
            if (nextState == null) { return false; }

            ChangeStateInternal(nextState);
            return true;
        }

        void ChangeStateInternal(AltoState nextState)
        {
            if (logVerbose)
            {
                string currentStateName = (_currentState != null) ? _currentState.GetType().Name : "null";
                Log($"{currentStateName} <color=#f894fc>-></color> {nextState.GetType().Name}");
            }

            if (_currentState != null)
            {
                _currentState.onExitNextState = nextState;
                _currentState.Exit();
            }
            nextState.onEnterPrevState = _currentState;
            _currentState = nextState;
            nextState.Enter();
        }
    }
}
