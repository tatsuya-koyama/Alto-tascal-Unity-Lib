namespace AltoFramework.Production
{
    public class TweenerHub : ITweenerHub
    {
        public AltoTweener sceneScopeTweener { get; private set; }

        ITimeKeeper _timeKeeper;

        public TweenerHub(ISceneDirector sceneDirector, ITimeKeeper timeKeeper)
        {
            _timeKeeper = timeKeeper;

            sceneScopeTweener = new AltoTweener();

            sceneDirector.sceneLoading += OnSceneLoading;
            sceneDirector.sceneUpdate  += OnSceneUpdate;
        }

        void OnSceneLoading()
        {
            sceneScopeTweener.ClearAll();
        }

        void OnSceneUpdate()
        {
            sceneScopeTweener.Update(_timeKeeper.dt);
        }
    }
}
