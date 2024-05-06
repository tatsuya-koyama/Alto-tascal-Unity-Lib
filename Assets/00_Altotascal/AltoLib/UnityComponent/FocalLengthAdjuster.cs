using UnityEngine;

namespace AltoLib
{
    // Unity のカメラは上下の視野が固定で、画面が横に広くなると左右の視野が広くなる。
    // （カメラの Field of View は Vertical FOV の意味）
    // ForcalLengthAdjuster はどのアスペクト比でも一定の視野がカメラに収まるように焦点距離を調整する。
    // （被写体の横幅がちょうどカメラに収まるような位置にカメラ位置を前後させる。FOV は一定に保つ）
    public class FocalLengthAdjuster : MonoBehaviour
    {
        [SerializeField] float aspectThresholdV = 9f;
        [SerializeField] float aspectThresholdH = 16.0f;
        [SerializeField] float subjectWidth = 20.0f;
        [SerializeField] Vector3 subjectPos = new Vector3(0, 0, 0);

        Camera _camera;
        Camera _mainCamera;
        float  _lastAspect = 0f;

        void Start()
        {
            _camera = GetComponent<Camera>();
            _mainCamera = Camera.main;
        }

        void Update()
        {
            AdjustCameraDistance();
        }

        void AdjustCameraDistance()
        {
            if (_lastAspect == _mainCamera.aspect)
            {
                return;
            }
            _lastAspect = _mainCamera.aspect;

            float cameraDistance = GetCameraDistanceToFit(_camera, _mainCamera, subjectWidth);
            Vector3 subjectToCamera = Vector3.Normalize(_camera.transform.position - subjectPos) * cameraDistance;
            _camera.transform.position = subjectPos + subjectToCamera;
        }

        float GetCameraDistanceToFit(Camera targetCamera, Camera mainCamera, float subjectWidth) {
            if (targetCamera == null || mainCamera == null || subjectWidth <= 0.0f)
            {
                Debug.LogError("[FovAdjuster] Invalid parameter.");
                return 0.0f;
            }

            float aspect = Mathf.Min(mainCamera.aspect, aspectThresholdV / aspectThresholdH);
            float frustumHeight = subjectWidth / aspect;
            return frustumHeight * 0.5f / Mathf.Tan(targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        }
    }
}
