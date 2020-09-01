using Cysharp.Threading.Tasks;
using System;

namespace AltoFramework
{
    public interface ITimeKeeper
    {
        // Wrap Time.deltaTime
        float dt { get; }

        UniTask Wait(float seconds, Action action, bool ignoreTimeScale = true);
        UniTask WaitFrame(int frame, Action action);
    }
}
