using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AltoFramework
{
    public interface IResourceStore
    {
        void UnloadAll();

        UniTask LoadSpriteAtlas(string assetAddress);
        void UnloadSpriteAtlas(string assetAddress);
        Sprite GetSprite(string spriteName);

        UniTask LoadObject(string assetAddress);
        void UnloadObject(string assetAddress);
        T GetObject<T>(string assetAddress) where T : ScriptableObject;
    }
}
