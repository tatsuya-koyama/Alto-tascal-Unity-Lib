using System.Collections.Generic;
using System.Text;
using UnityEditorInternal;

namespace AltoLib
{
    /// <summary>
    /// layer 名一覧を定数化したクラスのコードを生成する。
    /// <example><code>
    /// var generator = new LayerNameCodeGenerator();
    /// generator.outputDirPath = "OutDirPath";
    /// generator.namespaceName = "YourNamespace";
    /// generator.Generate();
    /// </code></example>
    /// </summary>
    public class LayerNameCodeGenerator : CodeGenerator
    {
        public override string outputFileName { get; set; } = "LayerName.gen.cs";
        public override string className      { get; set; } = "LayerName";

        protected override void WriteInner(StringBuilder builder)
        {
            var layers = InternalEditorUtility.layers;
            var labelSet = new HashSet<string>(layers);
            AppendSymbols(builder, labelSet);
        }
    }
}
