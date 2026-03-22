using UnityEngine;
using UnityEngine.Rendering;

namespace AltoLib.Rendering
{
    [VolumeComponentMenu("Alto/Screen Space Reflections")]
    public class SSR_VolumeComponent : VolumeComponent, IPostProcessComponent
    {
        public bool IsActive() => intensity.value > 0f;
        public bool IsTileCompatible() => false;

        [Header("General")]

        [Tooltip("SSR エフェクトの強さ。0 で無効")]
        public ClampedFloatParameter intensity = new(0f, 0f, 1f);

        [Header("Ray Marching")]

        [Tooltip("レイマーチングの最大ステップ数。大きいほど高品質だが重い")]
        public ClampedIntParameter maxSteps = new(32, 8, 128);

        [Tooltip("1 ステップで進める距離（ワールド空間上の長さ）")]
        public ClampedFloatParameter stepSize = new(0.1f, 0.01f, 1.0f);

        [Tooltip("レイが到達できる最大距離（ワールド空間上の長さ）")]
        public ClampedFloatParameter maxDistance = new(50f, 1f, 200f);

        [Tooltip("深度比較の厚みしきい値。大きいほど反射が発生しやすいがアーティファクトも増える")]
        public ClampedFloatParameter thickness = new(0.5f, 0.01f, 5f);

        [Header("Quality")]

        [Tooltip("ヒット後のバイナリサーチ精緻化ステップ数")]
        public ClampedIntParameter binarySearchSteps = new(4, 0, 16);

        [Tooltip("レンダリング解像度スケール。0.5 で半分 = 4 分の 1 の負荷")]
        public ClampedFloatParameter resolutionScale = new(0.5f, 0.05f, 1f);

        [Header("Fading")]

        [Tooltip("画面端でのフェードアウト幅")]
        public ClampedFloatParameter edgeFade = new(0.1f, 0f, 1f);

        [Tooltip("レイの移動距離によるフェードアウトの強さ")]
        public ClampedFloatParameter distanceFade = new(0.5f, 0f, 1f);

        [Tooltip("反射面からの距離によるフェードアウト。この距離（ワールド空間上の長さ）を超えた反射は完全に消える。0 で無効")]
        public ClampedFloatParameter surfaceFadeDistance = new(0f, 0f, 200f);

        [Tooltip("反射面距離フェードのカーブ。1 でリニア、大きいほど急激に減衰")]
        public ClampedFloatParameter surfaceFadePower = new(2f, 0.5f, 5f);

        [Header("Smoothing (Blur)")]

        [Tooltip("反射結果のブラー半径。大きいほど滑らかだがディテールが失われる。0 でブラー処理無効")]
        public ClampedFloatParameter smoothness = new(1.5f, 0f, 5f);
    }
}
