using System;
using UnityEngine;

namespace AltoFramework
{
    /// <summary>
    /// AudioSource と現在のボリューム値を保持し、フェードイン・アウトを制御
    /// </summary>
    public class AudioSourceState
    {
        public GameObject gameObject;
        public AudioSource source;
        public float originalVolume;
        public bool isFading { get; private set; } = false;
        public Action onStop = null;

        float _volumeTo;
        float _volumeFrom;
        float _fadeTime;
        float _fadeProgress;

        public void SetVolumeAtOnce(float volume)
        {
            source.volume = volume;
            isFading = false;
        }

        public void SetFade(float volumeFrom, float volumeTo, float fadeTime)
        {
            _volumeFrom = volumeFrom;
            _volumeTo   = volumeTo;
            _fadeTime   = fadeTime;

            source.volume = volumeFrom;
            _fadeProgress = 0f;
            isFading = true;
        }

        public void SetTargetVolume(float volume)
        {
            if (_volumeFrom < _volumeTo) { _volumeTo = volume; }
            if (!isFading) { source.volume = volume; }
        }

        public void StopSource()
        {
            if (!source.isPlaying) { return; }

            source.Stop();
            onStop?.Invoke();
            onStop = null;
        }

        public void Update(float dt)
        {
            UpdateStopCallback();

            if (!isFading) { return; }
            if (_fadeTime <= 0f) { return; }

            _fadeProgress += dt;
            float volumeRate = CalcVolumeRate(_fadeProgress, _fadeTime, _volumeFrom, _volumeTo);
            source.volume = (_volumeTo - _volumeFrom) * volumeRate + _volumeFrom;

            if (_fadeProgress >= _fadeTime) { isFading = false; }
        }

        /// <summary>
        /// StopSource() を呼ばれるのでなく、ループなしで単に再生し終わって
        /// 停止状態になったときにコールバックを呼んであげる処理
        /// </summary>
        void UpdateStopCallback()
        {
            if (onStop == null) { return; }

            if (!source.isPlaying)
            {
                onStop?.Invoke();
                onStop = null;
            }
        }

        // 経過時間からボリューム比を計算。
        // 線形ではなく、音量が小さくなるにつれてゆっくりボリュームが下がっていくような値を返す
        float CalcVolumeRate(float progress, float time, float from, float to)
        {
            var rate = Mathf.Clamp01(_fadeProgress / _fadeTime);

            // フェードイン時
            if (from < to)
            {
                return rate * rate;
            }

            // フェードアウト時
            return 1f - (1f - rate) * (1f - rate);
        }
    }
}
