namespace AltoFramework.Production
{
    public class ObjectPoolHub : IObjectPoolHub
    {
        public AltoObjectPoolRegistry sceneScopeObjectPoolRegistry { get; private set; }

        public ObjectPoolHub(ISceneDirector sceneDirector)
        {
            sceneScopeObjectPoolRegistry = new AltoObjectPoolRegistry();

            sceneDirector.sceneLoading += OnSceneLoading;
        }

        void OnSceneLoading()
        {
            sceneScopeObjectPoolRegistry.Clear();
        }
    }
}
