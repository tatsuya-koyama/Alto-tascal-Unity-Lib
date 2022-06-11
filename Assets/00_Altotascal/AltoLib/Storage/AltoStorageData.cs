using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace AltoLib
{
    public interface IAltoStorageData
    {
        int schemaVersion { get; }
        int savedSchemaVersion { get; }
        string SaveFileName();
        string SaveFileNameForDebug();

        void SavePreProcess();
        void OnDeserialize(string json);

        void OnCreateNewData();
        void OnMigrateData(int targetVersion);
        void OnBeforeSave();
        void OnAfterLoad();

        string GetCustomHash();
        void ClearDirty(bool useCache = false);
        bool IsDirty();

        bool isWriting { get; set; }
    }

    [System.Serializable]
    public class AltoStorageData : IAltoStorageData
    {
        // 内部制御用
        public bool isWriting { get; set; } = false;

        public int _savedSchemaVersion = 0;

        [NonSerialized] public bool logVerbose = true;
        [NonSerialized] protected string _prevHash;
        [NonSerialized] protected string _lastCalculatedHash;

        public override string ToString()
        {
            return $"{ base.ToString() } - { JsonUtility.ToJson(this) }";
        }

        [Conditional("ALTO_DEBUG")]
        void Log(string message)
        {
            if (!logVerbose) { return; }
            UnityEngine.Debug.Log($"[{this.GetType()}] {message}");
        }

        //----------------------------------------------------------------------
        // 必要に応じてサブクラスで override してほしいシリーズ
        //----------------------------------------------------------------------

        /// <summary>
        /// データのスキーマバージョン。更新時は 1 ずつインクリメントすること
        /// </summary>
        public virtual int schemaVersion => 1;

        /// <summary>
        /// ファイルをロードしようとしたがまだ無かった場合に呼び出される。
        /// データの初期化処理を各サブクラスにて実装
        /// </summary>
        public virtual void OnCreateNewData()
        {
        }

        /// <summary>
        /// アプリのアップデートでスキーマが変わった場合のデータのマイグレーション処理。
        /// OnDeserialize() 後、schemaVersion の値がロードされたデータのバージョンよりも
        /// 大きかった場合に、差分のバージョンの回数ぶん呼び出される。
        ///
        /// 例として保存されていたデータが schemaVersion = 2 で、
        /// それを呼び出したコードの定義が schemaVersion = 4 だった場合、
        /// 引数が targetVersion = 3, targetVersion = 4 で 2 回呼び出される。
        /// </summary>
        public virtual void OnMigrateData(int targetVersion)
        {
        }

        /// <summary>
        /// ファイルにセーブする前に呼ばれる
        /// </summary>
        public virtual void OnBeforeSave()
        {
        }

        /// <summary>
        /// ファイルからロードした後に呼ばれる
        /// </summary>
        public virtual void OnAfterLoad()
        {
        }

        //----------------------------------------------------------------------
        // 基本的にデフォルト実装のままで問題ないシリーズ
        //----------------------------------------------------------------------

        public virtual int savedSchemaVersion => _savedSchemaVersion;

        public virtual string SaveFileName()
        {
            return DigestUtil.GetMD5(GetType().Name);
        }

        public virtual string SaveFileNameForDebug()
        {
            return $"{ GetType().Name }.json";
        }

        public virtual void SavePreProcess()
        {
            _savedSchemaVersion = schemaVersion;
        }

        /// <summary>
        /// ファイルがロードされた時に呼び出される。
        /// ファイルの内容が json 文字列が渡ってくるのでデシリアライズして自身に反映する
        /// </summary>
        public virtual void OnDeserialize(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }

        /// <summary>
        /// 変更差分検知のためのデータのダイジェスト計算。
        /// ここでの実装はバイナリにシリアライズして MD5 をとる汎用実装だが、
        /// データがフラットな場合は以下のような実装に置き換えるやり方もある。
        /// こちらの方が処理は軽い（が、ハッシュの衝突リスクは上がる）：
        /// <example>
        ///   return (
        ///     field1, field2, field3, ...
        ///   ).GetHashCode().ToString();
        /// </example>
        /// </summary>
        public virtual string GetCustomHash()
        {
            using (var memoryStream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, this);
                memoryStream.Position = 0;
                string hash = DigestUtil.GetMD5(memoryStream);
                _lastCalculatedHash = hash;
                Log($"Hash : {hash}");
                return hash;
            }
        }

        public virtual void ClearDirty(bool useCache = false)
        {
            _prevHash = useCache ? _lastCalculatedHash : GetCustomHash();
        }

        public virtual bool IsDirty()
        {
            return GetCustomHash() != _prevHash;
        }
    }
}
