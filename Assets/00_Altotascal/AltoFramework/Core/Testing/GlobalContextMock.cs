using AltoFramework.Production;
using UnityEngine;

namespace AltoFramework.Testing
{
    public class GlobalContextMock : IGlobalContext
    {
        public ISceneDirector sceneDirector { get; private set; }

        public ITimeKeeper timeKeeper { get; private set; }

        public IResourceStore resourceStore { get; private set; }

        public IAudioPlayer bgmPlayer { get; private set; }

        public IAudioPlayer sePlayer { get; private set; }

        public ISignalHub signalHub { get; private set; }

        public ITweenerHub tweenerHub { get; private set; }

        public IObjectPoolHub objectPoolHub { get; private set; }

        GameObject _gameObject;

        public void Init(IBootConfig bootConfig = null)
        {
            if (bootConfig == null)
            {
                bootConfig = new DefaultBootConfig();
            }

            _gameObject = new GameObject("GameContext");

            resourceStore = new ResourceStore();

            sceneDirector = _gameObject.AddComponent<SceneDirectorMock>();
            sceneDirector.Init(_gameObject, bootConfig, resourceStore);

            timeKeeper = new TimeKeeper(sceneDirector);

            bgmPlayer = new BgmPlayer();
            bgmPlayer.Init(_gameObject, bootConfig.numBgmSourcePool, sceneDirector, resourceStore);

            sePlayer = new SePlayer();
            sePlayer.Init(_gameObject, bootConfig.numSeSourcePool, sceneDirector, resourceStore);

            signalHub = new SignalHub(sceneDirector);

            tweenerHub = new TweenerHub(sceneDirector, timeKeeper);

            objectPoolHub = new ObjectPoolHub(sceneDirector);

            bootConfig.OnGameBoot();
        }
    }
}
