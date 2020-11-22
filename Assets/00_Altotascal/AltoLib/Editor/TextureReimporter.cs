using UnityEditor;
using UnityEngine;

namespace AltoLib
{
    /// <summary>
    /// テクスチャフォーマットを一括変換するエディタ拡張。
    /// フォーマット形式などの設定は外出ししていないので、カスタムしたい場合は
    /// スクリプトをコピーして適宜書き換えてほしい。
    /// </summary>
    public class TextureReimporter
    {
        //----------------------------------------------------------------------
        // Convert Settings
        //----------------------------------------------------------------------

        const TextureImporterFormat IosTextureFormat     = TextureImporterFormat.ASTC_6x6;
        const TextureImporterFormat AndroidTextureFormat = TextureImporterFormat.ETC2_RGBA8;
        static readonly string[] TargetFolders = {"Assets"};

        public static void SetImportSettingsForIos(TextureImporter textureImporter, string assetPath)
        {
            int originalMaxSize = textureImporter.maxTextureSize;
            textureImporter.SetPlatformTextureSettings(new TextureImporterPlatformSettings
            {
                name               = "iPhone",
                overridden         = true,
                maxTextureSize     = originalMaxSize,
                resizeAlgorithm    = TextureResizeAlgorithm.Mitchell,
                format             = IosTextureFormat,
                textureCompression = TextureImporterCompression.Compressed,
                compressionQuality = 50,
            });
            Debug.Log($"{assetPath} [iOS] : Set {IosTextureFormat.ToString()}");
        }

        public static void SetImportSettingsForAndroid(TextureImporter textureImporter, string assetPath)
        {
            int originalMaxSize = textureImporter.maxTextureSize;
            textureImporter.SetPlatformTextureSettings(new TextureImporterPlatformSettings
            {
                name               = "Android",
                overridden         = true,
                maxTextureSize     = originalMaxSize,
                resizeAlgorithm    = TextureResizeAlgorithm.Mitchell,
                format             = AndroidTextureFormat,
                textureCompression = TextureImporterCompression.Compressed,
                compressionQuality = 50,
            });
            Debug.Log($"{assetPath} [Android] : Set {AndroidTextureFormat.ToString()}");
        }

        //----------------------------------------------------------------------
        // private
        //----------------------------------------------------------------------

        const string ProgressBarTitle = "Update Texture Format";

        /// <summary>
        /// ゲームに使用されているテクスチャの iOS / Android 向け圧縮フォーマットを統一する。
        /// テクスチャの Max Size は既存の設定をそのまま引き継ぎ、圧縮フォーマットのみを指定、
        /// 変更があったものについてアセットをインポートし直す。
        /// </summary>
        [MenuItem("Alto/Convert Texture Format to ASTC or ETC2")]
        static void UpdateTextureCompressionSetting()
        {
            string[] guids = AssetDatabase.FindAssets("t:texture2D", TargetFolders);

            try
            {
                EditorUtility.DisplayProgressBar(ProgressBarTitle, "", 0f);
                for (var i = 0; i < guids.Length; ++i)
                {
                    string guid = guids[i];
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    SetTextureCompression(assetPath, i, guids.Length);
                }

                EditorUtility.DisplayProgressBar(ProgressBarTitle, "Refresh AssetDatabase ...", 1f);
                AssetDatabase.Refresh();
                Debug.Log("Texture format conversion is completed.");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        static void SetTextureCompression(string assetPath, int count, int totalCount)
        {
            // Font テクスチャは除外
            if (assetPath.EndsWith(".ttf")) { return; }
            if (assetPath.EndsWith(".otf")) { return; }

            var textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (textureImporter == null)
            {
                Debug.LogError($"TextureImporter not found : {assetPath}");
                return;
            }

            if (!ShouldReimport(textureImporter)) { return; }
            // Note. プログレスバーの表示はわずかに硬直時間があるようなので実際に処理する場合だけ表示
            float progress = (float)count / totalCount;
            EditorUtility.DisplayProgressBar(ProgressBarTitle, $"{assetPath} ({count} / {totalCount})", progress);

            SetImportSettingsForIos(textureImporter, assetPath);
            SetImportSettingsForAndroid(textureImporter, assetPath);
            AssetDatabase.ImportAsset(assetPath);
        }

        static bool ShouldReimport(TextureImporter textureImporter)
        {
            var iosSettings = textureImporter.GetPlatformTextureSettings("iPhone");
            if (iosSettings.format != IosTextureFormat || iosSettings.overridden == false) { return true; }

            var androidSettings = textureImporter.GetPlatformTextureSettings("Android");
            if (androidSettings.format != AndroidTextureFormat || androidSettings.overridden == false) { return true; }

            return false;
        }
    }
}
