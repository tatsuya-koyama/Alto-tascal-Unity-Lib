using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace AltoFramework
{
    /// <summary>
    /// オーディオ再生を制御。
    ///
    /// 【Play 時の引数】
    ///  - loop : true ならループ再生。 BGM はデフォルト true, SE はデフォルト false
    ///  - mix  : true なら多重再生。 BGM はデフォルト false, SE はデフォルト true
    ///  - autoVolume : true なら同じ音を同時再生した時のボリュームの高まりを抑える
    ///                 mix = false の場合は、true を指定していても false として扱われる
    ///  - replay : true ならすでに指定の音が鳴っていた場合でも再生を行う。
    ///             BGM はデフォルト false, SE はデフォルト true
    ///
    /// 【ボリューム】
    ///  - ボリュームは個別の volume 指定に systemMasterVolume と userMasterVolume を
    ///    乗じて [0, 1] に Clamp したものが適用される。MasterVolume を 1 未満にしていた
    ///    場合は volume に 1 以上の値を指定すると他の音よりも相対的に音量が大きくなる
    ///
    /// 【フェードイン / フェードアウト】
    ///  - FadeIn の await はフェードイン完了時に返ってくる。
    ///    すでに再生中の音で replay = false 指定時は再生がキャンセルされるため、
    ///    await は即時に返る
    ///  - FadeOut も同様にフェードアウト完了時に await が返る。
    ///    すでに音が停止中の場合はキャンセルされ、await が即時に返る
    ///  * 現状 FadeOut 中の FadeOut は考慮されていない。
    ///    （やるとフェードアウトがされ直し、1 回目のタイミングで音が停止するため
    ///      変な聴こえ方になる）
    /// </summary>
    public class AudioPlayer : IAudioPlayer
    {
        AudioSourcePool _sourcePool = new AudioSourcePool();
        IResourceStore _resources;

        protected virtual string AudioSourceObjectName => "AudioSource";
        protected virtual bool DefaultLoop   => false;
        protected virtual bool DefaultMix    => false;
        protected virtual bool DefaultReplay => false;

        // 実装者が決めるシステム全体のマスターボリューム
        float _systemMasterVolume = 1.0f;
        public float systemMasterVolume
        {
            get { return _systemMasterVolume; }
            set
            {
                _systemMasterVolume = Mathf.Clamp01(value);
                UpdatePlayingVolume();
            }
        }

        // ゲームのユーザが調整するボリューム設定
        float _userMasterVolume = 1.0f;
        public float userMasterVolume
        {
            get { return _userMasterVolume; }
            set
            {
                _userMasterVolume = Mathf.Clamp01(value);
                UpdatePlayingVolume();
            }
        }

        // 一時的に音量を絞る処理に使う内部的なボリューム
        float _duckingVolume = 1.0f;
        float duckingVolume
        {
            get { return _duckingVolume; }
            set
            {
                _duckingVolume = Mathf.Clamp01(value);
                UpdatePlayingVolume();
            }
        }

        AltoTweener _tween = new AltoTweener();

        //----------------------------------------------------------------------
        // Init
        //----------------------------------------------------------------------

        public void Init(
            GameObject gameObject, int numSourcePool,
            ISceneDirector sceneDirector, IResourceHub resourceHub
        )
        {
            _sourcePool.Init(gameObject, AudioSourceObjectName, numSourcePool);
            _resources = resourceHub.sceneScopeResourceStore;

            sceneDirector.sceneUpdate += OnSceneUpdate;
        }

        //----------------------------------------------------------------------
        // Play / Fade In
        //----------------------------------------------------------------------

        public void Play(
            AudioClip audioClip, bool? loop = null, bool? mix = null,
            bool? replay = null, bool autoVolume = true,
            float volume = 1.0f, float pitch = 1.0f, float pan = 0f,
            float spatial = 0f, Vector3? position = null
        )
        {
            PlayGeneral(
                audioClip, loop, mix, replay,
                autoVolume, 0f, volume, 0f, pitch, pan, spatial, position
            );
        }

        public void Play(
            string audioPath, bool? loop = null, bool? mix = null,
            bool? replay = null, bool autoVolume = true,
            float volume = 1.0f, float pitch = 1.0f, float pan = 0f,
            float spatial = 0f, Vector3? position = null
        )
        {
            var audioClip = _resources.GetAudio(audioPath);
            Play(audioClip, loop, mix, replay, autoVolume, volume, pitch, pan, spatial, position);
        }

        public async UniTask FadeIn(
            AudioClip audioClip, float fadeTime, float volumeFrom = 0f, float volumeTo = 1f,
            bool? loop = null, bool? mix = null, bool? replay = null,
            bool autoVolume = true, float pitch = 1.0f, float pan = 0f,
            float spatial = 0f, Vector3? position = null
        )
        {
            bool isPlayed = PlayGeneral(
                audioClip, loop, mix, replay,
                autoVolume, volumeFrom, volumeTo, fadeTime, pitch, pan, spatial, position
            );
            if (!isPlayed) { return; }

            await UniTask.Delay(TimeSpan.FromSeconds(fadeTime));
        }

        public async UniTask FadeIn(
            string audioPath, float fadeTime, float volumeFrom = 0f, float volumeTo = 1f,
            bool? loop = null, bool? mix = null, bool? replay = null,
            bool autoVolume = true, float pitch = 1.0f, float pan = 0f,
            float spatial = 0f, Vector3? position = null
        )
        {
            var audioClip = _resources.GetAudio(audioPath);
            await FadeIn(
                audioClip, fadeTime, volumeFrom, volumeTo,
                loop, mix, replay, autoVolume, pitch, pan, spatial, position
            );
        }

        //----------------------------------------------------------------------
        // Stop / Fade Out
        //----------------------------------------------------------------------

        public void Stop(AudioClip audioClip = null)
        {
            if (audioClip == null)
            {
                _sourcePool.StopAll();
            }
            else
            {
                _sourcePool.Stop(audioClip);
            }
        }

        public void Stop(string audioPath)
        {
            var audioClip = _resources.GetAudio(audioPath);
            Stop(audioClip);
        }

        public async UniTask FadeOut(
            float fadeTime, AudioClip audioClip = null, float volumeFrom = 1f, float volumeTo = 0f
        )
        {
            float finalVolumeFrom = GetVolume(volumeFrom);
            float finalVolumeTo   = GetVolume(volumeTo);

            var states = (audioClip != null)
                ? _sourcePool.GetByAudioClip(audioClip)
                : _sourcePool.GetPlaying();

            if (states.Count == 0) { return; }

            foreach (var state in states)
            {
                state.SetFade(finalVolumeFrom, finalVolumeTo, fadeTime);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(fadeTime));
            foreach (var state in states)
            {
                state.source.Stop();
            }
        }

        public async UniTask FadeOut(
            float fadeTime, string audioPath, float volumeFrom = 1f, float volumeTo = 0f
        )
        {
            var audioClip = _resources.GetAudio(audioPath);
            await FadeOut(fadeTime, audioClip, volumeFrom, volumeTo);
        }

        //----------------------------------------------------------------------
        // Cross Fade
        //----------------------------------------------------------------------

        public async UniTask CrossFade(
            AudioClip audioClip, float fadeOutTime, float fadeInDelay, float fadeInTime,
            float volumeFrom = 0f, float volumeTo = 1f, bool? loop = null,
            bool autoVolume = true, float pitch = 1.0f, float pan = 0f,
            float spatial = 0f, Vector3? position = null
        )
        {
            var playingStates = _sourcePool.GetPlaying();
            foreach (var state in playingStates)
            {
                FadeOut(fadeOutTime, state.source.clip, state.source.volume, 0f).Forget();
            }
            await UniTask.Delay(TimeSpan.FromSeconds(fadeInDelay));
            await FadeIn(
                audioClip, fadeInTime, volumeFrom, volumeTo,
                loop, mix: true, replay: true, autoVolume, pitch, pan, spatial, position
            );
        }

        public async UniTask CrossFade(
            string audioPath, float fadeOutTime, float fadeInDelay, float fadeInTime,
            float volumeFrom = 0f, float volumeTo = 1f, bool? loop = null,
            bool autoVolume = true, float pitch = 1.0f, float pan = 0f,
            float spatial = 0f, Vector3? position = null
        )
        {
            var audioClip = _resources.GetAudio(audioPath);
            await CrossFade(
                audioClip, fadeOutTime, fadeInDelay, fadeInTime, volumeFrom, volumeTo,
                loop, autoVolume, pitch, pan, spatial, position
            );
        }

        //----------------------------------------------------------------------
        // Volume Effect
        //----------------------------------------------------------------------

        /// <summary>
        /// マスターボリュームを一時的に絞る
        /// </summary>
        public async UniTask Ducking(
            float _duckTime, float volumeScale = 0.25f,
            float _fadeOutTime = 0.2f, float _fadeInTime = 0.4f
        )
        {
            float fadeOutTime = Mathf.Max(_fadeOutTime, 0.01f);
            float fadeInTime  = Mathf.Max(_fadeInTime,  0.01f);
            float duckTime = Mathf.Max(_duckTime - fadeOutTime - fadeInTime, 0f);
            _tween.NewTween().FromTo(1f, volumeScale, fadeOutTime, AltoEase.Out2).OnUpdate(t => {
                duckingVolume = t;
            });
            _tween.NewTween().Delay(fadeOutTime + duckTime)
                .FromTo(volumeScale, 1f, fadeInTime, AltoEase.Linear).OnUpdate(t => {
                    duckingVolume = t;
                }
            );
            await UniTask.Delay(TimeSpan.FromSeconds(duckTime + fadeOutTime + fadeInTime));
        }

        //----------------------------------------------------------------------
        // Pause / Resume
        //----------------------------------------------------------------------

        public void Pause(AudioClip audioClip = null)
        {
            if (audioClip == null)
            {
                _sourcePool.PauseAll();
            }
            else
            {
                _sourcePool.Pause(audioClip);
            }
        }

        public void Pause(string audioPath)
        {
            var audioClip = _resources.GetAudio(audioPath);
            Pause(audioClip);
        }

        public void Resume(AudioClip audioClip = null)
        {
            if (audioClip == null)
            {
                _sourcePool.ResumeAll();
            }
            else
            {
                _sourcePool.Resume(audioClip);
            }
        }

        public void Resume(string audioPath)
        {
            var audioClip = _resources.GetAudio(audioPath);
            Resume(audioClip);
        }

        //----------------------------------------------------------------------
        // private
        //----------------------------------------------------------------------

        void OnSceneUpdate()
        {
            _sourcePool.Update(Time.unscaledDeltaTime);
            _tween.Update(Time.unscaledDeltaTime);
        }

        /// <summary>
        /// フェードインに対応した汎用再生処理。
        /// 再生時は true, 再生がキャンセルされた場合は false を返す
        /// </summary>
        bool PlayGeneral(
            AudioClip audioClip, bool? loop, bool? _mix, bool? _replay,
            bool _autoVolume, float volumeFrom, float volumeTo, float fadeTime,
            float pitch, float pan, float spatial, Vector3? _position
        )
        {
            if (audioClip == null) { return false; }

            bool replay = _replay ?? DefaultReplay;
            if (!replay)
            {
                if (_sourcePool.IsPlaying(audioClip)) { return false; }
            }

            bool mix = _mix ?? DefaultMix;
            if (!mix) { _sourcePool.StopAll(); }

            bool autoVolume = !mix ? false : _autoVolume;

            var audioSourceState = _sourcePool.GetNext();
            audioSourceState.originalVolume = volumeTo;
            float finalVolumeFrom = GetVolume(volumeFrom, autoVolume, audioClip);
            float finalVolumeTo   = GetVolume(volumeTo,   autoVolume, audioClip);
            if (fadeTime <= 0f)
            {
                audioSourceState.SetVolumeAtOnce(finalVolumeTo);
            }
            else
            {
                audioSourceState.SetFade(finalVolumeFrom, finalVolumeTo, fadeTime);
            }

            var source = audioSourceState.source;
            source.clip         = audioClip;
            source.loop         = loop ?? DefaultLoop;
            source.pitch        = pitch;
            source.panStereo    = pan;
            source.spatialBlend = spatial;

            Vector3 position = _position ?? Vector3.zero;
            var transform = audioSourceState.gameObject.transform;
            transform.position = position;

            source.Play();
            return true;
        }

        float GetVolume(float volume, bool autoVolume = false, AudioClip audioClip = null)
        {
            // 同じ音が多重再生された時のボリュームの高まりを抑える
            if (autoVolume)
            {
                var states = _sourcePool.GetByAudioClip(audioClip);
                foreach (var state in states)
                {
                    if      (state.source.time <= 1 / 60f) { volume *= 0f; }
                    else if (state.source.time <= 1 / 30f) { volume *= 0.8f; }
                    else if (state.source.time <= 1 / 20f) { volume *= 0.9f; }
                }
            }

            return Mathf.Clamp01(volume * systemMasterVolume * userMasterVolume * duckingVolume);
        }

        /// <summary>
        /// マスターボリュームが変更された際に再生中の音のボリュームを更新する
        /// </summary>
        void UpdatePlayingVolume()
        {
            var states = _sourcePool.GetPlaying();
            foreach (var state in states)
            {
                float volume = GetVolume(state.originalVolume);
                state.SetTargetVolume(volume);
            }
        }
    }

    public class BgmPlayer : AudioPlayer
    {
        protected override string AudioSourceObjectName => "BGM Source";
        protected override bool DefaultLoop   => true;
        protected override bool DefaultMix    => false;
        protected override bool DefaultReplay => false;
    }

    public class SePlayer : AudioPlayer
    {
        protected override string AudioSourceObjectName => "SE Source";
        protected override bool DefaultLoop   => false;
        protected override bool DefaultMix    => true;
        protected override bool DefaultReplay => true;
    }
}
