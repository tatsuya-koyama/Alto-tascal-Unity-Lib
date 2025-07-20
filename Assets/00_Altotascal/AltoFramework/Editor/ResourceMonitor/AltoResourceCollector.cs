#if UNITY_EDITOR
using AltoFramework.Production;
using System.Collections.Generic;
using System.Linq;
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
            DoCollectGameObjects(items);
            DoCollectScriptableObjects(items);
            DoCollectSpriteAtlases(items);
            DoCollectAudioClips(items);
            AssignId(items);
            return items;
        }

        public List<AltoResourceTreeViewItem> CollectGameObjects()
        {
            if (!IsReady()) { return null; }

            var items = new List<AltoResourceTreeViewItem>();
            DoCollectGameObjects(items);
            AssignId(items);
            return items;
        }

        public List<AltoResourceTreeViewItem> CollectScriptableObjects()
        {
            if (!IsReady()) { return null; }

            var items = new List<AltoResourceTreeViewItem>();
            DoCollectScriptableObjects(items);
            AssignId(items);
            return items;
        }

        public List<AltoResourceTreeViewItem> CollectSprites()
        {
            if (!IsReady()) { return null; }

            var items = new List<AltoResourceTreeViewItem>();
            DoCollectSpriteAtlases(items);
            AssignId(items);
            return items;
        }

        public List<AltoResourceTreeViewItem> CollectAudioClips()
        {
            if (!IsReady()) { return null; }

            var items = new List<AltoResourceTreeViewItem>();
            DoCollectAudioClips(items);
            AssignId(items);
            return items;
        }

        //----------------------------------------------------------------------
        // private
        //----------------------------------------------------------------------

        bool IsReady()
        {
            return (EditorApplication.isPlaying && Alto.Context != null);
        }

        void AssignId(List<AltoResourceTreeViewItem> items)
        {
            for (int i = 0; i < items.Count; ++i)
            {
                items[i].id = i;
            }
        }

        void DoCollectGameObjects(List<AltoResourceTreeViewItem> items)
        {
            var entries = Alto.Resource.registry.GetEntries().Where(x => {
                return x.type == typeof(GameObject);
            });
            foreach (var entry in entries)
            {
                var resource = Alto.Resource.GetGameObj(entry.address);
                items.Add(MakeItem(entry, "Prefab", resource));
            }
        }

        void DoCollectScriptableObjects(List<AltoResourceTreeViewItem> items)
        {
            var entries = Alto.Resource.registry.GetEntries().Where(x => {
                return x.type.IsSubclassOf(typeof(ScriptableObject));
            });
            foreach (var entry in entries)
            {
                var resource = Alto.Resource.GetObj<ScriptableObject>(entry.address);
                items.Add(MakeItem(entry, "Scriptable Object", resource));
            }
        }

        void DoCollectSpriteAtlases(List<AltoResourceTreeViewItem> items)
        {
            var entries = Alto.Resource.registry.GetEntries().Where(x => {
                return x.type == typeof(SpriteAtlas);
            });
            foreach (var entry in entries)
            {
                var resource = Alto.Resource.GetSpriteAtlas(entry.address);
                long memorySize = GetSpriteAtlasTextureMemory(resource);
                items.Add(MakeItem(entry, "Sprite Atlas", resource, memorySize));
            }
        }

        void DoCollectAudioClips(List<AltoResourceTreeViewItem> items)
        {
            var entries = Alto.Resource.registry.GetEntries().Where(x => {
                return x.type == typeof(AudioClip);
            });
            foreach (var entry in entries)
            {
                var resource = Alto.Resource.GetAudio(entry.address);
                items.Add(MakeItem(entry, "Audio Clip", resource));
            }
        }

        AltoResourceTreeViewItem MakeItem(
            ResourceRegistry.ResourceEntry entry, string category, Object resource,
            long? _memorySize = null
        )
        {
            long genericMemorySize = (resource == null) ? -1 : Profiler.GetRuntimeMemorySizeLong(resource);
            long memorySize = _memorySize ?? genericMemorySize;
            return new AltoResourceTreeViewItem(0)
            {
                category       = category,
                assetName      = entry.address,
                refCount       = entry.sceneScopeRefCount,
                globalRefCount = entry.globalScopeRefCount,
                loaded         = entry.loaded,
                memorySize     = memorySize,
            };
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
