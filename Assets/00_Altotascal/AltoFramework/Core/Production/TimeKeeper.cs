using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using UnityEngine;

namespace AltoFramework.Production
{
    public class TimeKeeper : ITimeKeeper
    {
        public float maxDeltaTime = 1 / 20f;

        public float dt
        {
            get
            {
                // Time.maximumDeltaTime を利用してもよいが、自前のコードでラップしておく
                return Mathf.Min(Time.deltaTime, maxDeltaTime);
            }
        }

        /// <summary>
        /// 各シーンでの経過時間
        /// </summary>
        public float t => _t;
        float _t = 0f;

        public TimeKeeper(ISceneDirector sceneDirector)
        {
            sceneDirector.sceneLoading += OnSceneLoading;
            sceneDirector.sceneUpdate  += OnSceneUpdate;
        }

        void OnSceneLoading()
        {
            _t = 0f;
        }

        void OnSceneUpdate()
        {
            _t += dt;
        }

        /// <summary>
        /// ゲーム内時間で指定秒数待つ
        /// ※ UniTask.Delay() ではスパイク発生時に処理が飛ぶような挙動があったため、
        ///    Timekeeper.dt ベースで自前実装
        /// </summary>
        public async UniTask Wait(float seconds)
        {
            await WaitCoroutine(seconds);
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

        public async void Delay(float seconds, Action action)
        {
            await WaitCoroutine(seconds);
            action?.Invoke();
        }

        public async UniTask WaitFrame(int frame)
        {
            await UniTask.DelayFrame(frame);
        }

        public async void DelayFrame(int frame, Action action = null)
        {
            await UniTask.DelayFrame(frame);
            action?.Invoke();
        }
    }
}
