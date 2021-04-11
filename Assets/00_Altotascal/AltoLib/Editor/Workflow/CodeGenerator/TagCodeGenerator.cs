using System.Collections.Generic;
using System.Text;
using UnityEditorInternal;

namespace AltoLib
{
    /// <summary>
    /// tag 一覧を定数化したクラスのコードを生成する。
    /// <example><code>
    /// var generator = new TagCodeGenerator();
    /// generator.outputDirPath = "OutDirPath";
    /// generator.namespaceName = "YourNamespace";
    /// generator.Generate();
    /// </code></example>
    /// </summary>
    public class TagCodeGenerator : CodeGenerator
    {
        public override string outputFileName { get; set; } = "Tag.gen.cs";
        public override string className      { get; set; } = "Tag";

        protected override void WriteInner(StringBuilder builder)
        {
            var tags = InternalEditorUtility.tags;
            var labelSet = new HashSet<string>(tags);
            AppendSymbols(builder, labelSet);
        }
    }
}
