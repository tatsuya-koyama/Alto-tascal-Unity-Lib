using System;
using System.Collections.Generic;
using UnityEngine;

namespace AltoLib
{
    public partial class AltoFSM<TContext>
    {
        public bool logVerbose = false;

        TContext _context;
        List<AltoState> _registeredStates = new List<AltoState>();
        AltoState _currentState;

        public class AnyState : AltoState {}

        public AltoFSM(TContext context, bool logVerbose = false)
        {
            _context = context;
            this.logVerbose = logVerbose;
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
        /// 現在ステートが対応するイベントでなかったり、Guard() によってガードされて
        /// 遷移が行われなかった場合は false を返す。
        /// </summary>
        public bool SendEvent(ValueType _eventId)
        {
            var nextState = GetNextState(_eventId);
            if (nextState == null) { return false; }

            ChangeStateInternal(nextState);
            return true;
        }

        public void Update()
        {
            if (_currentState == null) { return; }
            _currentState.Update();
        }

        //----------------------------------------------------------------------
        // private
        //----------------------------------------------------------------------

        void Log(string message)
        {
            if (!logVerbose) { return; }
            Debug.Log($"<color=#8cc659>[AltoFSM]</color> {message}");
        }

        void LogError(string message)
        {
            Debug.LogError($"<color=#8cc659>[AltoFSM]</color> [Error] {message}");
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

        void ChangeStateInternal(AltoState nextState)
        {
            if (logVerbose)
            {
                string currentStateName = (_currentState != null) ? _currentState.GetType().Name : "null";
                Log($"State changed : {currentStateName} -> {nextState.GetType().Name}");
            }

            if (_currentState != null)
            {
                _currentState.onExitNextState = nextState;
                _currentState.Exit();
            }
            nextState.onEnterPrevState = _currentState;
            nextState.Enter();

            _currentState = nextState;
        }
    }
}
