using Cysharp.Threading.Tasks;
using System;
using System.Collections;
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

        /// <summary>
        /// ゲーム内時間で指定秒数待つ
        /// ※ UniTask.Delay() ではスパイク発生時に処理が飛ぶような挙動があったため、
        ///    Timekeeper.dt ベースで自前実装
        /// </summary>
        public async UniTask Wait(float seconds, Action action = null)
        {
            await WaitCoroutine(seconds);
            action?.Invoke();
        }

        IEnumerator WaitCoroutine(float seconds)
        {
            float elapsedSec = 0;
            while (elapsedSec < seconds)
            {
                elapsedSec += this.dt;
                yield return null;
            }
        }

        public async UniTask WaitFrame(int frame, Action action = null)
        {
            await UniTask.DelayFrame(frame);
            action?.Invoke();
        }
    }
}
