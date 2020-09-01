using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace AltoFramework
{
    public delegate float AltoEasingFunc(float passedTime);
    public delegate void AltoTweenCallback(float value);

    /// <summary>
    ///   Tween interface for framework user.
    /// </summary>
    public interface IAltoTween
    {
        IAltoTween FromTo(float from, float to, float duration, AltoEasingFunc easingFunc = null);
        IAltoTween OnUpdate(AltoTweenCallback onUpdate);
        IAltoTween OnComplete(AltoTweenCallback onComplete);
        UniTask Async();

        //----------------------------------------------------------------------
        // Helpers for Unity objects
        //----------------------------------------------------------------------

        IAltoTween SetAlpha(Graphic graphic);
        IAltoTween SetAlpha(CanvasGroup canvasGroup);
    }
}
