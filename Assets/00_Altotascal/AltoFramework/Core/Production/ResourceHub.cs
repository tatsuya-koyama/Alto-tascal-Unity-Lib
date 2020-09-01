namespace AltoFramework.Production
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
            AltoLog.FW("[ResourceHub] Unload all scene-scoped resources.");
            sceneScopeResourceStore.UnloadAll();
        }
    }
}
