using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace AltoFramework.Production
{
    // Fade screen with UI Panel
    public class ScreenFader : MonoBehaviour
    {
        const int PanelSortOrder = 999;

        GameObject _canvasObj;
        Image      _panelImage;

        public void Init()
        {
            _canvasObj = MakeUIPanel();
            _canvasObj.SetActive(false);
        }

        public async UniTask FadeOut(float fadeTime = 0.3f)
        {
            _canvasObj.SetActive(true);
            var passedTime = 0.0f;
            while (passedTime < fadeTime)
            {
                SetPanelAlpha(passedTime / fadeTime);
                passedTime += Mathf.Min(Time.deltaTime, 1 / 30.0f);
                await UniTask.Yield();
            }
            SetPanelAlpha(1f);
        }

        public async UniTask FadeIn(float fadeTime = 0.3f)
        {
            var passedTime = 0.0f;
            while (passedTime < fadeTime)
            {
                SetPanelAlpha(1 - passedTime / fadeTime);
                passedTime += Mathf.Min(Time.deltaTime, 1 / 30.0f);
                await UniTask.Yield();
            }
            SetPanelAlpha(0);
            _canvasObj.SetActive(false);
        }

        public void SetColor(Color color)
        {
            _panelImage.color = color;
        }

        //----------------------------------------------------------------------
        // private
        //----------------------------------------------------------------------

        void SetPanelAlpha(float alpha)
        {
            var c = _panelImage.color;
            _panelImage.color = new Color(c.r, c.g, c.b, alpha);
        }

        GameObject MakeUIPanel()
        {
            GameObject canvasObj = new GameObject("FadeCanvas");
            GameObject.DontDestroyOnLoad(canvasObj);

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.sortingOrder = PanelSortOrder;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            GameObject panel = new GameObject("Panel");
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 1);

            Image image = panel.AddComponent<Image>();
            image.color = Color.black;
            _panelImage = image;
            panel.transform.SetParent(canvasObj.transform, false);
            return canvasObj;
        }
    }
}
