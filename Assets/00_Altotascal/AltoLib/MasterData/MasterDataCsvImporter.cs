#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace AltoLib
{
    public class MasterDataCsvImporter
    {
        /// <summary>
        /// AssetPostprocessor が検知したアセットから対象の csv を抽出し、
        /// マスタデータのインポートを行う。
        ///
        /// 【使用例】
        /// ・以下のようなコードを置くと、対象の csv ファイル更新時に
        ///   ScriptableObject にインポートする機構が作れる：
        ///
        /// public class YourImporter : AssetPostprocessor
        ///     static void OnPostprocessAllAssets(
        ///         string[] importedAssets,
        ///         string[] deletedAssets,
        ///         string[] movedAssets,
        ///         string[] movedFromAssetPaths
        ///     )
        ///     {
        ///         MasterDataCsvImporter.Import(
        ///             importedAssets,
        ///             @"Assets/.../(.*)\.csv",        // インポート対象とする csv のパスパターン
        ///             "Assets/Resources/MasterData",  // インポート先の ScriptableObject を置く場所
        ///             GetDataType
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
        /// ※ csv のソースが Google スプレッドシートであるなら、Unity Editor 上で動く
        ///    専用のインポーターが使える。（その場合上記のコードは必要ない）
        ///    詳しくは以下を参照： AltoLib/Editor/MasterData/README.md
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

        /// <summary>
        /// 指定した csv からマスタデータのインポートを行う。
        /// </summary>
        public static bool ImportCsv(
            string csvPath, string dataName, string dataPath,
            Func<string, Type> dataTypeGetter
        )
        {
            Debug.Log($"<color=#eeee33>Master data csv update detected :</color> { csvPath }");

            string destDataPath = $"{ dataPath }{ dataName }.asset";
            var data = AssetDatabase.LoadMainAssetAtPath(destDataPath);
            if (data == null)
            {
                Debug.LogError($"Master data ScriptableObject not exist : { destDataPath }");
                return false;
            }

            var type = dataTypeGetter(dataName);
            if (type == null)
            {
                Debug.LogError($"Type reflection failed : { dataName }");
                return false;
            }
            MethodInfo method = type.GetMethod("Import");
            if (method == null)
            {
                Debug.LogError($"Method reflection failed");
                return false;
            }

            try
            {
                var csvLines = LoadCsvFile(csvPath);
                method.Invoke(data, new object[]{ csvLines });

                EditorUtility.SetDirty(data);
                AssetDatabase.SaveAssets();
                Debug.Log($"<color=#33ee00>Master data import succeeded :</color> { dataName }");
                return true;
            }
            catch (Exception exception)
            {
                Exception cause = exception is TargetInvocationException && exception.InnerException != null
                    ? exception.InnerException
                    : exception;
                Debug.LogError($"Master data import failed : { dataName }\n{ cause }");
                return false;
            }
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
