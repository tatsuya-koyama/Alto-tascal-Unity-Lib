namespace AltoFramework
{
    public interface IGlobalContext
    {
        ISceneDirector sceneDirector { get; }

        ITimeKeeper timeKeeper { get; }

        IResourceStore resourceStore { get; }

        IAudioPlayer bgmPlayer { get; }

        IAudioPlayer sePlayer { get; }

        ISignalHub signalHub { get; }

        ITweenerHub tweenerHub { get; }

        IObjectPoolHub objectPoolHub { get; }

        void Init(IBootConfig bootConfig = null);
    }
}
