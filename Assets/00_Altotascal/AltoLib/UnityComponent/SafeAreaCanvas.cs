using UnityEngine;

namespace AltoLib
{
    public class SafeAreaCanvas : MonoBehaviour
    {
        RectTransform _rect;
        Rect _lastSafeArea = new Rect(0, 0, 0, 0);

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
