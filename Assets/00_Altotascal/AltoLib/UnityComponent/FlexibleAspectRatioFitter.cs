using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AltoLib
{
    /// <summary>
    /// RectTransform をある程度幅を持たせた範囲のアスペクト比に自動調整する。
    /// （AspectRatioFitter の FitInParent モードの、幅を持たせた版）
    ///
    /// 【要件の例】
    /// ・横持ちのゲームで、縦幅は iPad の 4 : 3 まで対応するがそれ以降は広がらないようにしたい
    /// ・横幅は近年の iPhone の 19.5 : 9 まで対応するがそれ以降は広がらないようにしたい
    /// ・Canvas の Reference Resolution は 1600 x 900 (16 : 9)
    ///
    /// 【使い方】
    /// ・上記の要件の場合で、Canvas の Scale Mode は Scale With Screen Size の Expand モードを指定
    /// ・Canvas 配下に画面全体に広げたオブジェクトを配置し、そのオブジェクトに本コンポーネントをアタッチ
    ///   ※ 画面全体に広げた = RectTransfrom の Anchor が (0,0) 〜 (1,1) になっている状態
    /// ・baseResolution に 1600, 900 を入力（Canvas の Reference Resolution と同じものを入れる）
    /// ・widestResolution に最も横長の場合の解像度として 1600, 1200 を入力（4 : 3）
    /// ・tallestResolution に最も縦長の場合の解像度として 1950, 900 を入力（19.5 : 9）
    ///
    /// 【Hierarchy 例】
    /// - ScreenCanvas
    ///     - EntireDisplay  // Canvas 全体に広げた RectTransform = ディスプレイ領域全体
    ///     - AspectFitter_InEntireScreen  ... ここに本コンポーネントをアタッチ
    ///         - （アスペクト比が制御されたゲーム画面領域）
    ///     - SafeArea  ... AltoLib.SafeAreaCanvas コンポーネントをアタッチ
    ///         - AspectFitter_InSafeArea  ... ここに本コンポーネントをアタッチ（entireScreenRect に EntireDisplay をセット）
    ///             - （セーフエリア内に収めた UI 領域）
    /// - OverlayCanvas
    ///     - AspectFitter_InEntireScreen
    ///         - （外側に画面を覆い隠す帯を配置）
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public class FlexibleAspectRatioFitter : UIBehaviour, ILayoutSelfController
    {
        [SerializeField] Vector2 baseResolution = default;
        [SerializeField] Vector2 widestResolution = default;
        [SerializeField] Vector2 tallestResolution = default;

        [Header("ぴったり収まるように scale 自動調整")]
        [SerializeField] bool autoExpandMode = true;

        [Header("[SafeArea 内で使用する場合] 画面全体 Rect")]
        [SerializeField] RectTransform entireScreenRect = default;

        RectTransform _rect;
        RectTransform MyRect
        {
            get
            {
                if (_rect == null) { _rect = GetComponent<RectTransform>(); }
                return _rect;
            }
        }

        //--------------------------------------------------------------------------
        // UIBehaviour
        //--------------------------------------------------------------------------

        protected override void OnRectTransformDimensionsChange()
        {
            UpdateRect();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateRect();
        }

        protected override void OnDisable()
        {
            LayoutRebuilder.MarkLayoutForRebuild(MyRect);
            base.OnDisable();
        }

        //--------------------------------------------------------------------------
        // ILayoutSelfController
        //--------------------------------------------------------------------------

        public virtual void SetLayoutHorizontal() { }
        public virtual void SetLayoutVertical() { }

        //--------------------------------------------------------------------------
        // Main logic
        //--------------------------------------------------------------------------

        void UpdateRect()
        {
            if (!IsActive()) { return; }

            MyRect.anchorMin = Vector2.zero;
            MyRect.anchorMax = Vector2.one;
            MyRect.anchoredPosition = Vector2.zero;

            if (entireScreenRect != null)
            {
                UpdateRect_InSafeArea();
                return;
            }

            Vector2 parentSize = GetParentSize();
            (MyRect.sizeDelta, MyRect.localScale) = CalcFittedSize(parentSize);
        }

        /// <summary>
        /// セーフエリア配下で使用する場合の処理。
        /// 画面全体に対してアスペクト比調整をした領域 x セーフエリア領域の
        /// 両方に収まるように領域を調整
        /// </summary>
        void UpdateRect_InSafeArea()
        {
            RectTransform safeAreaRectT = MyRect.parent as RectTransform;
            Rect safeAreaRect = new Rect(safeAreaRectT.rect);

            var (parentSizeDelta, parentScale) = CalcFittedSize(entireScreenRect.rect.size);
            Rect parentRect = new Rect(entireScreenRect.rect);
            parentRect.size = entireScreenRect.rect.size + parentSizeDelta;
            parentRect.position = parentRect.size * -0.5f;
            parentRect.position += safeAreaRectT.anchoredPosition;

            Rect overlapRect = RectUtil.GetOverlapRect(parentRect, safeAreaRect);

            Vector2 expandedParentSize = GetExpandedParentSize(overlapRect.size);
            Vector2 targetSize = CalcTargetSize(expandedParentSize);
            Vector2 sizeDelta = CalcSizeDelta(safeAreaRect.size, targetSize);
            Vector3 localScale = Vector3.one * CalcRectScale(safeAreaRect.size, targetSize);
            MyRect.sizeDelta = sizeDelta;
            MyRect.localScale = localScale;

            Vector2 dMin = overlapRect.min - safeAreaRectT.rect.min;
            Vector2 dMax = overlapRect.max - safeAreaRectT.rect.max;
            MyRect.anchoredPosition = (dMin + dMax) * -0.5f;
        }

        Vector2 GetParentSize()
        {
            RectTransform parent = MyRect.parent as RectTransform;
            if (!parent) { return Vector2.zero; }
            return parent.rect.size;
        }

        (Vector2 sizeDelta, Vector3 localScale) CalcFittedSize(Vector2 parentSize)
        {
            Vector2 expandedParentSize = GetExpandedParentSize(parentSize);
            Vector2 targetSize = CalcTargetSize(expandedParentSize);
            Vector2 sizeDelta = CalcSizeDelta(parentSize, targetSize);
            Vector3 localScale = Vector3.one * CalcRectScale(parentSize, targetSize);
            return (sizeDelta, localScale);
        }

        /// <summary>
        /// 自身の補正後サイズの計算に使う用の、Canvas の Expand 挙動を再現した親サイズを取得
        /// （エディタの Prefab モードでも再生時の挙動を再現するための処理）
        /// </summary>
        Vector2 GetExpandedParentSize(Vector2 parentSize)
        {
            float minRatio = Mathf.Min(parentSize.x / baseResolution.x, parentSize.y / baseResolution.y);
            return parentSize / minRatio;
        }

        Vector2 CalcTargetSize(Vector2 parentSize)
        {
            Vector2 targetSize = baseResolution;
            if (targetSize.x < parentSize.x)
            {
                targetSize.x = Mathf.Min(parentSize.x, widestResolution.x);
            }
            if (targetSize.y < parentSize.y)
            {
                targetSize.y = Mathf.Min(parentSize.y, tallestResolution.y);
            }
            return targetSize;
        }

        Vector2 CalcSizeDelta(Vector2 parentSize, Vector2 targetSize)
        {
            return new Vector2(
                (targetSize.x - parentSize.x),
                (targetSize.y - parentSize.y)
            );
        }

        /// <summary>
        /// 親の Rect にぴったり収まらない（自身が大きくてはみ出す or 自身が小さくて余白がある）
        /// 場合に、ぴったり収まるような scale を求める。Canvas の Expand 的挙動。
        /// この処理が必要になるのは以下のケース：
        ///
        /// ・もともとぴったり収まっていたところに、セーフエリアで領域が狭まった
        /// ・エディタの Prefab モード（Game View の画角に影響を受ける）で
        ///   Game View の解像度が想定される Canvas の Reference Resolution より大きい
        /// </summary>
        float CalcRectScale(Vector2 parentSize, Vector2 targetSize)
        {
            if (!autoExpandMode) { return 1f; }

            Vector2 ratio = targetSize / parentSize;
            return (ratio.x > ratio.y) ? (1 / ratio.x) : (1 / ratio.y);
        }

        //--------------------------------------------------------------------------
        // カスタムエディタ
        //--------------------------------------------------------------------------

        #if UNITY_EDITOR
        [CustomEditor(typeof(FlexibleAspectRatioFitter))]
        public class FieldFlowerEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                EditorGUILayout.Space();
                DebugInfo();
            }

            void DebugInfo()
            {
                var t = target as FlexibleAspectRatioFitter;
                Vector2 originalParentSize = t.GetParentSize();
                Vector2 parentSize = t.GetExpandedParentSize(originalParentSize);
                Vector2 targetSize = t.CalcTargetSize(parentSize);
                float aspect_portrait_9   = targetSize.y / targetSize.x * 9;
                float aspect_portrait_16  = targetSize.y / targetSize.x * 16;
                float aspect_landscape_9  = targetSize.x / targetSize.y * 9;
                float aspect_landscape_16 = targetSize.x / targetSize.y * 16;
                EditorGUILayout.HelpBox(
                    $"親 Rect のオリジナルサイズ : {originalParentSize}\n" +
                    $"親 Rect の Expand 後サイズ : {parentSize}\n" +
                    $"自身の補正後サイズ : {targetSize}\n" +
                    $"自身の実際のサイズ : {t.MyRect.rect.size}\n" +
                    $"自身の親に対する比率 : {targetSize / parentSize}\n" +
                    $"親の自身に対する比率 : {parentSize / targetSize}\n" +
                    $"----------\n" +
                    $"自身の Aspect :\n" +
                    $"　　 9 : {aspect_portrait_9:F2}\n" +
                    $"　　16 : {aspect_portrait_16:F2}\n" +
                    $"　　{aspect_landscape_9:F2} : 9\n" +
                    $"　　{aspect_landscape_16:F2} : 16",
                    MessageType.None
                );
            }
        }
        #endif

    }
}
