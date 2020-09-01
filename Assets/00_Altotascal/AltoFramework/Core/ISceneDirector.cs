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

        void Init(GameObject gameObject, IBootConfig bootConfig);

        UniTask GoToNextScene(ISceneContext nextSceneContext, float fadeOutTime = 0.3f, float fadeInTime = 0.3f);

        UniTask GoToNextScene(string nextSceneName, float fadeOutTime = 0.3f, float fadeInTime = 0.3f);
    }
}
