#if UNITY_EDITOR
using AltoFramework.Production;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.U2D;

namespace AltoFramework.Editor
{
    /// <summary>
    ///   ResourceStore からリソース情報を拾ってくる作業ロジック
    /// </summary>
    public class AltoResourceCollector
    {
        public List<AltoResourceTreeViewItem> CollectAll()
        {
            if (!IsReady()) { return null; }

            var items = new List<AltoResourceTreeViewItem>();
            DoCollectSpriteAtlases(items, true);
            DoCollectSpriteAtlases(items, false);
            DoCollectSprites(items, true);
            DoCollectSprites(items, false);
            DoCollectObjects(items, true);
            DoCollectObjects(items, false);
            AssignId(items);
            return items;
        }

        public List<AltoResourceTreeViewItem> CollectSprites()
        {
            if (!IsReady()) { return null; }

            var items = new List<AltoResourceTreeViewItem>();
            DoCollectSpriteAtlases(items, true);
            DoCollectSpriteAtlases(items, false);
            DoCollectSprites(items, true);
            DoCollectSprites(items, false);
            AssignId(items);
            return items;
        }

        public List<AltoResourceTreeViewItem> CollectScriptableObjects()
        {
            if (!IsReady()) { return null; }

            var items = new List<AltoResourceTreeViewItem>();
            DoCollectObjects(items, true);
            DoCollectObjects(items, false);
            AssignId(items);
            return items;
        }

        //----------------------------------------------------------------------
        // private
        //----------------------------------------------------------------------

        bool IsReady()
        {
            return (EditorApplication.isPlaying && Alto.context != null);
        }

        void AssignId(List<AltoResourceTreeViewItem> items)
        {
            for (int i = 0; i < items.Count; ++i)
            {
                items[i].id = i;
            }
        }

        void DoCollectSpriteAtlases(List<AltoResourceTreeViewItem> items, bool isGlobal)
        {
            var resources = isGlobal ? Alto.globalResources : Alto.resources;
            var atlases = ((ResourceStore)resources).atlases;

            foreach (var key in atlases.Keys)
            {
                items.Add(new AltoResourceTreeViewItem(0)
                {
                    category = "Sprite Atlas", assetName = key, isGlobal = isGlobal,
                    memorySize = GetSpriteAtlasTextureMemory(atlases[key]),
                });
            }
        }

        void DoCollectSprites(List<AltoResourceTreeViewItem> items, bool isGlobal)
        {
            var resources = isGlobal ? Alto.globalResources : Alto.resources;
            var sprites = ((ResourceStore)resources).sprites;

            foreach (var key in sprites.Keys)
            {
                items.Add(new AltoResourceTreeViewItem(0)
                {
                    category = "Sprite", assetName = key, isGlobal = isGlobal,
                    memorySize = -1,
                });
            }
        }

        void DoCollectObjects(List<AltoResourceTreeViewItem> items, bool isGlobal)
        {
            var resources = isGlobal ? Alto.globalResources : Alto.resources;
            var objects = ((ResourceStore)resources).objects;

            foreach (var key in objects.Keys)
            {
                items.Add(new AltoResourceTreeViewItem(0)
                {
                    category = "Scriptable Object", assetName = key, isGlobal = isGlobal,
                    memorySize = GetScriptableObjectMemory(objects[key]),
                });
            }
        }

        /// <summary>
        ///   SpriteAtlas のテクスチャのメモリサイズを取得。
        ///   中身の Sprite の texture を見ることで無理やり取得している。
        ///   そのため現状の実装は 1 枚にパッキングされている前提（複数枚のアトラスは未対応）
        /// <summary>
        long GetSpriteAtlasTextureMemory(SpriteAtlas atlas)
        {
            if (atlas.spriteCount == 0) { return 0; }

            Sprite[] sprites = new Sprite[atlas.spriteCount];
            atlas.GetSprites(sprites);
            return Profiler.GetRuntimeMemorySizeLong(sprites[0].texture);
        }

        /// <summary>
        ///   ScriptableObject のメモリサイズの目安を取得。
        ///   厳密なメモリ使用量を取得するのは難しそうなので
        ///   とりあえず Json にシリアライズした際のバイト数を返す
        /// <summary>
        int GetScriptableObjectMemory(ScriptableObject obj)
        {
            string json = JsonUtility.ToJson(obj);
            Encoding utf8 = Encoding.UTF8;
            return utf8.GetByteCount(json);
        }
    }
}
#endif
