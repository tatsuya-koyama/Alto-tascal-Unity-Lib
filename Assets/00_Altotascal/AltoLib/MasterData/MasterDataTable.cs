using System.Collections.Generic;
using UnityEngine;

namespace AltoLib
{
    public interface IMasterDataTable<TSchema>
    {
        List<TSchema> records { get; }
        void Import(List<string> csvLines);
    }

    public interface IMasterDataSchema
    {
        int PrimaryId { get; }
        string PrimaryKey { get; }
    }

    public class MasterDataTable<TSchema> : ScriptableObject, IMasterDataTable<TSchema>
        where TSchema : IMasterDataSchema, new()
    {
        public static bool VerboseLogMode = false;

        public List<TSchema> _records = new();
        public List<TSchema> records => _records;

        /// <summary>
        /// 「シンプルな」CSV の文字列リストを ScriptableObject に読み込む。
        /// 想定している CSV のフォーマット：
        ///   ・1 行目がヘッダ情報
        ///   ・値がクォートで囲まれていない
        ///   ・セル内のコンマと改行は <comma> と <br> で表現されている
        ///   ・List の要素はセル内のコンマで区切られている
        /// </summary>
        public void Import(List<string> csvLines)
        {
            if (!MasterDataCsvDeserializer<TSchema>.TryDeserialize(
                csvLines,
                CustomImport,
                VerboseLogMode,
                out var importedRecords
            ))
            {
                return;
            }

            _records.Clear();
            _records.AddRange(importedRecords);
        }

        /// <summary>
        /// 標準変換では扱えないフィールドだけを変換する。
        /// 変換した場合は true を返す。
        /// </summary>
        protected virtual bool CustomImport(TSchema dataRecord, string key, string value)
        {
            return false;
        }
    }
}
