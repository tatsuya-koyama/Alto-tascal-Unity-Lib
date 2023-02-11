using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class CameraWalk : MonoBehaviour
{
    [SerializeField] float _moveSpeed = 5.0f;
    [SerializeField] float _mouseSensitivity = 400.0f;
    [SerializeField] float _cameraTiltSpeed = 10.0f;
    [SerializeField] float _moveLerpFactor = 0.1f;
    [SerializeField] float _rotateLerpFactor = 0.1f;

    Transform _cameraTransform;
    Vector3 _targetPos;
    Quaternion _targetRotation;
    float _targetEulerZ = 0f;
    Vector3 _baseMousePos;
    Vector3 _baseCameraAngle;
    Vector3 _baseCameraPos;

    void Start()
    {
        Cursor.visible = false;
        _cameraTransform = this.gameObject.transform;
        _targetPos       = _cameraTransform.position;
        _targetRotation  = _cameraTransform.transform.rotation;
    }

    void Update()
    {
        MoveCameraByKey();
        TiltCameraByKey();
        RotateCameraByMouse();

        _cameraTransform.position = Vector3.Lerp(_cameraTransform.position, _targetPos, _moveLerpFactor);
        _cameraTransform.rotation = Quaternion.Lerp(
            _cameraTransform.rotation, _targetRotation, _rotateLerpFactor
        );
    }

    void MoveCameraByKey()
    {
        Vector3 pos = _targetPos;
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        float factor = Time.deltaTime * _moveSpeed;

        if (v != 0) { pos += _cameraTransform.forward * v * factor; }
        if (h != 0) { pos += _cameraTransform.right   * h * factor; }
        if (Input.GetKey(KeyCode.E)) { pos += _cameraTransform.up * 0.5f * factor; }
        if (Input.GetKey(KeyCode.Q)) { pos -= _cameraTransform.up * 0.5f * factor; }

        _targetPos = pos;
    }

    void TiltCameraByKey()
    {
        bool moved = false;
        if (Input.GetKey(KeyCode.F)) { _targetEulerZ += _cameraTiltSpeed * Time.deltaTime; moved = true; }
        if (Input.GetKey(KeyCode.R)) { _targetEulerZ -= _cameraTiltSpeed * Time.deltaTime; moved = true; }

        if (moved) {
            Vector3 euler = _cameraTransform.transform.eulerAngles;
            _targetRotation = Quaternion.Euler(euler.x, euler.y, _targetEulerZ);
        }
    }

    void RotateCameraByMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _baseMousePos = Input.mousePosition;
            _baseCameraAngle = _cameraTransform.transform.eulerAngles;
        }

        if (Input.GetMouseButton(0))
        {
            float dx = (_baseMousePos.x - Input.mousePosition.x) / Screen.width;
            float dy = (_baseMousePos.y - Input.mousePosition.y) / Screen.height;

            float eulerX = _baseCameraAngle.x + (dy * _mouseSensitivity);
            float eulerY = _baseCameraAngle.y - (dx * _mouseSensitivity);

            _targetRotation = Quaternion.Euler(eulerX, eulerY, _targetEulerZ);
        }
    }
}
