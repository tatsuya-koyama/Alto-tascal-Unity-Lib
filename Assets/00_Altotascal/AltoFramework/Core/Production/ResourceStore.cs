using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;

namespace AltoFramework.Production
{
    public class ResourceStore : IResourceStore
    {
        ResourceRegistry _registry = new ResourceRegistry();
        public ResourceRegistry registry => _registry;

        GameObjectStore _gameObjs = new GameObjectStore();
        ScriptableObjectStore _scriptableObjs = new ScriptableObjectStore();
        AudioClipStore _audios = new AudioClipStore();
        SpriteAtlasStore _sprites = new SpriteAtlasStore();

        bool _isLoading = false;
        int _loadRetryCount = 0;

        //----------------------------------------------------------------------
        // Manipulate reference count
        //----------------------------------------------------------------------

        public void Retain(params string[] assetAddressList)
        {
            foreach (var address in assetAddressList)
            {
                _registry.Retain(address);
            }
        }

        public void Release(params string[] assetAddressList)
        {
            foreach (var address in assetAddressList)
            {
                _registry.Release(address);
            }
        }

        public void RetainGlobal(params string[] assetAddressList)
        {
            foreach (var address in assetAddressList)
            {
                _registry.Retain(address, isGlobalScope: true);
            }
        }

        public void ReleaseGlobal(params string[] assetAddressList)
        {
            foreach (var address in assetAddressList)
            {
                _registry.Release(address, isGlobalScope: true);
            }
        }

        public async UniTask RetainGlobalWithAutoLoad(params string[] assetAddressList)
        {
            RetainGlobal(assetAddressList);
            await LoadMulti(assetAddressList);
        }

        public void ReleaseGlobalWithAutoUnload(params string[] assetAddressList)
        {
            ReleaseGlobal(assetAddressList);
            UnloadMulti(assetAddressList);
        }

        public void ReleaseAllSceneScoped()
        {
            _registry.ReleaseAll();
        }

        //----------------------------------------------------------------------
        // Retrieve loaded data
        //----------------------------------------------------------------------

        public GameObject GetGameObj(string assetAddress)
        {
            return _gameObjs.Get(assetAddress);
        }

        public T GetObj<T>(string assetAddress) where T : ScriptableObject
        {
            var obj = _scriptableObjs.Get(assetAddress);
            if (!(obj is T))
            {
                AltoLog.FW_Error($"[ResourceStore] ScriptableObject cast error : <{assetAddress}>");
                return null;
            }
            return (T)obj;
        }

        public AudioClip GetAudio(string assetAddress)
        {
            return _audios.Get(assetAddress);
        }

        public SpriteAtlas GetSpriteAtlas(string assetAddress)
        {
            return _sprites.Get(assetAddress);
        }

        /// <summary>
        /// スプライトアトラス内の Sprite を取得
        /// （現実装ではアトラス内のスプライト名がスプライト全体を通してユニークである前提）
        /// </summary>
        public Sprite GetSprite(string spriteName)
        {
            return _sprites.GetSprite(spriteName);
        }

        //----------------------------------------------------------------------
        // Retrieve data with auto loading
        //----------------------------------------------------------------------

        public async UniTask<AudioClip> GetAudioOndemand(string assetAddress)
        {
            if (!_audios.Contains(assetAddress))
            {
                await RetainGlobalWithAutoLoad(assetAddress);
            }
            return _audios.Get(assetAddress);
        }

        //----------------------------------------------------------------------
        // Load (Called by framework)
        //----------------------------------------------------------------------

        /// <summary>
        /// ロードが必要なリソース（参照カウントが 1 以上で未ロードのリソース）をロードする。
        /// ロードは未ロードのものが無くなるまで行われる。
        /// （ロード処理中に新たなリソースが Retain された場合、それもロードしきってから処理が返る）
        /// </summary>
        public async UniTask Load()
        {
            if (_isLoading)
            {
                AltoLog.FW("[ResourceStore] Loading is already running.");
                return;
            }
            _isLoading = true;

            var addresses = _registry.GetAddressesToLoad();
            await LoadMulti(addresses);
            _isLoading = false;

            if (_registry.ShouldLoadAny())
            {
                // ロード中に新たなリソースが Retain されていた場合はそれもロードする
                ++_loadRetryCount;
                if (_loadRetryCount > 99) { return; }
                await Load();
                --_loadRetryCount;
            }
        }

        async UniTask LoadMulti(IEnumerable<string> addresses)
        {
            var tasks = addresses.Select(address => LoadSingle(address));
            await UniTask.WhenAll(tasks);
        }

        /// <summary>
        /// アセット 1 つをメモリにロード。
        /// ※ 参照カウンタが 1 以上になっていなければロードされない
        /// </summary>
        async UniTask LoadSingle(string assetAddress)
        {
            if (!_registry.IsReferenced(assetAddress)) { return; }

            var asyncOpHandle = Addressables.LoadAssetAsync<UnityEngine.Object>(assetAddress);
            var resource = await asyncOpHandle.Task;
            if (asyncOpHandle.Status != AsyncOperationStatus.Succeeded)
            {
                AltoLog.FW_Error($"[ResourceStore] Load Error : <b>{assetAddress}</b>");
                return;
            }
            _registry.MarkLoaded(assetAddress, resource, asyncOpHandle);
            OnLoadResource(assetAddress, resource);
        }

        void OnLoadResource(string assetAddress, UnityEngine.Object resource)
        {
            switch (resource)
            {
                case GameObject gameObj:
                    _gameObjs.OnLoad(assetAddress, gameObj);
                    break;

                case ScriptableObject scriptableObj:
                    _scriptableObjs.OnLoad(assetAddress, scriptableObj);
                    break;

                case AudioClip audioClip:
                    _audios.OnLoad(assetAddress, audioClip);
                    break;

                case SpriteAtlas spriteAtlas:
                    _sprites.OnLoad(assetAddress, spriteAtlas);
                    break;

                default:
                    AltoLog.FW_Warn($"[ResourceStore] Unsupported asset type : {assetAddress} - {resource.GetType()}");
                    break;
            }
        }

        //----------------------------------------------------------------------
        // Unload (Called by framework)
        //----------------------------------------------------------------------

        /// <summary>
        /// アンロードが必要なリソース（ロード済みで参照カウントが 0 未満のリソース）をアンロード
        /// </summary>
        public void Unload()
        {
            var addresses = _registry.GetAddressesToUnload();
            UnloadMulti(addresses);
        }

        void UnloadMulti(IEnumerable<string> addresses)
        {
            foreach (var address in addresses)
            {
                UnloadSingle(address);
            }
        }

        /// <summary>
        /// アセット 1 つをメモリからアンロード。
        /// ※ 参照カウンタが 0 になっていなければアンロードされない
        /// </summary>
        void UnloadSingle(string assetAddress)
        {
            if (_registry.IsReferenced(assetAddress)) { return; }

            var entry = _registry.GetEntry(assetAddress);
            Addressables.Release(entry.handle);
            OnUnloadResource(entry);
            _registry.UnmarkLoaded(assetAddress);
        }

        void OnUnloadResource(ResourceRegistry.ResourceEntry entry)
        {
            if (entry.type == typeof(GameObject))
            {
                _gameObjs.OnUnload(entry.address);
            }
            else if (entry.type.IsSubclassOf(typeof(ScriptableObject)))
            {
                _scriptableObjs.OnUnload(entry.address);
            }
            else if (entry.type == typeof(AudioClip))
            {
                _audios.OnUnload(entry.address);
            }
            else if (entry.type == typeof(SpriteAtlas))
            {
                _sprites.OnUnload(entry.address);
            }
            else
            {
                AltoLog.FW_Warn($"[ResourceStore] Unsupported asset type : {entry.address} - {entry.type}");
            }
        }
    }
}
