using System;
using System.Collections.Generic;
using UnityEngine;

namespace AltoLib
{
    public partial class AltoFSM<TContext>
    {
        /// <summary>
        /// ステートを定義するクラス。 AltoFSM<T> の内部クラスになっている
        /// </summary>
        public class AltoState
        {
            protected internal virtual void Enter() {}
            protected internal virtual void Exit() {}
            protected internal virtual void Update() {}

            /// <summary>
            /// true を返すとイベントによる遷移をキャンセルする
            /// </summary>
            protected internal virtual bool Guard(ValueType eventId) { return false; }

            protected internal TContext context;

            // onEnter / onExit で直前・直後の State 情報が知りたかったら以下を見る。
            // Enter() / Exit() の引数にとっていないのは引数の型が長くなりがちで実装時に面倒なため
            protected internal AltoState onEnterPrevState;
            protected internal AltoState onExitNextState;

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
