using System;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace AltoLib.Editor
{
    internal class MasterDataCsvDownloadResult
    {
        public bool succeeded;
        public bool changed;
        public string csvAssetPath;
        public string error;
    }

    internal static class MasterDataCsvDownloader
    {
        const int TimeoutSeconds = 30;

        public static async UniTask<MasterDataCsvDownloadResult> DownloadAsync(
            MasterDataImporterConfig config,
            MasterDataSheetConfig sheet
        )
        {
            var result = new MasterDataCsvDownloadResult
            {
                csvAssetPath = config.GetCsvAssetPath(sheet),
            };

            try
            {
                string url = BuildUrl(config.spreadsheetId, sheet.gid);
                using var request = UnityWebRequest.Get(url);
                request.timeout = TimeoutSeconds;
                request.SetRequestHeader("Cache-Control", "no-cache");
                request.SetRequestHeader("Pragma", "no-cache");
                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    result.error = $"Download failed ({ request.responseCode }): { request.error }";
                    return result;
                }

                string csvText = request.downloadHandler.text;
                if (LooksLikeHtml(csvText))
                {
                    result.error = "The response looks like an HTML error page, not CSV.";
                    return result;
                }

                string parsedCsv = MasterDataCsvParser.Parse(csvText);
                result.changed = SaveIfChanged(result.csvAssetPath, parsedCsv);
                result.succeeded = true;
                return result;
            }
            catch (Exception exception)
            {
                result.error = exception.Message;
                return result;
            }
        }

        static string BuildUrl(string spreadsheetId, string gid)
        {
            return "https://docs.google.com/spreadsheets/d/e/"
                + Uri.EscapeDataString(spreadsheetId.Trim())
                + "/pub?gid="
                + Uri.EscapeDataString(gid.Trim())
                + "&single=true&output=csv&cacheBust="
                + Guid.NewGuid().ToString("N");
        }

        static bool SaveIfChanged(string csvAssetPath, string csvText)
        {
            string currentText = File.Exists(csvAssetPath)
                ? File.ReadAllText(csvAssetPath)
                : null;
            if (string.Equals(currentText, csvText, StringComparison.Ordinal))
            {
                return false;
            }

            string directory = Path.GetDirectoryName(csvAssetPath);
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException($"CSV output directory not found: { directory }");
            }

            string tempPath = $"{ csvAssetPath }.{ Guid.NewGuid():N}.importing";
            try
            {
                File.WriteAllText(tempPath, csvText, new UTF8Encoding(false));
                if (File.Exists(csvAssetPath))
                {
                    File.Replace(tempPath, csvAssetPath, null);
                }
                else
                {
                    File.Move(tempPath, csvAssetPath);
                }
            }
            finally
            {
                if (File.Exists(tempPath)) { File.Delete(tempPath); }
            }
            return true;
        }

        static bool LooksLikeHtml(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) { return false; }
            string trimmed = text.TrimStart();
            return trimmed.StartsWith("<!DOCTYPE html", StringComparison.OrdinalIgnoreCase)
                || trimmed.StartsWith("<html", StringComparison.OrdinalIgnoreCase);
        }
    }
}
