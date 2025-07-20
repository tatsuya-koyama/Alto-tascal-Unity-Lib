using System;
using UnityEngine;

namespace AltoFramework
{
    public interface IAltoSignal
    {
        void OnGet();
        void Clear();
    }

    /// <summary>
    /// Emit() 内で Emit() されたときの無限ループ検出用
    /// </summary>
    internal class AltoSignalGuard
    {
        public const int MaxEmitInFrame = 999;

        public static void Abort(object context)
        {
            throw new Exception($"[{context.GetType().FullName}] Signal's Emit seems to be stuck in infinite loop");
        }
    }

    /// <summary>
    ///   Typed message class with no arguments.
    /// </summary>
    public abstract class AltoSignal : IAltoSignal
    {
        Action _callbacks;
        Action _oneShotCallbacks;
        Action _lastCallback;
        int _emitCount = 0;

        public void OnGet()
        {
            _lastCallback = null;
        }

        public void Clear()
        {
            _callbacks = null;
            _oneShotCallbacks = null;
        }

        public AltoSignal Connect(Action action)
        {
            _callbacks += action;
            _lastCallback = action;
            return this;
        }

        public AltoSignal ConnectOnce(Action action)
        {
            _oneShotCallbacks += action;
            _lastCallback = action;
            return this;
        }

        public void Disconnect(Action action)
        {
            _callbacks -= action;
            _oneShotCallbacks -= action;
        }

        public void Emit()
        {
            ++_emitCount;
            if (_emitCount > AltoSignalGuard.MaxEmitInFrame)
            {
                AltoSignalGuard.Abort(this);
                return;
            }

            _callbacks?.Invoke();
            _oneShotCallbacks?.Invoke();
            _oneShotCallbacks = null;
            _emitCount = 0;
        }

        public void LifeLink(Component component)
        {
            AltoSignalLifeLinkHelper.LifeLink(component, this, _lastCallback);
        }
    }

    /// <summary>
    ///   Typed message class with 1 argument.
    /// </summary>
    public abstract class AltoSignal<T> : IAltoSignal
    {
        Action<T> _callbacks;
        Action<T> _oneShotCallbacks;
        Action<T> _lastCallback;
        int _emitCount = 0;

        public void OnGet()
        {
            _lastCallback = null;
        }

        public void Clear()
        {
            _callbacks = null;
            _oneShotCallbacks = null;
        }

        public AltoSignal<T> Connect(Action<T> action)
        {
            _callbacks += action;
            _lastCallback = action;
            return this;
        }

        public AltoSignal<T> ConnectOnce(Action<T> action)
        {
            _oneShotCallbacks += action;
            _lastCallback = action;
            return this;
        }

        public void Disconnect(Action<T> action)
        {
            _callbacks -= action;
            _oneShotCallbacks -= action;
        }

        public void Emit(T arg)
        {
            ++_emitCount;
            if (_emitCount > AltoSignalGuard.MaxEmitInFrame)
            {
                AltoSignalGuard.Abort(this);
                return;
            }

            _callbacks?.Invoke(arg);
            _oneShotCallbacks?.Invoke(arg);
            _oneShotCallbacks = null;
            _emitCount = 0;
        }

        public void LifeLink(Component component)
        {
            AltoSignalLifeLinkHelper.LifeLink(component, this, _lastCallback);
        }
    }

    /// <summary>
    ///   Typed message class with 2 argument.
    /// </summary>
    public abstract class AltoSignal<T1, T2> : IAltoSignal
    {
        Action<T1, T2> _callbacks;
        Action<T1, T2> _oneShotCallbacks;
        Action<T1, T2> _lastCallback;
        int _emitCount = 0;

        public void OnGet()
        {
            _lastCallback = null;
        }

        public void Clear()
        {
            _callbacks = null;
            _oneShotCallbacks = null;
        }

        public AltoSignal<T1, T2> Connect(Action<T1, T2> action)
        {
            _callbacks += action;
            _lastCallback = action;
            return this;
        }

        public AltoSignal<T1, T2> ConnectOnce(Action<T1, T2> action)
        {
            _oneShotCallbacks += action;
            _lastCallback = action;
            return this;
        }

        public void Disconnect(Action<T1, T2> action)
        {
            _callbacks -= action;
            _oneShotCallbacks -= action;
        }

        public void Emit(T1 arg1, T2 arg2)
        {
            ++_emitCount;
            if (_emitCount > AltoSignalGuard.MaxEmitInFrame)
            {
                AltoSignalGuard.Abort(this);
                return;
            }

            _callbacks?.Invoke(arg1, arg2);
            _oneShotCallbacks?.Invoke(arg1, arg2);
            _oneShotCallbacks = null;
            _emitCount = 0;
        }

        public void LifeLink(Component component)
        {
            AltoSignalLifeLinkHelper.LifeLink(component, this, _lastCallback);
        }
    }
}
