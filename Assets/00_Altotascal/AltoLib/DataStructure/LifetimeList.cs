using System;
using System.Collections;
using System.Collections.Generic;

namespace AltoLib
{
    public interface ILifetimeTickable
    {
        /// <summary>
        /// 死亡時に true, 生存時に false を返す
        /// </summary>
        bool Tick(float deltaTime);
    }

    /// <summary>
    /// ゲームでよくある、生まれては消えていくオブジェクトの生存リストを管理し
    /// 効率よく走査と削除を行うためのデータ構造。
    ///
    /// 【使い方】
    /// - 生存管理される側の要素は IlifetimeTickable を実装する。
    ///   Tick はフレーム更新処理で、自身が死んだフレームに true を返すように実装する
    /// - あとは LifetimeList に対して通常の List のように Add() し、
    ///   フレーム更新契機で Tick() を呼ぶ。LifetimeList.Tick() は走査しながら
    ///   生存オブジェクトを List 先頭に詰め、最後に末尾削除を行う。
    ///   末尾削除は List の Capacity を変更しないので、メモリアロケーションコストは最小。
    /// - Tick() 中のオブジェクト追加は普通は行わない想定だが、仮に行った場合は
    ///   走査処理が終了後に List に追加される（そのフレームでの走査対象にはならない）
    /// </summary>
    public class LifetimeList<T> : IReadOnlyList<T> where T : class, ILifetimeTickable
    {
        readonly List<T> _items;
        readonly List<T> _pendingAdditions = new();
        readonly Action<T> _onRemoved;

        enum State { Idle, Ticking, Clearing, }
        State _state = State.Idle;

        public LifetimeList(int initialCapacity = 0, Action<T> onRemoved = null)
        {
            _items = new(initialCapacity);
            _onRemoved = onRemoved;
        }

        public void Add(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            switch (_state)
            {
                case State.Idle:
                    _items.Add(item);
                    break;
                case State.Ticking:
                    _pendingAdditions.Add(item);
                    break;
                case State.Clearing:
                    throw new InvalidOperationException("Cannot add item in Clearing state");
            }
        }

        public int Tick(float deltaTime)
        {
            ValidateIdleState();
            _state = State.Ticking;

            int originalCount = _items.Count;
            int readIndex = 0;
            int writeIndex = 0;
            int removedCount = 0;

            while (readIndex < originalCount)
            {
                var item = _items[readIndex];
                bool isDead = item.Tick(deltaTime);
                if (!isDead)
                {
                    // 生存オブジェクトを先頭に詰める
                    if (writeIndex != readIndex)
                    {
                        _items[writeIndex] = item;
                    }
                    ++writeIndex;
                }
                else
                {
                    ++removedCount;
                    _onRemoved?.Invoke(item);
                }
                ++readIndex;
            }

            if (removedCount > 0)
            {
                _items.RemoveRange(writeIndex, removedCount);
            }

            _state = State.Idle;
            FlushPendingAdditions();
            return removedCount;
        }

        public void Clear()
        {
            ValidateIdleState();
            _state = State.Clearing;

            while (_items.Count > 0)
            {
                int lastIndex = _items.Count - 1;
                var item = _items[lastIndex];
                _items.RemoveAt(lastIndex);
                _onRemoved?.Invoke(item);
            }

            _state = State.Idle;
        }

        void ValidateIdleState()
        {
            if (_state != State.Idle)
            {
                throw new InvalidOperationException("LifetimeList not Idle");
            }
        }

        void FlushPendingAdditions()
        {
            if (_pendingAdditions.Count == 0) { return; }

            _items.AddRange(_pendingAdditions);
            _pendingAdditions.Clear();
        }

        //----------------------------------------------------------------------
        // IReadOnlyList
        //----------------------------------------------------------------------

        public int Count => _items.Count;
        public T this[int index] => _items[index];

        public List<T>.Enumerator GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
