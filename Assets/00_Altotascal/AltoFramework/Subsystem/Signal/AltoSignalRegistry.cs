using System;
using System.Collections;
using System.Collections.Generic;

namespace AltoFramework
{
    /// <summary>
    ///   Type-safed simple messaging system.
    /// </summary>
    public class AltoSignalRegistry
    {
        private Dictionary<Type, IAltoSignal> _signals = new Dictionary<Type, IAltoSignal>();

        public T GetOrCreate<T>() where T : IAltoSignal, new()
        {
            Type signalType = typeof(T);
            IAltoSignal signal;
            if (_signals.TryGetValue(signalType, out signal))
            {
                signal.OnGet();
                return (T)signal;
            }

            signal = (IAltoSignal)Activator.CreateInstance(signalType);
            _signals.Add(signalType, signal);
            signal.OnGet();
            return (T)signal;
        }

        public void Clear()
        {
            foreach (var signal in _signals.Values)
            {
                signal.Clear();
            }
            _signals.Clear();
        }

        public int Count
        {
            get { return _signals.Count; }
        }
    }
}
