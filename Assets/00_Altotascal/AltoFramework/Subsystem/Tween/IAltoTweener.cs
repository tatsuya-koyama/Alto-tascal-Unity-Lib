namespace AltoFramework
{
    /// <summary>
    ///   Tweener interface for framework user.
    /// </summary>
    public interface IAltoTweener
    {
        int count { get; }

        void Go(
            object obj, float from, float to, float duration,
            AltoEasingFunc easingFunc,
            AltoTweenCallback onUpdate
        );

        IAltoTween NewTween(object obj = null);
        void Finish(object obj);
        void ClearAll();
    }
}
