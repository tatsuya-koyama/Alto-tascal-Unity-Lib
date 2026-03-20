using UnityEngine;

namespace AltoFramework.Production
{
    public class ObjectPoolHub : IObjectPoolHub
    {
        public AltoObjectPoolRegistry sceneScopeObjectPoolRegistry { get; private set; }

        public ObjectPoolHub(ISceneDirector sceneDirector, Transform parentTransform)
        {
            sceneScopeObjectPoolRegistry = new AltoObjectPoolRegistry(parentTransform);

            sceneDirector.sceneLoading += OnSceneLoading;
        }

        void OnSceneLoading()
        {
            sceneScopeObjectPoolRegistry.Clear();
        }
    }
}
