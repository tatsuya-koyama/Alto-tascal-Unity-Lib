using UnityEngine;

namespace AltoLib
{
    /// <summary>
    /// スクリーン全体に対する Safe Area サイズの比率を RectTransform の scale に反映する。
    /// SafeAreaCanvas コンポーネントをつけたオブジェクトの子オブジェクトを
    /// Safe Area にフィットさせる用途に使用
    /// </summary>
    public class SafeAreaFitter : MonoBehaviour
    {
        RectTransform _rect;
        Rect _lastSafeArea = new Rect(0, 0, 0, 0);

        void Awake()
        {
            _rect = GetComponent<RectTransform>();
        }

        void LateUpdate()
        {
            UpdateSafeArea();
        }

        void UpdateSafeArea()
        {
            Rect safeArea = Screen.safeArea;
            if (safeArea == _lastSafeArea) { return; }

            _lastSafeArea = safeArea;

            bool isShortSideVertical = (safeArea.size.y < safeArea.size.x);
            float scale = isShortSideVertical ? (safeArea.size.y / Screen.height)
                                              : (safeArea.size.x / Screen.width);
            _rect.localScale = Vector3.one * scale;
        }
    }
}
