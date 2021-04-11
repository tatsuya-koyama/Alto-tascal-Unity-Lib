using System.Collections.Generic;
using System.Text;

namespace AltoLib
{
    /// <summary>
    /// Addressable Asset のラベル一覧を定数化したクラスのコードを生成する。
    /// <example><code>
    /// var generator = new AssetLabelCodeGenerator();
    /// generator.outputDirPath = "OutDirPath";
    /// generator.namespaceName = "YourNamespace";
    /// generator.Generate();
    /// </code></example>
    /// </summary>
    public class AssetLabelCodeGenerator : CodeGenerator
    {
        public virtual string assetDirPath { get; set; } = "Assets/AddressableAssetsData/AssetGroups";

        public override string outputFileName { get; set; } = "AssetLabel.gen.cs";
        public override string className      { get; set; } = "AssetLabel";

        protected override void WriteInner(StringBuilder builder)
        {
            var labelSet    = new HashSet<string>();
            var assetGroups = EditorFileUtil.LoadAssetGroups(assetDirPath);
            foreach (var group in assetGroups)
            {
                // Ignore Built-in resources
                if (group.ReadOnly) { continue; }

                foreach (var entry in group.entries)
                {
                    foreach (string label in entry.labels)
                    {
                        labelSet.Add(label);
                    }
                }
            }
            AppendSymbols(builder, labelSet);
        }
    }
}
