using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AltoFramework
{
    public interface IAudioPlayer
    {
        float systemMasterVolume { get; set; }
        float userMasterVolume { get; set; }

        void Init(
            GameObject gameObject, int numSourcePool,
            ISceneDirector sceneDirector, IResourceStore resourceStore
        );

        void Play(
            AudioClip audioClip, bool? loop = null, bool? mix = null, bool? replay = null,
            bool autoVolume = true, float volume = 1.0f, float pitch = 1.0f, float pan = 0f,
            float spatial = 0f, Vector3? position = null
        );
        void Play(
            string audioPath, bool? loop = null, bool? mix = null, bool? replay = null,
            bool autoVolume = true, float volume = 1.0f, float pitch = 1.0f, float pan = 0f,
            float spatial = 0f, Vector3? position = null
        );
        UniTask PlayOndemand(
            string audioPath, bool? loop = null, bool? mix = null, bool? replay = null,
            bool autoVolume = true, float volume = 1.0f, float pitch = 1.0f, float pan = 0f,
            float spatial = 0f, Vector3? position = null
        );

        void Stop(AudioClip audioClip = null);
        void Stop(string audioPath);

        UniTask FadeIn(
            AudioClip audioClip, float fadeTime, float volumeFrom = 0f, float volumeTo = 1f,
            bool? loop = null, bool? mix = null, bool? replay = null,
            bool autoVolume = true, float pitch = 1.0f, float pan = 0f,
            float spatial = 0f, Vector3? position = null
        );
        UniTask FadeIn(
            string audioPath, float fadeTime, float volumeFrom = 0f, float volumeTo = 1f,
            bool? loop = null, bool? mix = null, bool? replay = null,
            bool autoVolume = true, float pitch = 1.0f, float pan = 0f,
            float spatial = 0f, Vector3? position = null
        );
        UniTask FadeOut(
            float fadeTime, AudioClip audioClip = null, float volumeFrom = 1f, float volumeTo = 0f
        );
        UniTask FadeOut(
            float fadeTime, string audioPath, float volumeFrom = 1f, float volumeTo = 0f
        );
        UniTask CrossFade(
            AudioClip audioClip, float fadeOutTime, float fadeInDelay, float fadeInTime,
            float volumeFrom = 0f, float volumeTo = 1f, bool? loop = null,
            bool autoVolume = true, float pitch = 1.0f, float pan = 0f,
            float spatial = 0f, Vector3? position = null
        );
        UniTask CrossFade(
            string audioPath, float fadeOutTime, float fadeInDelay, float fadeInTime,
            float volumeFrom = 0f, float volumeTo = 1f, bool? loop = null,
            bool autoVolume = true, float pitch = 1.0f, float pan = 0f,
            float spatial = 0f, Vector3? position = null
        );
        UniTask Ducking(
            float duckTime, float volumeScale = 0.25f,
            float fadeOutTime = 0.2f, float fadeInTime = 0.4f
        );
        UniTask FadeVolume(
            float _duration, float volumeFrom, float volumeTo, AltoEasingFunc ease = null
        );

        void Pause(AudioClip audioClip = null);
        void Pause(string audioPath);

        void Resume(AudioClip audioClip = null);
        void Resume(string audioPath);
    }
}
