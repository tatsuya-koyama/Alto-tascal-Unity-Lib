using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;

namespace AltoFramework
{
    public interface IResourceStore
    {
        ResourceRegistry registry { get; }

        void Retain(params string[] assetAddressList);
        void Release(params string[] assetAddressList);
        void RetainGlobal(params string[] assetAddressList);
        void ReleaseGlobal(params string[] assetAddressList);
        void ReleaseAllSceneScoped();
        UniTask RetainGlobalWithAutoLoad(params string[] assetAddressList);
        void ReleaseGlobalWithAutoUnload(params string[] assetAddressList);

        GameObject GetGameObj(string assetAddress);
        T GetObj<T>(string assetAddress) where T : ScriptableObject;
        AudioClip GetAudio(string assetAddress);
        SpriteAtlas GetSpriteAtlas(string assetAddress);
        Sprite GetSprite(string spriteName);

        UniTask<AudioClip> GetAudioOndemand(string assetAddress);

        UniTask Load();
        void Unload();
    }
}
