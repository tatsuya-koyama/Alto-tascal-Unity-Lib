using AltoFramework.Production;
using UnityEngine;

namespace AltoFramework.Testing
{
    public class GlobalContextMock : IGlobalContext
    {
        public ISceneDirector sceneDirector { get; private set; }

        public ITimeKeeper timeKeeper { get; private set; }

        public IResourceHub resourceHub { get; private set; }

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

            sceneDirector = _gameObject.AddComponent<SceneDirectorMock>();
            sceneDirector.Init(_gameObject, bootConfig);

            timeKeeper = new TimeKeeper();

            resourceHub = new ResourceHub(sceneDirector);

            bgmPlayer = new BgmPlayer();
            bgmPlayer.Init(_gameObject, bootConfig.numBgmSourcePool, sceneDirector, resourceHub);

            sePlayer = new SePlayer();
            sePlayer.Init(_gameObject, bootConfig.numSeSourcePool, sceneDirector, resourceHub);

            signalHub = new SignalHub(sceneDirector);

            tweenerHub = new TweenerHub(sceneDirector, timeKeeper);

            objectPoolHub = new ObjectPoolHub(sceneDirector);

            bootConfig.OnGameBoot();
        }
    }
}
