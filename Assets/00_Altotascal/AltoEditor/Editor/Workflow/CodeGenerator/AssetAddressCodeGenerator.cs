using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AltoEditor
{
    /// <summary>
    /// Addressable Asset のアドレス一覧を定数化したクラスのコードを生成する。
    /// <example><code>
    /// var generator = new AssetAddressCodeGenerator();
    /// generator.outputDirPath = "OutDirPath";
    /// generator.namespaceName = "YourNamespace";
    /// generator.Generate();
    /// </code></example>
    /// </summary>
    public class AssetAddressCodeGenerator : CodeGenerator
    {
        public virtual string assetDirPath { get; set; } = "Assets/AddressableAssetsData/AssetGroups";

        public override string outputFileName { get; set; } = "AssetAddress.gen.cs";
        public override string className      { get; set; } = "AssetAddress";

        protected override void WriteInner(StringBuilder builder)
        {
            var addresseSet = new HashSet<string>();
            var assetGroups = EditorFileUtil.LoadAssetGroups(assetDirPath);
            foreach (var group in assetGroups)
            {
                // Ignore Built-in resources
                if (group.ReadOnly) { continue; }

                foreach (var entry in group.entries)
                {
                    bool isNew = addresseSet.Add(entry.address);
                    if (!isNew)
                    {
                        Debug.LogWarning($"Duplicated address found : { entry.address }");
                    }
                }
            }
            AppendSymbols(builder, addresseSet);
        }
    }
}
