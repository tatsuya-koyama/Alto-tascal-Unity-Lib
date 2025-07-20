using Cysharp.Threading.Tasks;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;

namespace AltoLib
{
    public class StorageIO_LocalFile : IStorageIO
    {
        const int IvSeedLength = 22;

        //----------------------------------------------------------------------
        // Write
        //----------------------------------------------------------------------

        /// <summary>
        /// 暗号化してローカルファイルに保存
        /// </summary>
        public async UniTask<bool> WriteFileAsync(
            IAltoStorageData data, string cryptoKey, string dataPath, string slotPrefix
        )
        {
            try
            {
                string json = JsonUtility.ToJson(data);
                byte[] dataBytes = Encoding.UTF8.GetBytes(json);

                string ivSeed = IdUtil.GetGuidAs22Chars();
                dataBytes = AltoCrypto.Encrypt(dataBytes, cryptoKey, ivSeed);
                byte[] ivSeedBytes = Encoding.UTF8.GetBytes(ivSeed);

                string path = $"{ dataPath }/{ slotPrefix }{ data.SaveFileName() }";
                string tmpPath = path + ".tmp";
                using (var fileStream = new FileStream(tmpPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    await fileStream.WriteAsync(ivSeedBytes, 0, ivSeedBytes.Length);
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

        /// <summary>
        /// デバッグ用に Plain Text の json ファイルを保存
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

        public async UniTask<bool> ReadFileAsync(
            IAltoStorageData data, string cryptoKey, string dataPath, string slotPrefix
        )
        {
            string path = $"{ dataPath }/{ slotPrefix }{ data.SaveFileName() }";
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
                dataBytes = AltoCrypto.Decrypt(dataBytes, cryptoKey, ivSeed);

                string json = Encoding.UTF8.GetString(dataBytes);
                data.OnDeserialize(json);
                data.ClearDirty();
                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
                return false;
            }
        }

        //----------------------------------------------------------------------
        // Console Log
        //----------------------------------------------------------------------

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        void Log(string message)
        {
            UnityEngine.Debug.Log($"<color=#61d521>[AltoStorage-LocalFile]</color> {message}");
        }
    }
}
