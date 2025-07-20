using System;
using UnityEngine;

namespace AltoFramework
{
    public class AltoSignalLifeLinkHelper
    {
        static AltoSignalDestroyTrigger GetOrCreateTrigger(Component component)
        {
            var trigger = component.GetComponent<AltoSignalDestroyTrigger>();
            if (trigger == null)
            {
                trigger = component.gameObject.AddComponent<AltoSignalDestroyTrigger>();
            }
            return trigger;
        }

        static void LogError()
        {
            Alto.Log.Error("LifeLink() should use after Connect() or ConnectOnce()");
        }

        public static void LifeLink(Component component, AltoSignal signal, Action action)
        {
            if (action == null) { LogError(); return; }

            var trigger = GetOrCreateTrigger(component);
            trigger.ListenDestroy(() =>
            {
                signal.Disconnect(action);
            });
        }

        public static void LifeLink<T>(Component component, AltoSignal<T> signal, Action<T> action)
        {
            if (action == null) { LogError(); return; }

            var trigger = GetOrCreateTrigger(component);
            trigger.ListenDestroy(() =>
            {
                signal.Disconnect(action);
            });
        }

        public static void LifeLink<T1, T2>(Component component, AltoSignal<T1, T2> signal, Action<T1, T2> action)
        {
            if (action == null) { LogError(); return; }

            var trigger = GetOrCreateTrigger(component);
            trigger.ListenDestroy(() =>
            {
                signal.Disconnect(action);
            });
        }
    }
}
