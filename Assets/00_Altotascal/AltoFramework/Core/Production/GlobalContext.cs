using UnityEngine;

namespace AltoFramework.Production
{
    public class GlobalContext : IGlobalContext
    {
        public ISceneDirector sceneDirector { get; private set; }

        public ITimeKeeper timeKeeper { get; private set; }

        public IResourceHub resourceHub { get; private set; }

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
            GameObject.DontDestroyOnLoad(_gameObject);

            sceneDirector = _gameObject.AddComponent<SceneDirector>();
            sceneDirector.Init(_gameObject, bootConfig);

            timeKeeper = new TimeKeeper();

            resourceHub = new ResourceHub(sceneDirector);

            signalHub = new SignalHub(sceneDirector);

            tweenerHub = new TweenerHub(sceneDirector, timeKeeper);

            objectPoolHub = new ObjectPoolHub(sceneDirector);

            bootConfig.OnGameBoot();
        }
    }
}
