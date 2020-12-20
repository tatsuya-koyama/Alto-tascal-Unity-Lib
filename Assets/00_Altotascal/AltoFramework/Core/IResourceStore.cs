using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AltoFramework
{
    public interface IResourceStore
    {
        void UnloadAll();

        UniTask LoadSpriteAtlas(params string[] assetAddress);
        void UnloadSpriteAtlas(string assetAddress);
        Sprite GetSprite(string spriteName);

        UniTask LoadObject(params string[] assetAddress);
        void UnloadObject(string assetAddress);
        T GetObject<T>(string assetAddress) where T : ScriptableObject;

        UniTask LoadAudio(params string[] assetAddress);
        void UnloadAudio(string assetAddress);
        AudioClip GetAudio(string assetAddress);
    }
}
