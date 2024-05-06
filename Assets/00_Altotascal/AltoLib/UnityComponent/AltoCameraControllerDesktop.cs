using UnityEngine;

namespace AltoLib
{
    public class AltoCameraControllerDesktop : MonoBehaviour
    {
        [SerializeField] Component cameraComponent = null;

        IAltoThirdPersonCamera _thirdPersonCamera = null;

        void Awake()
        {
            _thirdPersonCamera = cameraComponent.GetComponent<IAltoThirdPersonCamera>();
        }

        void Update()
        {
            UpdateAngleWithMouse();
            UpdateDistanceWithMouse();
        }

        void UpdateAngleWithMouse()
        {
            if (!Input.GetMouseButton(0)) { return; }

            float moveX = Input.GetAxis("Mouse X");
            float moveY = Input.GetAxis("Mouse Y");
            _thirdPersonCamera.MoveAngle(moveX, moveY);
        }

        void UpdateDistanceWithMouse()
        {
            float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
            _thirdPersonCamera.MoveDistance(mouseScroll);
        }
    }
}
