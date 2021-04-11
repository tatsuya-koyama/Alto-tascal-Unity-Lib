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

        UniTask Wait(float seconds, Action action = null);
        UniTask WaitFrame(int frame, Action action = null);
    }
}
