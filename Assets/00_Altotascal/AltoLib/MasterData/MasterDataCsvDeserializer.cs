using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace AltoLib
{
    /// <summary>
    /// シンプルな CSV の行をスキーマのレコードへ変換する。
    /// </summary>
    internal static class MasterDataCsvDeserializer<TSchema>
        where TSchema : IMasterDataSchema, new()
    {
        const string CommentSymbol = "#";

        internal static bool TryDeserialize(
            IReadOnlyList<string> csvLines,
            Func<TSchema, string, string, bool> customImport,
            bool verboseLogMode,
            out List<TSchema> records
        )
        {
            records = null;
            if (csvLines == null || csvLines.Count == 0)
            {
                Debug.LogWarning("csvLines is empty");
                return false;
            }

            var headerValues = csvLines[0]
                .Split(',')
                .Where(value => value.Length > 0)
                .ToList();
            var importedRecords = new List<TSchema>();

            for (int lineIndex = 1; lineIndex < csvLines.Count; ++lineIndex)
            {
                string[] values = csvLines[lineIndex].Split(',');
                if (IsCommentLine(values)) { continue; }
                if (!IsValidLine(values, headerValues.Count, lineIndex + 1)) { continue; }

                var dataRecord = new TSchema();
                for (int i = 0; i < headerValues.Count; ++i)
                {
                    string key = headerValues[i];
                    // 列を分割した後で、セル内に退避していたコンマと改行を復元する
                    string value = MasterDataCsvCellCodec.Decode(values[i]);
                    if (!customImport(dataRecord, key, value))
                    {
                        SetField(dataRecord, key, value, verboseLogMode);
                    }
                }
                importedRecords.Add(dataRecord);
            }

            records = importedRecords;
            return true;
        }

        static bool IsCommentLine(string[] csvValues)
        {
            return csvValues[0].StartsWith(CommentSymbol);
        }

        static bool IsValidLine(string[] csvValues, int numKeys, int lineNumber)
        {
            if (csvValues.All(value => value == String.Empty)) { return false; }
            if (csvValues.Length < numKeys)
            {
                throw new FormatException(
                    $"CSV line { lineNumber } has fewer values than the header."
                );
            }
            return true;
        }

        static void SetField(
            TSchema dataRecord,
            string key,
            string value,
            bool verboseLogMode
        )
        {
            // 先頭が # で始まるカラム名はスキップ
            if (key.StartsWith(CommentSymbol)) { return; }

            FieldInfo fieldInfo = dataRecord.GetType().GetField(
                key,
                BindingFlags.Public | BindingFlags.Instance
            );
            if (fieldInfo == null)
            {
                if (verboseLogMode) { Debug.Log($"field not found : { key }"); }
                return;
            }

            if (MasterDataValueConverter.TryConvert(
                fieldInfo.FieldType,
                value,
                out object convertedValue
            ))
            {
                fieldInfo.SetValue(dataRecord, convertedValue);
            }
        }
    }
}
