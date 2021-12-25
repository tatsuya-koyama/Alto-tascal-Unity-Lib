using UnityEngine;

namespace AltoLib
{
    public interface IAltoThirdPersonCamera
    {
        void MoveAngle(float moveX, float moveY);
        void MoveDistance(float moveZ);
        void LookOther(Vector3 otherPos);
        void StopLookOther();
    }

    public class AltoThirdPersonCamera : MonoBehaviour, IAltoThirdPersonCamera
    {
        [SerializeField] public GameObject target = null;
        [SerializeField] public Vector3 lookPosOffset = Vector3.zero;
        [SerializeField] public float distance = 7.0f;        // Distance to follow object
        [SerializeField] public float polarAngle = 60.0f;     // 極角。太陽の傾き的な角度
        [SerializeField] public float azimuthAngle = 235.0f;  // 方位角
        [SerializeField] public float minDistance = 2.5f;
        [SerializeField] public float maxDistance = 100.0f;
        [SerializeField] public float minPolarAngle = 5.0f;
        [SerializeField] public float maxPolarAngle = 85.0f;
        [SerializeField] public float maxCameraY = -99f;
        [SerializeField] public bool smoothFollow = true;
        [SerializeField] public float followSpeed = 10f;
        [SerializeField] public Vector2 angleMoveSensitivity = new Vector2(2.0f, 2.0f);
        [SerializeField] public float angleMoveAttenuation = 4.8f;
        [SerializeField] public float distanceMoveSensitivity = 0.1f;
        [SerializeField] public float distanceMoveAttenuation = 4.8f;

        [SerializeField] public bool detectWall = true;
        [SerializeField] public int detectWallLayerMask = -1;
        [SerializeField] public float detectWallRayLength = 3.0f;
        [SerializeField] public float detectWallCorrectSpeed = 3f;
        [SerializeField] public float detectWallYOffset = 1.2f;

        [SerializeField] public bool autoZoom = true;
        [SerializeField] public float autoZoomRange = 60f;
        [SerializeField] public float autoZoomDistance = 5f;
        [SerializeField] public Vector3 autoZoomLookPosOffset = new Vector3(0, 2f, 0);

        [SerializeField] public bool drawDebug = true;

        Vector3 _currentIdealPos;
        Vector3 _currentLookPos;
        Vector2 _currentAngleMove;
        float _currentDistanceMove;

        // 操作対象以外を見る制御用
        Vector3 _otherLookPos;
        bool _lookingOtherPos = false;

        Vector2 _inputAngleMove;
        float _inputDistanceMove;

        // 壁際カメラ制御用
        Vector3 _forcePos = Vector3.zero;
        bool _isPosForced = false;  // true 時、強制的に _forcePos の位置に移動
        float _forcePosLerp = 0f;

        float _distanceOffset = 0f;
        Vector3 _autoZoomPosOffset = Vector3.zero;

        //----------------------------------------------------------------------
        // IAltoThirdPersonCamera
        //----------------------------------------------------------------------

        public void MoveAngle(float moveX, float moveY)
        {
            _inputAngleMove.x = moveX;
            _inputAngleMove.y = moveY;
        }

        public void MoveDistance(float moveZ)
        {
            _inputDistanceMove = moveZ;
        }

        public void LookOther(Vector3 otherPos)
        {
            _otherLookPos = otherPos;
            _lookingOtherPos = true;
        }

        public void StopLookOther()
        {
            _lookingOtherPos = false;
        }

        //----------------------------------------------------------------------
        // MonoBehaviour
        //----------------------------------------------------------------------

        void Start()
        {
            _currentLookPos = target.transform.position + lookPosOffset;
        }

        void LateUpdate()
        {
            UpdateLookPos();
            UpdateAngle();
            AutoZoom();
            UpdateDistance();
            DetectWall();
            UpdateCameraPos();
        }

        //----------------------------------------------------------------------
        // private
        //----------------------------------------------------------------------

        void UpdateLookPos()
        {
            Vector3 lookPos = target.transform.position + lookPosOffset;
            if (_lookingOtherPos) { lookPos = _otherLookPos; }

            if (smoothFollow)
            {
                _currentLookPos += (lookPos - _currentLookPos) * followSpeed * dt();
            }
            else
            {
                _currentLookPos = lookPos;
            }
        }

        void UpdateAngle()
        {
            float atten = 1f + (angleMoveAttenuation * dt());
            _currentAngleMove.x = (_currentAngleMove.x + (_inputAngleMove.x * 60 * dt())) / atten;
            _currentAngleMove.y = (_currentAngleMove.y + (_inputAngleMove.y * 60 * dt())) / atten;
            _inputAngleMove = Vector2.zero;

            float angleX = azimuthAngle - (_currentAngleMove.x * angleMoveSensitivity.x);
            azimuthAngle = Mathf.Repeat(angleX, 360f);

            float angleY = polarAngle + (_currentAngleMove.y * angleMoveSensitivity.y);
            polarAngle = Mathf.Clamp(angleY, minPolarAngle, maxPolarAngle);
        }

        void UpdateDistance()
        {
            float atten = 1f + (distanceMoveAttenuation * dt());
            _currentDistanceMove = (_currentDistanceMove + (_inputDistanceMove * 60 * dt())) / atten;
            _inputDistanceMove = 0;
            float d = distance - (_currentDistanceMove * distanceMoveSensitivity);
            distance = Mathf.Clamp(d, minDistance, maxDistance);
        }

        void AutoZoom()
        {
            if (!autoZoom)
            {
                _distanceOffset = 0f;
                _autoZoomPosOffset = Vector3.zero;
                return;
            }

            float zoomFactor = (polarAngle - (90f - autoZoomRange)) / autoZoomRange;
            _distanceOffset = Mathf.Clamp(zoomFactor, 0f, 1f) * -autoZoomDistance;
            _autoZoomPosOffset = Vector3.Lerp(Vector3.zero, autoZoomLookPosOffset, zoomFactor);
        }

        void UpdateCameraPos()
        {
            float finalDistance = Mathf.Clamp(distance + _distanceOffset, minDistance, maxDistance);
            _currentIdealPos = NewPos(finalDistance, _currentLookPos);

            // 先にカメラの向きを計算
            Vector3 lookPos = _currentLookPos + _autoZoomPosOffset;
            transform.position = _currentIdealPos;
            transform.LookAt(lookPos);

            // 壁際にいる場合は向きを固定したままカメラ位置を補正
            if (_isPosForced)
            {
                _forcePosLerp += (1f - _forcePosLerp) * detectWallCorrectSpeed * dt();
            }
            else
            {
                _forcePosLerp += (0f - _forcePosLerp) * detectWallCorrectSpeed * dt();
            }
            _forcePosLerp = Mathf.Clamp(_forcePosLerp, 0f, 1f);
            transform.position = Vector3.Lerp(_currentIdealPos, _forcePos, _forcePosLerp);

            // 指定の y 座標より下には下がらないようにする
            // （ターゲット落下時に見下ろすような処理を行う時用）
            if (transform.position.y < maxCameraY) {
                Vector3 pos = transform.position;
                pos.y = maxCameraY;
                transform.position = pos;
            }
        }

        void DetectWall()
        {
            _isPosForced = false;
            if (!detectWall) { return; }

            Vector3 fromPos = target.transform.position + lookPosOffset + _autoZoomPosOffset;
            fromPos.y -= detectWallYOffset;
            Vector3 lookDir = fromPos - _currentIdealPos;
            lookDir = fromPos - _currentIdealPos;
            lookDir.Normalize();

            float idealDistance = Vector3.Distance(_currentIdealPos, fromPos);
            float rayLength = Mathf.Min(idealDistance, detectWallRayLength);
            Vector3 toPos = fromPos - (lookDir * rayLength);

            // デバッグ表示
            if (drawDebug)
            {
                Vector3 toPosBottom = toPos;
                toPosBottom.y = fromPos.y;
                Debug.DrawLine(fromPos, toPos, Color.magenta);
                Debug.DrawLine(toPos, toPosBottom, Color.red);
                Debug.DrawLine(toPosBottom, fromPos, Color.red);
            }

            RaycastHit hitWall;
            if (Physics.Linecast(fromPos, toPos, out hitWall, detectWallLayerMask))
            {
                _forcePos = hitWall.point;
                _forcePos.y += detectWallYOffset;
                _isPosForced = true;

                if (drawDebug)
                {
                    DebugDrawUtil.DrawDiamond3D(hitWall.point, 0.2f, Color.yellow);
                }
            }
        }

        Vector3 NewPos(float newDistance, Vector3 lookPos)
        {
            var da = azimuthAngle * Mathf.Deg2Rad;
            var dp = polarAngle * Mathf.Deg2Rad;
            return new Vector3(
                lookPos.x + newDistance * Mathf.Sin(dp) * Mathf.Cos(da),
                lookPos.y + newDistance * Mathf.Cos(dp),
                lookPos.z + newDistance * Mathf.Sin(dp) * Mathf.Sin(da)
            );
        }

        float dt()
        {
            return Mathf.Clamp(Time.deltaTime, 1 / 60f, 1 / 15f);
        }
    }
}
