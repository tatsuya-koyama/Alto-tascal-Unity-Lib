using System;

namespace AltoFramework
{
    public interface IAltoSignal
    {
        void Clear();
    }

    /// <summary>
    ///   Typed message class with no arguments.
    /// </summary>
    public abstract class AltoSignal : IAltoSignal
    {
        private Action _callbacks;
        private Action _oneShotCallbacks;

        public void Connect(Action action)
        {
            _callbacks += action;
        }

        public void ConnectOnce(Action action)
        {
            _oneShotCallbacks += action;
        }

        public void Disconnect(Action action)
        {
            _callbacks -= action;
            _oneShotCallbacks -= action;
        }

        public void Emit()
        {
            _callbacks?.Invoke();
            _oneShotCallbacks?.Invoke();
            _oneShotCallbacks = null;
        }

        public void Clear()
        {
            _callbacks = null;
            _oneShotCallbacks = null;
        }
    }

    /// <summary>
    ///   Typed message class with 1 argument.
    /// </summary>
    public abstract class AltoSignal<T> : IAltoSignal
    {
        private Action<T> _callbacks;
        private Action<T> _oneShotCallbacks;

        public void Connect(Action<T> action)
        {
            _callbacks += action;
        }

        public void ConnectOnce(Action<T> action)
        {
            _oneShotCallbacks += action;
        }

        public void Disconnect(Action<T> action)
        {
            _callbacks -= action;
            _oneShotCallbacks -= action;
        }

        public void Emit(T arg)
        {
            _callbacks?.Invoke(arg);
            _oneShotCallbacks?.Invoke(arg);
            _oneShotCallbacks = null;
        }

        public void Clear()
        {
            _callbacks = null;
            _oneShotCallbacks = null;
        }
    }

    /// <summary>
    ///   Typed message class with 2 argument.
    /// </summary>
    public abstract class AltoSignal<T1, T2> : IAltoSignal
    {
        private Action<T1, T2> _callbacks;
        private Action<T1, T2> _oneShotCallbacks;

        public void Connect(Action<T1, T2> action)
        {
            _callbacks += action;
        }

        public void ConnectOnce(Action<T1, T2> action)
        {
            _oneShotCallbacks += action;
        }

        public void Disconnect(Action<T1, T2> action)
        {
            _callbacks -= action;
            _oneShotCallbacks -= action;
        }

        public void Emit(T1 arg1, T2 arg2)
        {
            _callbacks?.Invoke(arg1, arg2);
            _oneShotCallbacks?.Invoke(arg1, arg2);
            _oneShotCallbacks = null;
        }

        public void Clear()
        {
            _callbacks = null;
            _oneShotCallbacks = null;
        }
    }
}
