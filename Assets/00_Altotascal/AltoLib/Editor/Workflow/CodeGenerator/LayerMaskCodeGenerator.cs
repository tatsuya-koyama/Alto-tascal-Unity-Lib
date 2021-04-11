using System.Collections.Generic;
using System.Text;
using UnityEditorInternal;
using UnityEngine;

namespace AltoLib
{
    /// <summary>
    /// layer のマスク値一覧を定数化したクラスのコードを生成する。
    /// <example><code>
    /// var generator = new LayerMaskCodeGenerator();
    /// generator.outputDirPath = "OutDirPath";
    /// generator.namespaceName = "YourNamespace";
    /// generator.Generate();
    /// </code></example>
    /// </summary>
    public class LayerMaskCodeGenerator : CodeGenerator
    {
        public override string outputFileName { get; set; } = "LayerMask.gen.cs";
        public override string className      { get; set; } = "LayerMask";

        protected override void WriteInner(StringBuilder builder)
        {
            var layers = InternalEditorUtility.layers;
            var layerToMask = new Dictionary<string, int>();
            foreach (string layer in layers)
            {
                int layerMask = 1 << LayerMask.NameToLayer(layer);
                layerToMask.Add(layer, layerMask);
            }
            AppendSymbols(builder, layerToMask);
        }
    }
}
