using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace AltoFramework
{
    /// <summary>
    ///   Lightweight tween utility for general use.
    /// </summary>
    public class AltoTween : IAltoTween
    {
        float _from;
        float _to;
        float _duration;
        AltoEasingFunc _easingFunc;
        AltoTweenCallback _onUpdate;
        AltoTweenCallback _onComplete;

        float _passedTime = 0f;

        public AltoTween(
            float from = 0f, float to = 0f, float duration = 0f,
            AltoEasingFunc easingFunc = null,
            AltoTweenCallback onUpdate = null,
            AltoTweenCallback onComplete = null
        )
        {
            _from       = from;
            _to         = to;
            _duration   = duration;
            _easingFunc = easingFunc;
            _onUpdate   = onUpdate;
            _onComplete = onComplete;
        }

        //----------------------------------------------------------------------
        // IAltoTween implementation
        //----------------------------------------------------------------------

        public IAltoTween FromTo(
            float from, float to, float duration,
            AltoEasingFunc easingFunc = null
        )
        {
            _from       = from;
            _to         = to;
            _duration   = duration;
            _easingFunc = (easingFunc != null) ? easingFunc : AltoEase.Linear;
            return this;
        }

        public IAltoTween OnUpdate(AltoTweenCallback onUpdate)
        {
            _onUpdate = onUpdate;
            _passedTime = 0;
            Update(0);
            return this;
        }

        public IAltoTween OnComplete(AltoTweenCallback onComplete)
        {
            _onComplete = onComplete;
            return this;
        }

        public async UniTask Async()
        {
            await UniTask.WaitUntil(() => IsCompleted());
        }

        public IAltoTween SetAlpha(Graphic graphic)
        {
            return OnUpdate(x => {
                var color = graphic.color;
                color.a = x;
                graphic.color = color;
            });
        }

        public IAltoTween SetAlpha(CanvasGroup canvasGroup)
        {
            return OnUpdate(x => {
                canvasGroup.alpha = x;
            });
        }

        //----------------------------------------------------------------------

        public void Init()
        {
            _passedTime = 0;
            Update(0);
        }

        public void Update(float deltaTime)
        {
            if (IsCompleted()) { return; }
            AltoAssert.IsNotNull(_easingFunc);
            AltoAssert.IsNotNull(_onUpdate);

            _passedTime += deltaTime;
            if (_passedTime >= _duration)
            {
                Complete();
                return;
            }

            float t = _easingFunc(_passedTime / _duration);
            float x = Mathf.Lerp(_from, _to, t);
            _onUpdate(x);
        }

        public void Complete()
        {
            _passedTime = _duration;
            _onUpdate(_to);
            _onComplete?.Invoke(_to);
        }

        public bool IsCompleted()
        {
            return (_passedTime >= _duration);
        }
    }
}
