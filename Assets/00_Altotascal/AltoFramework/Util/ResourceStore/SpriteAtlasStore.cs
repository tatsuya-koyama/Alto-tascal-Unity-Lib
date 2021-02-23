using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace AltoFramework
{
    /// <summary>
    /// ロードした Addressable Assets の Sprite Atlas を保持。
    /// 各 Sprite を GetSprite("スプライト名") で取得可能にする。
    /// 現実装ではアトラス内のスプライト名がスプライト全体を通してユニークである前提
    /// （複数のアトラスに同名のスプライトがあった場合はロード時に Dictionary のエラーが発生する）
    /// </summary>
    public class SpriteAtlasStore : ResourceSubStore<SpriteAtlas>
    {
        Dictionary<string, Sprite> _sprites = new Dictionary<string, Sprite>();

        public Sprite GetSprite(string spriteName)
        {
            Sprite sprite;
            if (!_sprites.TryGetValue(spriteName, out sprite))
            {
                AltoLog.FW_Error($"[SpriteAtlasStore] Sprite <b>{spriteName}</b> not found.");
                return null;
            }
            return sprite;
        }

        public override void OnLoad(string assetAddress, SpriteAtlas resource)
        {
            base.OnLoad(assetAddress, resource);
            RegisterSprites(resource);
        }

        public override void OnUnload(string assetAddress)
        {
            UnregisterSprites(Get(assetAddress));
            base.OnUnload(assetAddress);
        }

        //----------------------------------------------------------------------
        // private
        //----------------------------------------------------------------------

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

        /// <summary>
        /// スプライトアトラス内の Sprite を走査して処理
        /// </summary>
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
    }
}
