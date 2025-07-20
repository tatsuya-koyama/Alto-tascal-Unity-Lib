using System;
using UnityEngine;

namespace AltoFramework
{
    public class AltoSignalDestroyTrigger : MonoBehaviour
    {
        Action _callbacks;

        void OnDestroy()
        {
            _callbacks?.Invoke();
            _callbacks = null;
        }

        public void ListenDestroy(Action action)
        {
            _callbacks += action;
        }
    }
}
