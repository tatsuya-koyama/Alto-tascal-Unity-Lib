using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AltoFramework
{
    public interface ISceneDirector
    {
        ISceneContext currentSceneContext { get; }
        bool isInTransition { get; }

        event Action sceneLoading;
        event Action sceneLoaded;
        event Action sceneUpdate;

        void Init(GameObject gameObject, IBootConfig bootConfig, IResourceStore resourceStore);

        UniTask GoToNextScene(ISceneContext nextSceneContext, float fadeOutTime = 0.3f, float fadeInTime = 0.3f);
        UniTask GoToNextScene(string nextSceneName, float fadeOutTime = 0.3f, float fadeInTime = 0.3f);

        UniTask GoToNextSceneWithCustomTransition(ISceneContext nextSceneContext);
        UniTask GoToNextSceneWithCustomTransition(string nextSceneName);

        void SetFadeColor(Color color);
    }
}
