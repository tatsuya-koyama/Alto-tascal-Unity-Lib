using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace AltoFramework.Production
{
    public class TimeKeeper : ITimeKeeper
    {
        public float maxDeltaTime = 1 / 30f;

        public float dt
        {
            get
            {
                // Time.maximumDeltaTime を利用してもよいが、自前のコードでラップしておく
                return Mathf.Min(Time.deltaTime, maxDeltaTime);
            }
        }

        public async UniTask Wait(float seconds, Action action, bool ignoreTimeScale = true)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(seconds), ignoreTimeScale);
            action();
        }

        public async UniTask WaitFrame(int frame, Action action)
        {
            await UniTask.DelayFrame(frame);
            action();
        }
    }
}
