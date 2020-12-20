#if ALTO_DEV
// Define "ALTO_DEV" symbol to enable this preprocess script.
using UnityEditor;
using UnityEngine;

namespace AltoLib
{
    /// <summary>
    /// プロジェクトにテクスチャアセットを追加した際、
    /// iOS / Android 向け圧縮フォーマットを自動で設定するサンプルコード。
    /// 各テクスチャの初回 Import 時に実行される。
    ///
    /// フォーマットは TextureReimporter で指定したものが適用される。
    /// </summary>
    public class TexturePreprocess : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            // 初回 Import 時のみ処理を行う。
            // 以前は .meta ファイルの存在確認で判定できていたが、
            // Unity 2019.4 では Preprocess の段階で .meta ファイルが作られていた。
            // 初回は importSettingsMissing が True になるようなのでそちらで判定する。
            var textureImporter = (TextureImporter)assetImporter;
            if (!textureImporter.importSettingsMissing) { return; }

            Debug.Log($"New texture is detected : {assetPath}");
            if (textureImporter == null)
            {
                Debug.LogError($"TextureImporter not found : {assetPath}");
                return;
            }

            TextureReimporter.SetImportSettingsForIos(textureImporter, assetPath);
            TextureReimporter.SetImportSettingsForAndroid(textureImporter, assetPath);
        }
    }
}
#endif
