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

        ScreenFader _screenFader;

        public void Init(GameObject gameObject, IBootConfig bootConfig)
        {
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
            AltoLog.FW($"[SceneDirector] Load scene with scene context : <b>{nextSceneContext}</b>");
            await LoadSceneWithFade(nextSceneContext, nextSceneContext.SceneName(), fadeOutTime, fadeInTime);
        }

        public async UniTask GoToNextScene(string nextSceneName, float fadeOutTime = 0.3f, float fadeInTime = 0.3f)
        {
            AltoLog.FW("[SceneDirector] * No scene context is given.");
            await LoadSceneWithFade(null, nextSceneName, fadeOutTime, fadeInTime);
        }

        //----------------------------------------------------------------------
        // private
        //----------------------------------------------------------------------

        async UniTask LoadSceneWithFade(
            ISceneContext nextSceneContext,
            string nextSceneName,
            float fadeOutTime = 0.3f,
            float fadeInTime = 0.3f
        )
        {
            if (isInTransition)
            {
                AltoLog.FW_Warn($"[SceneDirector] Now in transition - {nextSceneName} is dismissed.");
                return;
            }
            isInTransition = true;

            await _screenFader.FadeOut(fadeOutTime);
            sceneLoading?.Invoke();

            DestroyAllObjectsInScene();
            if (currentSceneContext != null)
            {
                await currentSceneContext.Finalize();
            }
            SetIsSceneReady(false);
            currentSceneContext = nextSceneContext;

            AltoLog.FW("[SceneDirector] - Init <b>Before</b> Load Scene");
            if (nextSceneContext != null)
            {
                await nextSceneContext.InitBeforeLoadScene();
            }
            await SceneManager.LoadSceneAsync(nextSceneName);

            AltoLog.FW("[SceneDirector] - Init <b>After</b> Load Scene");
            if (currentSceneContext != null)
            {
                await currentSceneContext.InitAfterLoadScene();
            }
            SetIsSceneReady(true);

            sceneLoaded?.Invoke();
            await _screenFader.FadeIn(fadeInTime);
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
    }
}
