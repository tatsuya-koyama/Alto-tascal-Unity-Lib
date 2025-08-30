using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("AltoLib.Tests")]
namespace AltoLib
{
    public partial class AltoFSM<TContext>
    {
        /// <summary>
        /// ステートを定義するクラス。 AltoFSM<T> の内部クラスになっている
        /// </summary>
        public class AltoState
        {
            /// <summary>
            /// 初期化処理。FSM に初めて AddTransition() したタイミングで呼ばれる
            /// </summary>
            protected internal virtual void Init() {}

            /// <summary>
            /// State に入るときに呼ばれる
            /// </summary>
            protected internal virtual void Enter() {}

            /// <summary>
            /// State から出るときに呼ばれる
            /// </summary>
            protected internal virtual void Exit() {}

            /// <summary>
            /// AltoFSM の Update() を呼ぶと呼ばれる
            /// </summary>
            protected internal virtual void Update() {}

            /// <summary>
            /// true を返すとイベントによる遷移をキャンセルする
            /// </summary>
            protected internal virtual bool Guard(ValueType eventId) { return false; }

            /// <summary>
            /// 自身で遷移イベントを送出する
            /// </summary>
            protected void SendEvent(ValueType _eventId)
            {
                fsm.SendEvent(_eventId);
            }

            /// <summary>
            /// 自身を持つ FSM に親 FSM がセットされていたら、親 FSM にイベントを送出する
            /// </summary>
            protected void SendEventToParentFsm(ValueType _eventId)
            {
                fsm.parentFsm?.SendEvent(_eventId);
            }

            protected internal TContext context;

            // onEnter / onExit で直前・直後の State 情報が知りたかったら以下を見る。
            // Enter() / Exit() の引数にとっていないのは引数の型が長くなりがちで実装時に面倒なため
            protected internal AltoState onEnterPrevState;
            protected internal AltoState onExitNextState;

            protected internal IAltoFSM fsm;

            //------------------------------------------------------------------
            // internal
            //------------------------------------------------------------------

            /// <summary>
            /// このイベントが来たらこのステートに遷移する、というマップ
            /// </summary>
            internal Dictionary<int, AltoState> _transitionTable = new Dictionary<int, AltoState>();

            internal AltoState GetNextState(int eventId)
            {
                AltoState nextState;
                if (!_transitionTable.TryGetValue(eventId, out nextState))
                {
                    return null;
                }
                return nextState;
            }

            //------------------------------------------------------------------
            // private
            //------------------------------------------------------------------

            void LogError(string message)
            {
                Debug.LogError($"<color=#59c694>[AltoState]</color> [Error] {message}");
            }
        }
    }
}
