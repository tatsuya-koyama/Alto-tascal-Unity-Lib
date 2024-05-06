using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public List<TSchema> _records = new List<TSchema>();
        public List<TSchema> records { get { return _records; } }

        /// <summary>
        /// 1 セル目がこの値で始まっていたらコメント行とみなす
        /// </summary>
        protected string CommentSymbol => "#";

        /// <summary>
        /// 「シンプルな」CSV の文字列リストを ScriptableObject に読み込む。
        /// 想定している CSV のフォーマット：
        ///   ・1 行目がヘッダ情報
        ///   ・値がクォートで囲まれていない
        ///   ・値にコンマや改行を含まない
        /// </summary>
        public virtual void Import(List<string> csvLines)
        {
            if (csvLines == null || csvLines.Count == 0)
            {
                Debug.LogWarning("csvLines is empty");
                return;
            }

            var headerValues = csvLines[0].Split(',').ToList();
            headerValues = headerValues.Where(str => str.Length > 0).ToList();
            csvLines.RemoveAt(0);

            _records.Clear();

            foreach (string csv in csvLines)
            {
                var dataRecord = new TSchema();
                var values = csv.Split(',');
                if (!IsValidLine(values, headerValues.Count)) { continue; }
                if (IsCommentLine(values)) { continue; }

                for (int i = 0; i < values.Length && i < headerValues.Count; ++i)
                {
                    string key   = headerValues[i];
                    string value = values[i];
                    if (!CustomImport(dataRecord, key, value))
                    {
                        SetField(dataRecord, key, value);
                    }
                }
                _records.Add(dataRecord);
            }
        }

        //----------------------------------------------------------------------
        // protected
        //----------------------------------------------------------------------

        /// <summary>
        /// 特定の変換処理を行いたい場合は以下を override し、
        /// 変換を行った場合は true を返すようにする
        /// </summary>
        protected virtual bool CustomImport(TSchema dataRecord, string key, string value)
        {
            return false;
        }

        protected virtual List<string> SplitStr(string delimiter, string str)
        {
            return str.Split(new string[]{delimiter}, StringSplitOptions.None).ToList();
        }

        protected virtual bool IsCommentLine(string[] csvValues)
        {
            if (csvValues[0] == null) { return false; }
            return csvValues[0].StartsWith(CommentSymbol);
        }

        protected virtual bool IsValidLine(string[] csvValues, int numKey)
        {
            // データ部が全て空欄の行は無視
            bool isAllEmpty = true;
            for (int i = 0; i < numKey; ++i)
            {
                string value = csvValues[i];
                if (value != String.Empty) { isAllEmpty = false; break; }
            }
            if (isAllEmpty) { return false; }

            return true;
        }

        /// <summary>
        /// CSV ヘッダのキーに対応するフィールドにリフレクションで値を反映
        /// </summary>
        protected virtual void SetField(TSchema dataRecord, string key, string value)
        {
            // 先頭が # で始まるカラム名はスキップ
            if (key.StartsWith(CommentSymbol)) { return; }

            FieldInfo fieldInfo = dataRecord.GetType().GetField(key, BindingFlags.Public | BindingFlags.Instance);
            if (fieldInfo == null)
            {
                Debug.LogWarning($"field not found : {key}");
                return;
            }

            System.Type type = fieldInfo.FieldType;
            if (value == "" && type != typeof(string)) { value = "0"; }

            if      (type == typeof(int))    { fieldInfo.SetValue(dataRecord, int.Parse(value)); }
            else if (type == typeof(long))   { fieldInfo.SetValue(dataRecord, long.Parse(value)); }
            else if (type == typeof(string)) { fieldInfo.SetValue(dataRecord, value); }
            else if (type == typeof(float))  { fieldInfo.SetValue(dataRecord, float.Parse(value)); }
            else if (type == typeof(double)) { fieldInfo.SetValue(dataRecord, double.Parse(value)); }
            else if (type == typeof(bool))   { fieldInfo.SetValue(dataRecord, value != "0"); }
        }
    }
}
