using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;

namespace AltoLib
{
    /// <summary>
    /// ビルドに含まれるシーン名一覧を定数化したクラスのコードを生成する。
    /// <example><code>
    /// var generator = new SceneNameCodeGenerator();
    /// generator.outputDirPath = "OutDirPath";
    /// generator.namespaceName = "YourNamespace";
    /// generator.Generate();
    /// </code></example>
    /// </summary>
    public class SceneNameCodeGenerator : CodeGenerator
    {
        public override string outputFileName { get; set; } = "SceneName.gen.cs";
        public override string className { get; set; } = "SceneName";

        protected override void WriteInner(StringBuilder builder)
        {
            var sceneNameSet = new HashSet<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                string sceneName = Path.GetFileNameWithoutExtension(scene.path);
                sceneNameSet.Add(sceneName);
            }
            AppendSymbols(builder, sceneNameSet);
        }
    }
}
