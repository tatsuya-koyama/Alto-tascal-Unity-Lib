using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace AltoLib
{
    public abstract class AltoStorage
    {
        public bool logVerbose = true;

        /// <summary>
        /// 現在使用中のセーブファイルのスロット番号。セーブ / ロード前にこの値を指定しておく
        /// （スロット番号ごとの prefix がついた状態でセーブファイルが保存される）
        /// </summary>
        public int slotIndex = 1;

        protected List<IAltoStorageData> _dataList;

        protected abstract string CryptoKey();

        /// <summary>
        /// Returns all data members list.
        /// </summary>
        protected abstract List<IAltoStorageData> InitDataList();

        protected IStorageIO _storageIO;
        protected virtual IStorageIO StorageIO => _storageIO ??= new StorageIO_LocalFile();

        protected virtual string DataPath()
        {
            // Android の場合 persistentDataPath は外部ストレージを返すことがあり
            // ユーザに公開状態になってしまうので、ネイティブ経由で内部ストレージのパスを返す
            #if UNITY_ANDROID && !UNITY_EDITOR
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var getFilesDir = currentActivity.Call<AndroidJavaObject>("getFilesDir"))
            {
                return getFilesDir.Call<string>("getCanonicalPath");
            }
            #else
            return Application.persistentDataPath;
            #endif
        }

        protected virtual string SlotPrefix()
        {
            return $"s{slotIndex}-";
        }

        public AltoStorage()
        {
            _dataList = InitDataList();
        }

        //----------------------------------------------------------------------
        // Save
        //----------------------------------------------------------------------

        public async UniTask<bool> SaveDirty()
        {
            var dirtyDataList = _dataList.Where(data => data.IsDirty()).ToList();
            Log($"{dirtyDataList.Count} dirty data found.");
            if (dirtyDataList.Count == 0) { return true; }

            var tasks = dirtyDataList.Select(data => SaveAsync(data, true));
            var results = await UniTask.WhenAll(tasks);
            return !results.Any(x => x == false);
        }

        public async UniTask<bool> SaveAll()
        {
            var tasks = _dataList.Select(data => SaveAsync(data));
            var results = await UniTask.WhenAll(tasks);
            return !results.Any(x => x == false);
        }

        /// <summary>
        /// スロット指定版セーブ。処理後は選択中 slotIndex は元の値に戻る
        /// </summary>
        public async UniTask<bool> SaveAll(int spotSlotIndex)
        {
            int oldSlotIndex = this.slotIndex;
            this.slotIndex = spotSlotIndex;
            bool result = await SaveAll();
            this.slotIndex = oldSlotIndex;
            return result;
        }

        public async UniTask<bool> SaveAsync(IAltoStorageData data, bool useDirtyCache = false)
        {
            // 二重実行禁止
            if (data.isWriting)
            {
                LogError($"Now file is writing, so skipped save process : {data.SaveFileName()}");
                return false;
            }

            data.isWriting = true;
            data.SavePreProcess();
            data.OnBeforeSave();

            #if UNITY_EDITOR
            await StorageIO.WriteDebugFileAsync(data, DataPath(), SlotPrefix());
            #endif
            bool succeeded = await StorageIO.WriteFileAsync(data, CryptoKey(), DataPath(), SlotPrefix());

            if (succeeded) { data.ClearDirty(useDirtyCache); }
            data.isWriting = false;
            return succeeded;
        }

        //----------------------------------------------------------------------
        // Load
        //----------------------------------------------------------------------

        public async UniTask<bool> LoadAll()
        {
            var tasks = _dataList.Select(data => LoadAsync(data));
            var results = await UniTask.WhenAll(tasks);
            return !results.Any(x => x == false);
        }

        /// <summary>
        /// スロット指定版ロード。処理後は選択中 slotIndex は元の値に戻る
        /// </summary>
        public async UniTask<bool> LoadAll(int spotSlotIndex)
        {
            int oldSlotIndex = this.slotIndex;
            this.slotIndex = spotSlotIndex;
            bool result = await LoadAll();
            this.slotIndex = oldSlotIndex;
            return result;
        }

        public async UniTask<bool> LoadAsync(IAltoStorageData data)
        {
            bool succeeded = await StorageIO.ReadFileAsync(data, CryptoKey(), DataPath(), SlotPrefix());
            if (succeeded) { MigrateIfNeeded(data); }
            data.OnAfterLoad();
            return succeeded;
        }

        void MigrateIfNeeded(IAltoStorageData data)
        {
            if (data.savedSchemaVersion >= data.schemaVersion) { return; }

            Log($"Migration { data.GetType() } : {data.savedSchemaVersion} -> {data.schemaVersion}");
            if (data.savedSchemaVersion + 1000 < data.schemaVersion)
            {
                LogError("Data shcema version is too big! / Migration is canceled.");
                return;
            }

            int versionHead = data.savedSchemaVersion;
            while (versionHead < data.schemaVersion)
            {
                data.OnMigrateData(versionHead + 1);
                ++versionHead;
            }
        }

        //----------------------------------------------------------------------
        // Console Log
        //----------------------------------------------------------------------

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        protected void Log(string message)
        {
            if (!logVerbose) { return; }
            UnityEngine.Debug.Log($"<color=#61d521>[AltoStorage]</color> {message}");
        }

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        protected void LogError(string message)
        {
            UnityEngine.Debug.LogError($"<color=#61d521>[AltoStorage]</color> [Error] {message}");
        }
    }
}
