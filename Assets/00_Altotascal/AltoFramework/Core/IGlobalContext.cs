namespace AltoFramework
{
    public interface IGlobalContext
    {
        ISceneDirector sceneDirector { get; }

        ITimeKeeper timeKeeper { get; }

        IResourceHub resourceHub { get; }

        ISignalHub signalHub { get; }

        ITweenerHub tweenerHub { get; }

        IObjectPoolHub objectPoolHub { get; }

        void Init(IBootConfig bootConfig = null);
    }
}
