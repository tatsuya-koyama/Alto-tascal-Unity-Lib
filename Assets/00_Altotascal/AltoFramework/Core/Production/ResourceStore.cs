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
        private Dictionary<string, SpriteAtlas>      _atlases = new Dictionary<string, SpriteAtlas>();
        private Dictionary<string, Sprite>           _sprites = new Dictionary<string, Sprite>();
        private Dictionary<string, ScriptableObject> _objects = new Dictionary<string, ScriptableObject>();
        private Dictionary<string, AudioClip>        _audios  = new Dictionary<string, AudioClip>();

        //----------------------------------------------------------------------
        // For Debug
        //----------------------------------------------------------------------
        #if UNITY_EDITOR
        public Dictionary<string, SpriteAtlas>      atlases { get { return _atlases; } }
        public Dictionary<string, Sprite>           sprites { get { return _sprites; } }
        public Dictionary<string, ScriptableObject> objects { get { return _objects; } }
        public Dictionary<string, AudioClip>        audios  { get { return _audios;  } }
        #endif

        //----------------------------------------------------------------------
        // General
        //----------------------------------------------------------------------

        public void UnloadAll()
        {
            _atlases.Keys.ToList().ForEach(key => UnloadSpriteAtlas(key));
            _objects.Keys.ToList().ForEach(key => UnloadObject(key));
            _audios .Keys.ToList().ForEach(key => UnloadAudio(key));
        }

        //----------------------------------------------------------------------
        // Sprite Atlas
        //----------------------------------------------------------------------

        /// <summary>
        ///   Addressable Assets のスプライトアトラスをメモリにロードし、
        ///   GetSprite(スプライト名) で取得可能にする。
        ///   現実装ではアトラス内のスプライト名がユニークである前提
        ///   （複数のアトラスに同名のスプライトがあった場合はロード時に Dictionary のエラーが発生する）
        /// </summary>
        async UniTask LoadSpriteAtlasSingle(string assetAddress)
        {
            if (_atlases.ContainsKey(assetAddress))
            {
                AltoLog.FW_Warn($"[ResourceStore] SpriteAtlas <{assetAddress}> is already loaded.");
                return;
            }

            var asyncOpHandle = Addressables.LoadAssetAsync<SpriteAtlas>(assetAddress);
            var spriteAtlas = await asyncOpHandle.Task;
            if (asyncOpHandle.Status != AsyncOperationStatus.Succeeded)
            {
                AltoLog.Error($"[ResourceStore] SpriteAtlas Load Error : <b>{assetAddress}</b>");
                return;
            }
            _atlases.Add(assetAddress, spriteAtlas);
            RegisterSprites(spriteAtlas);
        }

        public async UniTask LoadSpriteAtlas(params string[] assetAddressList)
        {
            var tasks = assetAddressList.Select(address => LoadSpriteAtlasSingle(address));
            await UniTask.WhenAll(tasks);
        }

        public void UnloadSpriteAtlas(string assetAddress)
        {
            if (!_atlases.ContainsKey(assetAddress))
            {
                AltoLog.FW_Warn($"[ResourceStore] SpriteAtlas <{assetAddress}> is not loaded.");
                return;
            }

            var spriteAtlas = _atlases[assetAddress];
            _atlases.Remove(assetAddress);
            UnregisterSprites(spriteAtlas);
            Addressables.Release<SpriteAtlas>(spriteAtlas);
        }

        public Sprite GetSprite(string spriteName)
        {
            Sprite sprite;
            if (!_sprites.TryGetValue(spriteName, out sprite))
            {
                AltoLog.Error($"[ResourceStore] Sprite <{spriteName}> not found.");
                return null;
            }

            return sprite;
        }

        // スプライトアトラス内の Sprite を走査
        void ForEachSprite(SpriteAtlas atlas, Action<string, Sprite> action)
        {
            Sprite[] spritesInAtlas = new Sprite[atlas.spriteCount];
            atlas.GetSprites(spritesInAtlas);

            foreach (var sprite in spritesInAtlas)
            {
                // SpriteAtlas.GetSprites で取得した Sprite の名前には "(Clone)" が付くため、それを削る
                string spriteName = sprite.name.Substring(0, sprite.name.Length - "(Clone)".Length);
                action(spriteName, sprite);
            }
        }

        void RegisterSprites(SpriteAtlas atlas)
        {
            ForEachSprite(atlas, (spriteName, sprite) => {
                _sprites.Add(spriteName, sprite);
            });
        }

        void UnregisterSprites(SpriteAtlas atlas)
        {
            ForEachSprite(atlas, (spriteName, sprite) => {
                _sprites.Remove(spriteName);
            });
        }

        //----------------------------------------------------------------------
        // ScriptableObject
        //----------------------------------------------------------------------

        async UniTask LoadObjectSingle(string assetAddress)
        {
            if (_objects.ContainsKey(assetAddress))
            {
                AltoLog.FW_Warn($"[ResourceStore] ScriptableObject <{assetAddress}> is already loaded.");
                return;
            }

            var asyncOpHandle = Addressables.LoadAssetAsync<ScriptableObject>(assetAddress);
            var obj = await asyncOpHandle.Task;
            if (asyncOpHandle.Status != AsyncOperationStatus.Succeeded)
            {
                AltoLog.Error($"[ResourceStore] ScriptableObject Load Error : <b>{assetAddress}</b>");
                return;
            }
            _objects.Add(assetAddress, obj);
        }

        public async UniTask LoadObject(params string[] assetAddressList)
        {
            var tasks = assetAddressList.Select(address => LoadObjectSingle(address));
            await UniTask.WhenAll(tasks);
        }

        public void UnloadObject(string assetAddress)
        {
            if (!_objects.ContainsKey(assetAddress))
            {
                AltoLog.FW_Warn($"[ResourceStore] ScriptableObject <{assetAddress}> is not loaded.");
                return;
            }

            var obj = _objects[assetAddress];
            _objects.Remove(assetAddress);
            Addressables.Release<ScriptableObject>(obj);
        }

        public T GetObject<T>(string assetAddress) where T : ScriptableObject
        {
            ScriptableObject obj;
            if (!_objects.TryGetValue(assetAddress, out obj))
            {
                AltoLog.Error($"[ResourceStore] ScriptableObject <{assetAddress}> not found.");
                return null;
            }
            if (!(obj is T))
            {
                AltoLog.Error($"[ResourceStore] ScriptableObject cast error : <{assetAddress}>");
                return null;
            }
            return (T)obj;
        }

        //----------------------------------------------------------------------
        // AudioClip
        //----------------------------------------------------------------------

        async UniTask LoadAudioSingle(string assetAddress)
        {
            if (_audios.ContainsKey(assetAddress))
            {
                AltoLog.FW_Warn($"[ResourceStore] AudioClip <{assetAddress}> is already loaded.");
                return;
            }

            var asyncOpHandle = Addressables.LoadAssetAsync<AudioClip>(assetAddress);
            var audioClip = await asyncOpHandle.Task;
            if (asyncOpHandle.Status != AsyncOperationStatus.Succeeded)
            {
                AltoLog.Error($"[ResourceStore] AudioClip Load Error : <b>{assetAddress}</b>");
                return;
            }
            _audios.Add(assetAddress, audioClip);
        }

        public async UniTask LoadAudio(params string[] assetAddressList)
        {
            var tasks = assetAddressList.Select(address => LoadAudioSingle(address));
            await UniTask.WhenAll(tasks);
        }

        public void UnloadAudio(string assetAddress)
        {
            if (!_audios.ContainsKey(assetAddress))
            {
                AltoLog.FW_Warn($"[ResourceStore] AudioClip <{assetAddress}> is not loaded.");
                return;
            }

            var audioClip = _audios[assetAddress];
            _audios.Remove(assetAddress);
            Addressables.Release<AudioClip>(audioClip);
        }

        public AudioClip GetAudio(string assetAddress)
        {
            AudioClip audioClip;
            if (!_audios.TryGetValue(assetAddress, out audioClip))
            {
                AltoLog.Error($"[ResourceStore] AudioClip <{assetAddress}> not found.");
                return null;
            }
            return audioClip;
        }
    }
}
