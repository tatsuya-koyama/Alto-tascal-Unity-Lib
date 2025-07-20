using AltoFramework;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AltoFramework
{
    /// <summary>
    /// リソースの Addressable アドレスと参照カウントを保持。
    /// ToDo : 現状は参照カウントが 0 でアンロードされた後もエントリは残り続ける。
    ///        不要になったエントリを適当なタイミングで掃除したい
    /// </summary>
    public class ResourceRegistry
    {
        public class ResourceEntry
        {
            public string address;
            public int sceneScopeRefCount = 0;
            public int globalScopeRefCount = 0;
            public int refCount => sceneScopeRefCount + globalScopeRefCount;
            public bool loaded = false;
            public Type type;
            public AsyncOperationHandle handle;
        }
        Dictionary<string, ResourceEntry> _entries = new Dictionary<string, ResourceEntry>();

        //----------------------------------------------------------------------
        // データ操作
        //----------------------------------------------------------------------

        public void Clear()
        {
            _entries.Clear();
        }

        /// <summary>
        /// 参照カウントを +1
        /// </summary>
        public void Retain(string assetAddress, bool isGlobalScope = false)
        {
            if (!_entries.ContainsKey(assetAddress))
            {
                _entries.Add(assetAddress, new ResourceEntry() { address = assetAddress });
            }

            if (isGlobalScope) {
                _entries[assetAddress].globalScopeRefCount += 1;
            } else {
                _entries[assetAddress].sceneScopeRefCount += 1;
            }
        }

        /// <summary>
        /// 参照カウントを -1
        /// </summary>
        public void Release(string assetAddress, bool isGlobalScope = false)
        {
            if (!ValidateKey(assetAddress)) { return; }

            if (isGlobalScope) {
                _entries[assetAddress].globalScopeRefCount -= 1;
            } else {
                _entries[assetAddress].sceneScopeRefCount -= 1;
            }
        }

        /// <summary>
        /// カウントが 1 以上の全てのリソースの参照カウントを -1
        /// </summary>
        public void ReleaseAll(bool isGlobalScope = false)
        {
            foreach (var entry in _entries.Values)
            {
                if (isGlobalScope) {
                    if (entry.globalScopeRefCount > 0) { entry.globalScopeRefCount -= 1; }
                } else {
                    if (entry.sceneScopeRefCount > 0) { entry.sceneScopeRefCount -= 1; }
                }
            }
        }

        /// <summary>
        /// ロード済みをマーク。リソースの型とアンロード用のハンドラもここで保持
        /// </summary>
        public void MarkLoaded(
            string assetAddress, UnityEngine.Object resource, AsyncOperationHandle handle
        )
        {
            if (!ValidateKey(assetAddress)) { return; }
            var entry = _entries[assetAddress];
            entry.loaded = true;
            entry.type   = resource.GetType();
            entry.handle = handle;
        }

        /// <summary>
        /// ロード済みのマークを解除
        /// </summary>
        public void UnmarkLoaded(string assetAddress)
        {
            if (!ValidateKey(assetAddress)) { return; }
            var entry = _entries[assetAddress];
            entry.loaded = false;
            entry.type   = null;
        }

        //----------------------------------------------------------------------
        // 状態取得
        //----------------------------------------------------------------------

        public ResourceEntry GetEntry(string assetAddress)
        {
            ResourceEntry entry;
            if (!_entries.TryGetValue(assetAddress, out entry))
            {
                Alto.Log.FW_Error($"[ResourceRegistry] Entry not found : {assetAddress}");
                return null;
            }
            return entry;
        }

        public List<ResourceEntry> GetEntries()
        {
            return _entries.Values.Where(x => x.type != null).ToList();
        }

        /// <summary>
        /// 参照カウントが 1 以上（ロードされているべきリソース）なら true
        /// </summary>
        public bool IsReferenced(string assetAddress)
        {
            return GetEntry(assetAddress).refCount > 0;
        }

        /// <summary>
        /// ロードが必要なリソース（参照カウントが 1 以上で未ロードのリソース）
        /// が 1 つでも残っていたら true
        /// </summary>
        public bool ShouldLoadAny()
        {
            foreach (var entry in _entries.Values)
            {
                if (!entry.loaded && entry.refCount > 0) { return true; }
            }
            return false;
        }

        /// <summary>
        /// ロードが必要なリソース（参照カウントが 1 以上で未ロードのリソース）
        /// のアドレスのリストを返す
        /// </summary>
        public List<string> GetAddressesToLoad()
        {
            return _entries.Values.Where(x => !x.loaded && x.refCount > 0)
                           .Select(x => x.address)
                           .ToList();
        }

        /// <summary>
        /// アンロードが必要なリソース（ロード済みで参照カウントが 0 未満のリソース）
        /// のアドレスのリストを返す
        /// </summary>
        public List<string> GetAddressesToUnload()
        {
            return _entries.Values.Where(x => x.loaded && x.refCount <= 0)
                           .Select(x => x.address)
                           .ToList();
        }

        //----------------------------------------------------------------------
        // private
        //----------------------------------------------------------------------

        bool ValidateKey(string assetAddress)
        {
            if (!_entries.ContainsKey(assetAddress))
            {
                Alto.Log.FW_Warn($"[ResourceRegistry] Key not found : {assetAddress}");
                return false;
            }
            return true;
        }
    }
}
