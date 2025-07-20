using Cysharp.Threading.Tasks;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;

namespace AltoLib
{
    /// <summary>
    /// 永続化先を PlayerPrefs とした読み書き
    /// </summary>
    public class StorageIO_PlayerPrefs : IStorageIO
    {
        const string KeyHead = "savedata/";
        const int IvSeedBase64Length = 32;  // 22 文字を Base64 エンコードすると 32 文字

        //----------------------------------------------------------------------
        // Write
        //----------------------------------------------------------------------

        /// <summary>
        /// 暗号化して PlayerPrefs に保存
        /// </summary>
        public UniTask<bool> WriteFileAsync(
            IAltoStorageData data, string cryptoKey, string dataPath, string slotPrefix
        )
        {
            try
            {
                string json = JsonUtility.ToJson(data);
                byte[] dataBytes = Encoding.UTF8.GetBytes(json);

                string ivSeed = IdUtil.GetGuidAs22Chars();
                dataBytes = AltoCrypto.Encrypt(dataBytes, cryptoKey, ivSeed);
                string dataBase64 = Convert.ToBase64String(dataBytes);

                byte[] ivSeedBytes = Encoding.UTF8.GetBytes(ivSeed);
                string ivSeedBase64 = Convert.ToBase64String(ivSeedBytes);

                string key = $"{ KeyHead }{ slotPrefix }{ data.SaveFileName() }";
                string dataStr = ivSeedBase64 + dataBase64;
                PlayerPrefs.SetString(key, dataStr);
                Log($"Save to PlayerPrefs : {key} - {dataStr.Length} chars");
                return UniTask.FromResult(true);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
                return UniTask.FromResult(false);
            }
        }

        /// <summary>
        /// デバッグ用に Plain Text の json ファイルを保存
        /// ※ こちらはエディタでのデバッグ用でローカルファイルに保存する
        /// </summary>
        public async UniTask<bool> WriteDebugFileAsync(
            IAltoStorageData data, string dataPath, string slotPrefix
        )
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);
                byte[] dataBytes = Encoding.UTF8.GetBytes(json);

                string path = $"{ dataPath }/{ slotPrefix }{ data.SaveFileNameForDebug() }";
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
                UnityEngine.Debug.LogException(e);
                return false;
            }
        }

        //----------------------------------------------------------------------
        // Read
        //----------------------------------------------------------------------

        public UniTask<bool> ReadFileAsync(
            IAltoStorageData data, string cryptoKey, string dataPath, string slotPrefix
        )
        {
            string key = $"{ KeyHead }{ slotPrefix }{ data.SaveFileName() }";
            Log($"Read PlayerPrefs : {key}");
            string dataStr = PlayerPrefs.GetString(key, "");

            if (string.IsNullOrEmpty(dataStr))
            {
                Log($"key not exists, so set initial data : { data.GetType() }");
                data.OnCreateNewData();
                return UniTask.FromResult(false);
            }
            if (dataStr.Length < IvSeedBase64Length)
            {
                Log($"data length not sufficient ({dataStr.Length} / {IvSeedBase64Length}), so set initial data : { data.GetType() }");
                data.OnCreateNewData();
                return UniTask.FromResult(false);
            }

            try
            {
                string ivSeedBase64 = dataStr.Substring(0, IvSeedBase64Length);
                string dataBase64 = dataStr.Substring(IvSeedBase64Length);

                byte[] ivSeedBytes = Convert.FromBase64String(ivSeedBase64);
                byte[] dataBytes = Convert.FromBase64String(dataBase64);

                string ivSeed = Encoding.UTF8.GetString(ivSeedBytes);
                dataBytes = AltoCrypto.Decrypt(dataBytes, cryptoKey, ivSeed);

                string json = Encoding.UTF8.GetString(dataBytes);
                data.OnDeserialize(json);
                data.ClearDirty();
                return UniTask.FromResult(true);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
                return UniTask.FromResult(false);
            }
        }

        //----------------------------------------------------------------------
        // Console Log
        //----------------------------------------------------------------------

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        void Log(string message)
        {
            UnityEngine.Debug.Log($"<color=#61d521>[AltoStorage-PlayerPrefs]</color> {message}");
        }
    }
}
