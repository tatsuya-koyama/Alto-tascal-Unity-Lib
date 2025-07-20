using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AltoFramework.Production
{
    public class SceneDirector : MonoBehaviour, ISceneDirector
    {
        public ISceneContext currentSceneContext { get; private set; }
        public bool isInTransition { get; private set; } = false;

        public event Action sceneLoading;
        public event Action sceneLoaded;
        public event Action sceneUpdate;

        bool _useGlobalAudioListener;
        IResourceStore _resourceStore;
        ScreenFader _screenFader;

        //----------------------------------------------------------------------
        // ISceneDirector
        //----------------------------------------------------------------------

        public void Init(GameObject gameObject, IBootConfig bootConfig, IResourceStore resourceStore)
        {
            _useGlobalAudioListener = bootConfig.useGlobalAudioListener;
            _resourceStore = resourceStore;

            _screenFader = gameObject.AddComponent<ScreenFader>();
            _screenFader.Init();
        }

        void Update()
        {
            if (currentSceneContext != null && !currentSceneContext.IsReady) { return; }

            currentSceneContext?.Update();
            sceneUpdate?.Invoke();
        }

        public async UniTask GoToNextScene(ISceneContext nextSceneContext, float fadeOutTime = 0.3f, float fadeInTime = 0.3f)
        {
            Alto.Log.FW($"[SceneDirector] Load scene with scene context : <b>{nextSceneContext}</b>");
            await LoadSceneWithFade(nextSceneContext, nextSceneContext.SceneName(), false, fadeOutTime, fadeInTime);
        }

        public async UniTask GoToNextScene(string nextSceneName, float fadeOutTime = 0.3f, float fadeInTime = 0.3f)
        {
            Alto.Log.FW("[SceneDirector] * No scene context is given.");
            await LoadSceneWithFade(null, nextSceneName, false, fadeOutTime, fadeInTime);
        }

        public async UniTask GoToNextSceneWithCustomTransition(ISceneContext nextSceneContext)
        {
            Alto.Log.FW($"[SceneDirector] Load scene with scene context : <b>{nextSceneContext}</b>");
            await LoadSceneWithFade(nextSceneContext, nextSceneContext.SceneName(), true);
        }

        public async UniTask GoToNextSceneWithCustomTransition(string nextSceneName)
        {
            Alto.Log.FW("[SceneDirector] * No scene context is given.");
            await LoadSceneWithFade(null, nextSceneName, true);
        }

        public void SetFadeColor(Color color)
        {
            _screenFader.SetColor(color);
        }

        //----------------------------------------------------------------------
        // private
        //----------------------------------------------------------------------

        async UniTask LoadSceneWithFade(
            ISceneContext nextSceneContext,
            string nextSceneName,
            bool useCustomTransition,
            float fadeOutTime = 0.3f,
            float fadeInTime = 0.3f
        )
        {
            if (isInTransition)
            {
                Alto.Log.FW_Warn($"[SceneDirector] Now in transition - {nextSceneName} is dismissed.");
                return;
            }
            isInTransition = true;

            //----- 暗転と後片付け
            if (useCustomTransition && currentSceneContext != null) {
                await currentSceneContext.CustomFadeOut();
            } else {
                await _screenFader.FadeOut(fadeOutTime);
            }
            sceneLoading?.Invoke();

            DestroyAllObjectsInScene();
            if (currentSceneContext != null)
            {
                await currentSceneContext.Finalize();
                currentSceneContext.CancelTokenSource.Cancel();
            }
            SetIsSceneReady(false);
            currentSceneContext = nextSceneContext;

            // 次のシーンに必要なリソースをロード、不要なものはアンロード
            await LoadAndUnloadResources(nextSceneContext);

            //----- 次のシーンの読み込み
            Alto.Log.FW("[SceneDirector] - Init <b>Before</b> Load Scene");
            if (nextSceneContext != null)
            {
                await nextSceneContext.InitBeforeLoadScene();
            }

            await SceneManager.LoadSceneAsync(nextSceneName);
            DisableLocalAudioListener();

            Alto.Log.FW("[SceneDirector] - Init <b>After</b> Load Scene");
            if (currentSceneContext != null)
            {
                await currentSceneContext.InitAfterLoadScene();
            }
            SetIsSceneReady(true);

            sceneLoaded?.Invoke();

            // シーンの 1 フレーム目は重くなるので 1 フレーム待ってからフェードイン
            await UniTask.DelayFrame(1);
            if (useCustomTransition && currentSceneContext != null) {
                await currentSceneContext.CustomFadeIn();
            } else {
                await _screenFader.FadeIn(fadeInTime);
            }

            isInTransition = false;
            currentSceneContext?.OnStartupScene();
        }

        void DestroyAllObjectsInScene()
        {
            List<GameObject> rootObjects = new List<GameObject>();
            Scene scene = SceneManager.GetActiveScene();
            scene.GetRootGameObjects(rootObjects);

            foreach (var obj in rootObjects)
            {
                Destroy(obj);
            }
        }

        void SetIsSceneReady(bool isReady)
        {
            if (currentSceneContext != null)
            {
                currentSceneContext.IsReady = isReady;
            }
        }

        async UniTask LoadAndUnloadResources(ISceneContext nextSceneContext)
        {
            _resourceStore.ReleaseAllSceneScoped();
            nextSceneContext.RetainResource();
            _resourceStore.Unload();
            await _resourceStore.Load();
        }

        /// <summary>
        /// シーンをまたぐ AudioListener を使うモードの場合は
        /// 複数 Listener によるエラーが出ないように、
        /// メインカメラに AudioListener がついていたらオフにする
        /// </summary>
        void DisableLocalAudioListener()
        {
            if (!_useGlobalAudioListener) { return; }
            if (Camera.main == null) { return; }

            var audioListener = Camera.main.GetComponent<AudioListener>();
            if (audioListener != null)
            {
                audioListener.enabled = false;
            }
        }
    }
}
