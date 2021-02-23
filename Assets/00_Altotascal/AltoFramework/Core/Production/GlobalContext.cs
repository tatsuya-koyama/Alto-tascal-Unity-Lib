using UnityEngine;

namespace AltoFramework.Production
{
    public class GlobalContext : IGlobalContext
    {
        public ISceneDirector sceneDirector { get; private set; }

        public ITimeKeeper timeKeeper { get; private set; }

        public IResourceStore resourceStore { get; private set; }

        public IAudioPlayer bgmPlayer { get; private set; }

        public IAudioPlayer sePlayer { get; private set; }

        public ISignalHub signalHub { get; private set; }

        public ITweenerHub tweenerHub { get; private set; }

        public IObjectPoolHub objectPoolHub { get; private set; }

        GameObject _contextGameObj;
        GameObject _audioSourceGameObj;

        public void Init(IBootConfig bootConfig = null)
        {
            if (bootConfig == null)
            {
                bootConfig = new DefaultBootConfig();
            }

            _contextGameObj     = new GameObject("AltoGlobalContext");
            _audioSourceGameObj = new GameObject("AltoAudioSource");
            GameObject.DontDestroyOnLoad(_contextGameObj);
            GameObject.DontDestroyOnLoad(_audioSourceGameObj);

            resourceStore = new ResourceStore();

            sceneDirector = _contextGameObj.AddComponent<SceneDirector>();
            sceneDirector.Init(_contextGameObj, bootConfig, resourceStore);
            sceneDirector.sceneUpdate += OnSceneUpdate;

            timeKeeper = new TimeKeeper();

            bgmPlayer = new BgmPlayer();
            bgmPlayer.Init(_audioSourceGameObj, bootConfig.numBgmSourcePool, sceneDirector, resourceStore);

            sePlayer = new SePlayer();
            sePlayer.Init(_audioSourceGameObj, bootConfig.numSeSourcePool, sceneDirector, resourceStore);

            if (bootConfig.useGlobalAudioListener)
            {
                _contextGameObj.AddComponent<AudioListener>();
            }

            signalHub = new SignalHub(sceneDirector);

            tweenerHub = new TweenerHub(sceneDirector, timeKeeper);

            objectPoolHub = new ObjectPoolHub(sceneDirector);

            bootConfig.OnGameBoot();
        }

        void OnSceneUpdate()
        {
            // Global context の GameObject に AudioListener をつけた場合に
            // 3D サウンドも機能させるため、GameObject の位置をカメラと合わせる
            if (Camera.main != null)
            {
                _contextGameObj.transform.position = Camera.main.transform.position;
            }
        }
    }
}
