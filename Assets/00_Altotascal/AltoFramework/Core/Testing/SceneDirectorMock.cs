using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AltoFramework.Testing
{
    public class SceneDirectorMock : MonoBehaviour, ISceneDirector
    {
        public ISceneContext currentSceneContext { get; private set; }
        public bool isInTransition { get; private set; } = false;

        public event Action sceneLoading;
        public event Action sceneLoaded;
        public event Action sceneUpdate;

        public void Init(GameObject gameObject, IBootConfig bootConfig, IResourceStore resourceStore)
        {
        }

        void Update()
        {
            if (currentSceneContext == null || !currentSceneContext.IsReady) { return; }

            currentSceneContext?.Update();
            sceneUpdate?.Invoke();
        }

        public async UniTask GoToNextScene(ISceneContext nextSceneContext, float fadeOutTime = 0.3f, float fadeInTime = 0.3f)
        {
            await LoadSceneWithFade(nextSceneContext, nextSceneContext.SceneName(), fadeOutTime, fadeInTime);
        }

        public async UniTask GoToNextScene(string nextSceneName, float fadeOutTime = 0.3f, float fadeInTime = 0.3f)
        {
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
                return;
            }
            isInTransition = true;

            sceneLoading?.Invoke();

            if (currentSceneContext != null)
            {
                await currentSceneContext.Finalize();
            }
            SetIsSceneReady(false);
            currentSceneContext = nextSceneContext;

            if (nextSceneContext != null)
            {
                await nextSceneContext.InitBeforeLoadScene();
            }
            await SceneManager.LoadSceneAsync(nextSceneName);

            if (currentSceneContext != null)
            {
                await currentSceneContext.InitAfterLoadScene();
            }
            SetIsSceneReady(true);

            sceneLoaded?.Invoke();
            isInTransition = false;
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
