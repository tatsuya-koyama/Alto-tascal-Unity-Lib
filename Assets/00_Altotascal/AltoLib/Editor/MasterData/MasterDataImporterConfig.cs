using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AltoLib.Editor
{
    [Serializable]
    public class MasterDataSheetConfig
    {
        public string name;
        public string gid;
    }

    [Serializable]
    public class MasterDataImporterConfig
    {
        public string spreadsheetId;
        public string csvPostfix;
        public string csvOutputDirectory;
        public string dataTableOutputDirectory;
        public string dataNamespace;
        public List<MasterDataSheetConfig> sheets = new();

        public string GetDataName(MasterDataSheetConfig sheet)
        {
            return $"{ sheet.name }{ csvPostfix }";
        }

        public string GetCsvAssetPath(MasterDataSheetConfig sheet)
        {
            return $"{ TrimDirectory(csvOutputDirectory) }/{ GetDataName(sheet) }.csv";
        }

        public string GetDataTableDirectory()
        {
            return $"{ TrimDirectory(dataTableOutputDirectory) }/";
        }

        public Type GetDataType(string dataName)
        {
            string fullName = string.IsNullOrWhiteSpace(dataNamespace)
                ? dataName
                : $"{ dataNamespace }.{ dataName }";

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(fullName);
                if (type != null) { return type; }
            }
            return null;
        }

        static string TrimDirectory(string directory)
        {
            return directory?.Trim().TrimEnd('/', '\\').Replace('\\', '/');
        }
    }

    public static class MasterDataImporterConfigLoader
    {
        public const string ConfigPath = "Assets/ProjectSettings/AltoMasterDataImporter.json";

        public static bool TryLoad(out MasterDataImporterConfig config, out string error)
        {
            config = null;
            error = null;

            if (!File.Exists(ConfigPath))
            {
                error = $"Config file not found: { ConfigPath }";
                return false;
            }

            try
            {
                string json = File.ReadAllText(ConfigPath);
                config = JsonUtility.FromJson<MasterDataImporterConfig>(json);
            }
            catch (Exception exception)
            {
                error = $"Failed to read config: { exception.Message }";
                return false;
            }

            error = Validate(config);
            return error == null;
        }

        static string Validate(MasterDataImporterConfig config)
        {
            if (config == null) { return "Config JSON is empty or invalid."; }
            if (string.IsNullOrWhiteSpace(config.spreadsheetId))
            {
                return "spreadsheetId is required.";
            }
            if (!IsSimpleName(config.csvPostfix))
            {
                return "csvPostfix is required and must be a valid name fragment.";
            }
            if (!IsAssetDirectory(config.csvOutputDirectory))
            {
                return "csvOutputDirectory must be an existing directory under Assets/.";
            }
            if (!IsAssetDirectory(config.dataTableOutputDirectory))
            {
                return "dataTableOutputDirectory must be an existing directory under Assets/.";
            }
            if (config.sheets == null || config.sheets.Count == 0)
            {
                return "At least one sheet is required.";
            }

            var dataNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (var sheet in config.sheets)
            {
                if (sheet == null) { return "sheets contains an empty item."; }
                if (string.IsNullOrWhiteSpace(sheet.name)) { return "Each sheet requires name."; }
                if (!long.TryParse(sheet.gid, out long gid) || gid < 0)
                {
                    return $"Sheet '{ sheet.name }' has an invalid gid.";
                }
                if (!IsSimpleName(sheet.name))
                {
                    return $"Sheet '{ sheet.name }' has an invalid name.";
                }
                string dataName = config.GetDataName(sheet);
                if (!dataNames.Add(dataName))
                {
                    return $"Duplicate data name: { dataName }";
                }
            }
            return null;
        }

        static bool IsAssetDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) { return false; }
            string normalized = path.Trim().TrimEnd('/', '\\').Replace('\\', '/');
            if (!normalized.StartsWith("Assets/", StringComparison.Ordinal)) { return false; }
            if (normalized.Contains("/../") || normalized.EndsWith("/..")) { return false; }
            return AssetDatabase.IsValidFolder(normalized);
        }

        static bool IsSimpleName(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) { return false; }
            if (!char.IsLetter(value[0]) && value[0] != '_') { return false; }

            for (int i = 1; i < value.Length; ++i)
            {
                if (!char.IsLetterOrDigit(value[i]) && value[i] != '_') { return false; }
            }
            return true;
        }
    }
}
