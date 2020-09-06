using Cysharp.Threading.Tasks;
using System.Threading;

namespace AltoFramework
{
    /// <summary>
    ///   Scene Transition flow and callback timings :
    ///     [Ex. Transition from Scene_1 to Scene_2]
    ///       - Fade out screen
    ///       - (Invoke SceneDirector.sceneLoading event)
    ///       - Destroy all Scene_1 objects
    ///       -  * Scene_1 context :: Finalize()
    ///       -  * Scene_2 context :: InitBeforeLoadScene()
    ///       - Start Load Scene_2
    ///       - (MonoBehaviors' Awake)
    ///       -  * Scene_2 context :: InitAfterLoadScene()
    ///       - (Invoke SceneDirector.sceneLoaded event)
    ///       - Fade in screen
    ///       -  * Scene_2 context :: OnStartupScene()
    /// </summary>
    public interface ISceneContext
    {
        bool IsReady { get; set; }
        CancellationTokenSource CancelTokenSource { get; }

        string SceneName();
        UniTask InitBeforeLoadScene();
        UniTask InitAfterLoadScene();
        void OnStartupScene();
        UniTask Finalize();

        void Update();
    }

    public abstract class DefaultSceneContext : ISceneContext
    {
        public bool IsReady { get; set; }
        public CancellationTokenSource CancelTokenSource { get; private set; } = new CancellationTokenSource();

        public abstract string SceneName();

        public virtual UniTask InitBeforeLoadScene()
        {
            return UniTask.CompletedTask;
        }

        public virtual UniTask InitAfterLoadScene()
        {
            return UniTask.CompletedTask;
        }

        public virtual void OnStartupScene()
        {
        }

        public virtual void Update()
        {
        }

        public virtual UniTask Finalize()
        {
            return UniTask.CompletedTask;
        }
    }
}
