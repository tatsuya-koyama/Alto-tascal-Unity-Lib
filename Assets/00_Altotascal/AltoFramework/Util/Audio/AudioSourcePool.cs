using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AltoFramework
{
    /// <summary>
    /// AltoFramework.AudioSourceState のプールを保持。
    /// AudioPlayer から使用される
    /// </summary>
    public class AudioSourcePool
    {
        List<AudioSourceState> _pool = new List<AudioSourceState>();
        int _head = 0;

        public AudioSourceState currentSourceState { get; private set; }

        public void Init(GameObject gameObject, string objectName, int numPool)
        {
            for (int i = 0; i < numPool; ++i)
            {
                var sourceObj = new GameObject($"{objectName} ({i + 1})");
                sourceObj.transform.SetParent(gameObject.transform);
                var audioSource = sourceObj.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                var audioState = new AudioSourceState()
                {
                    gameObject = sourceObj,
                    source = audioSource,
                };
                _pool.Add(audioState);
            }
        }

        public void Update(float dt)
        {
            foreach (var state in _pool) { state.Update(dt); }
        }

        //----------------------------------------------------------------------
        // Get States
        //----------------------------------------------------------------------

        public AudioSourceState GetNext()
        {
            ++_head;
            if (_head >= _pool.Count) { _head = 0; }
            currentSourceState = _pool[_head];
            return currentSourceState;
        }

        /// <summary>
        /// 指定した AudioClip を再生中の AudioSource のリストを取得
        /// </summary>
        public List<AudioSourceState> GetByAudioClip(AudioClip audioClip)
        {
            return _pool.Where(x => x.source.isPlaying && x.source.clip == audioClip).ToList();
        }

        public List<AudioSourceState> GetPlaying()
        {
            return _pool.Where(x => x.source.isPlaying).ToList();
        }

        public bool IsPlaying(AudioClip audioClip)
        {
            return GetByAudioClip(audioClip).Count > 0;
        }

        //----------------------------------------------------------------------
        // Stop
        //----------------------------------------------------------------------

        public void StopAll()
        {
            foreach (var state in _pool) { state.StopSource(); }
        }

        public void StopCurrent()
        {
            if (currentSourceState == null) { return; }
            currentSourceState.StopSource();
        }

        public void Stop(AudioClip audioClip)
        {
            foreach (var state in _pool)
            {
                if (state.source.clip == audioClip) { state.StopSource(); }
            }
        }

        //----------------------------------------------------------------------
        // Pause
        //----------------------------------------------------------------------

        public void PauseAll()
        {
            foreach (var state in _pool) { state.source.Pause(); }
        }

        public void PauseCurrent()
        {
            if (currentSourceState == null) { return; }
            currentSourceState.source.Pause();
        }

        public void Pause(AudioClip audioClip)
        {
            foreach (var state in _pool)
            {
                if (state.source.clip == audioClip) { state.source.Pause(); }
            }
        }

        //----------------------------------------------------------------------
        // Resume
        //----------------------------------------------------------------------

        public void ResumeAll()
        {
            foreach (var state in _pool) { state.source.UnPause(); }
        }

        public void ResumeCurrent()
        {
            if (currentSourceState == null) { return; }
            currentSourceState.source.UnPause();
        }

        public void Resume(AudioClip audioClip)
        {
            foreach (var state in _pool)
            {
                if (state.source.clip == audioClip) { state.source.UnPause(); }
            }
        }

    }
}
