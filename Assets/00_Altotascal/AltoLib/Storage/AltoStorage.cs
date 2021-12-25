using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        const int IvSeedLength = 22;

        protected abstract string CryptoKey();

        /// <summary>
        /// Returns all data members list.
        /// </summary>
        protected abstract List<IAltoStorageData> InitDataList();

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
            await WriteDebugFileAsync(data);
            #endif
            bool succeeded = await WriteFileAsync(data, useDirtyCache);
            data.isWriting = false;
            return succeeded;
        }

        async UniTask<bool> WriteFileAsync(IAltoStorageData data, bool useDirtyCache = false)
        {
            try
            {
                string json = JsonUtility.ToJson(data);
                byte[] dataBytes = Encoding.UTF8.GetBytes(json);

                string ivSeed = IdUtil.GetGuidAs22Chars();
                dataBytes = AltoCrypto.Encrypt(dataBytes, CryptoKey(), ivSeed);
                byte[] ivSeedBytes = Encoding.UTF8.GetBytes(ivSeed);

                string path = $"{ DataPath() }/{ SlotPrefix() }{ data.SaveFileName() }";
                string tmpPath = path + ".tmp";
                using (var fileStream = new FileStream(tmpPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    await fileStream.WriteAsync(ivSeedBytes, 0, ivSeedBytes.Length);
                    await fileStream.WriteAsync(dataBytes, 0, dataBytes.Length);
                    fileStream.Flush(flushToDisk: true);
                }

                if (File.Exists(path)) { File.Delete(path); }
                File.Move(tmpPath, path);
                data.ClearDirty(useDirtyCache);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        /// <summary>
        /// デバッグ用に Plain Text の json ファイルを保存
        /// </summary>
        async UniTask<bool> WriteDebugFileAsync(IAltoStorageData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);
                byte[] dataBytes = Encoding.UTF8.GetBytes(json);

                string path = $"{ DataPath() }/{ SlotPrefix() }{ data.SaveFileNameForDebug() }";
                string tmpPath = path + ".tmp";
                Log($"Write debug file : {path}");
                using (var fileStream = new FileStream(tmpPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    await fileStream.WriteAsync(dataBytes, 0, dataBytes.Length);
                    fileStream.Flush(flushToDisk: true);
                }

                if (File.Exists(path)) { File.Delete(path); }
                File.Move(tmpPath, path);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
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
            bool succeeded = await ReadFileAsync(data);
            data.OnAfterLoad();
            return succeeded;
        }

        async UniTask<bool> ReadFileAsync(IAltoStorageData data)
        {
            string path = $"{ DataPath() }/{ SlotPrefix() }{ data.SaveFileName() }";
            Log($"Read file : {path}");
            if (!File.Exists(path))
            {
                Log($"File not exists, so set initial data : { data.GetType() }");
                data.OnCreateNewData();
                return false;
            }

            try
            {
                byte[] dataBytes;
                byte[] ivSeedBytes = new byte[IvSeedLength];
                using (var fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read))
                {
                    dataBytes = new byte[fileStream.Length - IvSeedLength];
                    await fileStream.ReadAsync(ivSeedBytes, 0, IvSeedLength);
                    await fileStream.ReadAsync(dataBytes, 0, dataBytes.Length);
                }

                string ivSeed = Encoding.UTF8.GetString(ivSeedBytes);
                dataBytes = AltoCrypto.Decrypt(dataBytes, CryptoKey(), ivSeed);

                string json = Encoding.UTF8.GetString(dataBytes);
                data.OnDeserialize(json);
                data.ClearDirty();

                MigrateIfNeeded(data);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
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
        // private
        //----------------------------------------------------------------------

        protected void Log(string message)
        {
            if (!logVerbose) { return; }
            Debug.Log($"<color=#61d521>[AltoStorage]</color> {message}");
        }

        protected void LogError(string message)
        {
            Debug.LogError($"<color=#61d521>[AltoStorage]</color> [Error] {message}");
        }
    }
}
