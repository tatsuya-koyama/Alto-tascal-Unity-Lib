﻿#if UNITY_EDITOR
using AltoFramework;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;

namespace AltoLib
{
    public class MasterDataCsvImporter
    {
        /// <summary>
        /// csv からマスタデータのインポートを行う。
        /// AssetPostprocessor の OnPostprocessAllAssets で使用する想定。
        ///
        /// 【使用例】
        /// public class YourImporter : AssetPostprocessor
        ///     static void OnPostprocessAllAssets(
        ///         string[] importedAssets,
        ///         string[] deletedAssets,
        ///         string[] movedAssets,
        ///         string[] movedFromAssetPaths
        ///     )
        ///     {
        ///         MasterDataCsvImporter.Import(
        ///             importedAssets, @"Assets/.../(.*)\.csv", "Assets/Resources/MasterData", GetDataType
        ///         );
        ///     }
        ///
        ///     static Type GetDataType(string dataName)
        ///     {
        ///         string typeName = $"YourNamespace.{ dataName }DataTable";
        ///         return System.Type.GetType(typeName);
        ///     }
        /// }
        ///
        /// ・事前にインポート先の ScriptableObject を作成しておく必要がある
        /// ・基本的に各種ファイル名、クラス名はデータごとに揃えておく規約。
        ///   （Unity の ScriptableObject がファイル名・クラス名を一致させておく必要があることにも起因）
        ///   例えばデータのクラス名が HogeDataTable : MasterDataTable<HogeMaster> の場合、
        ///     * クラス実装のファイル名 : HogeDataTable.cs
        ///     * csv ファイル : HogeDataTable.csv
        ///     * 読み込み先の ScriptableObject : HogeDataTable.asset
        ///   とする
        /// </summary>
        public static void Import(
            string[] importedAssets, string csvPathPattern, string dataPath,
            Func<string, Type> dataTypeGetter
        )
        {
            foreach (string assetPath in importedAssets)
            {
                Match match = Regex.Match(assetPath, csvPathPattern);
                if (match.Success)
                {
                    string dataName = match.Groups[1].Value;
                    ImportCsv(assetPath, dataName, dataPath, dataTypeGetter);
                }
            }
        }

        static void ImportCsv(
            string csvPath, string dataName, string dataPath,
            Func<string, Type> dataTypeGetter
        )
        {
            Alto.Log.Info($"Master data csv update detected : { csvPath }");

            string destDataPath = $"{ dataPath }{ dataName }.asset";
            var data = AssetDatabase.LoadMainAssetAtPath(destDataPath);
            if (data == null)
            {
                Alto.Log.Error($"Master data ScriptableObject not exist : { destDataPath }");
                return;
            }

            var type = dataTypeGetter(dataName);
            if (type == null)
            {
                Alto.Log.Error($"Type reflection failed : { dataName }");
                return;
            }
            MethodInfo method = type.GetMethod("Import");
            if (method == null)
            {
                Alto.Log.Error($"Method reflection failed");
                return;
            }

            var csvLines = LoadCsvFile(csvPath);
            method.Invoke(data, new object[]{ csvLines });

            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
            Alto.Log.Success($"Master data import succeeded : { dataName }");
        }

        static List<string> LoadCsvFile(string csvPath)
        {
            var csvLines = new List<string>();
            using (var reader = new StreamReader(csvPath))
            {
                while (!reader.EndOfStream)
                {
                    csvLines.Add(reader.ReadLine());
                }
            }
            return csvLines;
        }
    }
}
#endif
