using Cysharp.Threading.Tasks;
using System;

namespace AltoFramework
{
    public interface ITimeKeeper
    {
        /// <summary>Wrapped Time.deltaTime</summary>
        float dt { get; }

        /// <summary>Elapsed time in scene (accumulated dt)</summary>
        float t { get; }

        UniTask Wait(float seconds);
        UniTask WaitFrame(int frame);
        void Delay(float seconds, Action action);
        void DelayFrame(int frame, Action action);
    }
}
