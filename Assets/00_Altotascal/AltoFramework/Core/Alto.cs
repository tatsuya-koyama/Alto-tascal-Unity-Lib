using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace AltoFramework
{
    /// <summary>
    ///   Global access point for global-scope and scene-scope objects.
    /// </summary>
    public class Alto
    {
        private static IGlobalContext _context = null;

        public static IGlobalContext context
        {
            get
            {
                if (_context == null)
                {
                    AltoLog.FW("Initialize Alto Context automatically...");
                    _context = new Production.GlobalContext();
                    _context.Init();
                }
                return _context;
            }
            set { _context = value; }
        }

        public static bool hasGlobalContext => (_context != null);

        //----------------------------------------------------------------------
        // Shortcut methods / properties
        //----------------------------------------------------------------------

        public static ISceneDirector scene
        {
            get { return context.sceneDirector; }
        }

        public static ISceneContext sceneContext
        {
            get { return context.sceneDirector?.currentSceneContext; }
        }

        public static bool hasSceneContext => (sceneContext != null);

        /// <summary>
        ///   Returns wrapped frame delta time.
        /// </summary>
        public static float dt
        {
            get { return context.timeKeeper.dt; }
        }

        public static ITimeKeeper time
        {
            get { return context.timeKeeper; }
        }

        public static IResourceStore resources
        {
            get { return context.resourceHub.sceneScopeResourceStore; }
        }

        public static IResourceStore globalResources
        {
            get { return context.resourceHub.globalScopeResourceStore; }
        }

        public static IAudioPlayer bgm
        {
            get { return context.bgmPlayer; }
        }

        public static IAudioPlayer se
        {
            get { return context.sePlayer; }
        }

        /// <summary>
        ///   Get scene-scoped Signal instance.
        ///   (Scene-scoped Signal listeners are cleared when loading new scene.)
        /// </summary>
        /// <example><code>
        ///   // Define custom signal classes
        ///   public class MySignal : AltoSignal {}
        ///   public class MySignalWithParam : AltoSignal<int> {}
        ///
        ///   Alto.Signal<MySignal>().Connect(handler);      // subscribe
        ///   Alto.Signal<MySignal>().ConnectOnce(handler);  // subscribe one-shoft event
        ///   Alto.Signal<MySignal>().Disconnect(handler);   // unsubscribe
        ///   Alto.Signal<MySignal>().Emit();                // publish (connected handlers are invoked)
        ///   Alto.Signal<MySignalWithParam>().Emit(123);    // publish with argument
        /// </code></example>
        public static T Signal<T>() where T : IAltoSignal, new()
        {
            return context.signalHub.sceneScopeSignalRegistry.GetOrCreate<T>();
        }

        public static T GlobalSignal<T>() where T : IAltoSignal, new()
        {
            return context.signalHub.globalScopeSignalRegistry.GetOrCreate<T>();
        }

        /// <summary>
        ///   Get new tween object to register tween task with method chain.
        /// </summary>
        /// <example><code>
        ///   // 0 -> 1 まで、0.18 秒かけて uGUI の alpha を変化
        ///   Alto.Tween().FromTo(0f, 1.0f, 0.18f, AltoEase.Liner).SetAlpha(graphic);
        ///
        ///   // OnUpdate でイージング中の値を受け取って好きな処理に使える
        ///   Alto.Tween().FromTo(5.0f, 0f, 0.3f).OnUpdate(x => {
        ///       someImage.material.SetFloat("shaderProp", x);
        ///   });
        ///
        ///   // await したい時は Async() をつける
        ///   // ※ シーン遷移時には自動でキャンセルされる。キャンセルしたくなければ Async(false)
        ///   await Alto.Tween().FromTo(0f, 1.0f, 0.3f).SetAlpha(graphic).Async();
        /// </code></example>
        public static IAltoTween Tween(object obj = null)
        {
            return context.tweenerHub.sceneScopeTweener.NewTween(obj);
        }

        public static IAltoTweener tweener
        {
            get { return context.tweenerHub.sceneScopeTweener; }
        }

        /// <summary>
        ///   Create object pool for MonoBehaviour.
        /// </summary>
        /// <example><code>
        ///   // 使う前にはオリジナルの GameObject で初期化が必要。初期プールサイズを指定可能。
        ///   // プール対象は Alto.Util.PoolableBehaviour を継承している必要がある
        ///   Alto.CreateObjectPool<YourComponent>(originalGameObj, 64);
        ///
        ///   // プールからオブジェクトを取得
        ///   Alto.ObjectPool<YourComponent>().Get();
        ///
        ///   // プールに返却する時はオブジェクト側で以下を呼ぶ：
        ///   ReturnToPool();
        /// </code></example>
        public static AltoObjectPool<T> CreateObjectPool<T>(
            GameObject original, int reserveNum
        ) where T : PoolableBehaviour
        {
            return context.objectPoolHub.sceneScopeObjectPoolRegistry.CreatePool<T>(original, reserveNum);
        }

        public static AltoObjectPool<T> ObjectPool<T>() where T : PoolableBehaviour
        {
            return context.objectPoolHub.sceneScopeObjectPoolRegistry.GetPool<T>();
        }

        //----------------------------------------------------------------------
        // async / await Helper
        //----------------------------------------------------------------------

        /// <summary>
        ///   UniTask を、シーン遷移時に自動でキャンセルされる UniTask に変換する。
        ///   GameObject に影響する処理を await する時は基本 Alto.Async() で囲っておくことで
        ///   シーン遷移後に前のシーンの非同期処理が走り続ける…といった事故を防ぐことができる。
        /// </summary>
        /// <example><code>
        ///   await Alto.Async(SomeAsyncFund());
        /// </code><example>
        public static UniTask Async(UniTask task)
        {
            var ct = sceneContext.CancelTokenSource.Token;
            return task.WithCancellation(ct);
        }

        public static UniTask Wait(float seconds, Action action = null)
        {
            return Async(time.Wait(seconds, action));
        }
    }
}
