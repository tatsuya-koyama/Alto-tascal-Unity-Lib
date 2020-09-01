namespace AltoFramework.Production
{
    public class SignalHub : ISignalHub
    {
        public AltoSignalRegistry globalScopeSignalRegistry { get; private set; }
        public AltoSignalRegistry sceneScopeSignalRegistry  { get; private set; }

        public SignalHub(ISceneDirector sceneDirector)
        {
            globalScopeSignalRegistry = new AltoSignalRegistry();
            sceneScopeSignalRegistry  = new AltoSignalRegistry();

            sceneDirector.sceneLoading += OnSceneLoading;
        }

        void OnSceneLoading()
        {
            sceneScopeSignalRegistry.Clear();
        }
    }
}
