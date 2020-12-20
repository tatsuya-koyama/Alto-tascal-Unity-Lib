using UnityEngine;

namespace AltoLib
{
    // 被写体の横幅がちょうどカメラに収まるよう、カメラの FOV を自動調整する
    public class FovAdjuster : MonoBehaviour
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
            AdjustCameraFOV();
        }

        void AdjustCameraFOV()
        {
            if (_lastAspect == _mainCamera.aspect)
            {
                return;
            }
            _lastAspect = _mainCamera.aspect;

            _camera.fieldOfView = GetCameraFOVToFit(_camera, _mainCamera, subjectWidth);
        }

        float GetCameraFOVToFit(Camera targetCamera, Camera mainCamera, float subjectWidth) {
            if (targetCamera == null || mainCamera == null || subjectWidth <= 0.0f)
            {
                Debug.LogError("[FovAdjuster] Invalid parameter.");
                return 179;  // max value of field of view
            }

            float aspect = Mathf.Min(mainCamera.aspect, aspectThresholdV / aspectThresholdH);
            float frustumHeight = subjectWidth / aspect;
            float distance = Vector3.Distance(targetCamera.transform.position, subjectPos);
            return 2.0f * Mathf.Atan(frustumHeight * 0.5f / distance) * Mathf.Rad2Deg;
        }
    }
}
