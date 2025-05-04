using UnityEngine;

namespace AltoLib
{
    public class SafeAreaCanvas : MonoBehaviour
    {
        [SerializeField] Canvas canvas = default;
        [SerializeField] bool keepAnchorMinMax = true;

        RectTransform _rect;
        Rect _lastSafeArea = new Rect(0, 0, 0, 0);
        float _lastCanvasScaleFactor = 0f;

        void Awake()
        {
            _rect = GetComponent<RectTransform>();
            UpdateSafeArea();
        }

        void Update()
        {
            UpdateSafeArea();
        }

        void UpdateSafeArea()
        {
            if (keepAnchorMinMax) {
                UpdateSafeArea_WithOffset();
            } else {
                UpdateSafeArea_WithAnchor();
            }
        }

        /// <summary>
        /// AnchorMin / Max を変えずに変更する版
        /// </summary>
        void UpdateSafeArea_WithOffset()
        {
            Rect safeArea = Screen.safeArea;
            if (safeArea == _lastSafeArea &&
                canvas.scaleFactor == _lastCanvasScaleFactor)
            {
                return;
            }

            _lastSafeArea = safeArea;
            _lastCanvasScaleFactor = canvas.scaleFactor;

            Vector2 safeAreaMin = safeArea.position;
            Vector2 safeAreaMax = safeArea.position + safeArea.size;
            _rect.offsetMin = safeAreaMin / canvas.scaleFactor;
            _rect.offsetMax = -1f * (new Vector2(Screen.width, Screen.height) - safeAreaMax) / canvas.scaleFactor;
        }

        void UpdateSafeArea_WithAnchor()
        {
            Rect safeArea = Screen.safeArea;
            if (safeArea == _lastSafeArea) { return; }

            _lastSafeArea = safeArea;

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;
            _rect.anchorMin = anchorMin;
            _rect.anchorMax = anchorMax;
        }
    }
}
