using UnityEngine.UI;

namespace AltoLib
{
    /// <summary>
    /// 描画を行わない RaycastTarget.
    /// 画像を調整せずにボタンのタッチ判定を広げるのに使える。（Image でやるより処理効率が良い）
    /// なお Unity 2020.1 からは RaycastPadding というオプションが導入されたので不要になる。
    ///
    /// 参考 : https://answers.unity.com/questions/1091618/ui-panel-without-image-component-as-raycast-target.html
    /// </summary>
    public class RaycastTargetWithoutDrawing : Graphic
    {
        public override void SetMaterialDirty() { return; }
        public override void SetVerticesDirty() { return; }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            return;
        }
    }
}
