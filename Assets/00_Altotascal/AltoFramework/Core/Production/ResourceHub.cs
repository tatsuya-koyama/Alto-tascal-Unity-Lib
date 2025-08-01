﻿namespace AltoFramework.Production
{
    public class ResourceHub : IResourceHub
    {
        public IResourceStore globalScopeResourceStore { get; private set; }
        public IResourceStore sceneScopeResourceStore  { get; private set; }

        public ResourceHub(ISceneDirector sceneDirector)
        {
            globalScopeResourceStore = new ResourceStore();
            sceneScopeResourceStore  = new ResourceStore();

            sceneDirector.sceneLoading += OnSceneLoading;
        }

        void OnSceneLoading()
        {
            Alto.Log.FW("[ResourceHub] Unload scene-scoped resources no longer needed.");
            sceneScopeResourceStore.Unload();
        }
    }
}
