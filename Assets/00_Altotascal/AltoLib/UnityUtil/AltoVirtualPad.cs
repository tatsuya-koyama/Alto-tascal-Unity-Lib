using UnityEngine;
using UnityEngine.EventSystems;

namespace AltoLib
{
    /// <summary>
    /// 画面の左半分と右半分で操作するバーチャルパッド
    /// </summary>
    public class AltoVirtualPad : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public class PointerInfo
        {
            public bool isLeft = true;
            public float sensitivity;
            public int pointerId;
            public bool isPressed = false;
            public Vector2 originPos = Vector2.zero;
            public Vector2 currentPos = Vector2.zero;

            public void Init(bool isLeft, float sensitivity)
            {
                this.isLeft = isLeft;
                this.sensitivity = sensitivity;
            }

            public float GetHorizontalInput()
            {
                if (!isPressed) { return 0; }

                float h = currentPos.x - originPos.x;
                float aspect = Screen.width / Screen.height;
                if (aspect > 1f) { h *= aspect; }

                return Mathf.Clamp(h * sensitivity, -1f, 1f);
            }

            public float GetVerticalInput()
            {
                if (!isPressed) { return 0; }

                float v = currentPos.y - originPos.y;
                float aspect = Screen.width / Screen.height;
                if (aspect > 1f) { v *= aspect; }

                return Mathf.Clamp(v * sensitivity, -1f, 1f);
            }
        }

        [SerializeField] public float leftSensitivity  = 16f;  // 左スティック感度
        [SerializeField] public float rightSensitivity = 16f;  // 右スティック感度
        [SerializeField] public float stickSlack = 0.8f;  // スティックの遊び。これを超えた距離指が動くと軸が追従

        PointerInfo _leftPointer  = new PointerInfo();
        PointerInfo _rightPointer = new PointerInfo();

        public PointerInfo leftPointer  => _leftPointer;
        public PointerInfo rightPointer => _rightPointer;

        void Awake()
        {
            _leftPointer.Init(true, leftSensitivity);
            _rightPointer.Init(false, rightSensitivity);
        }

        //----------------------------------------------------------------------
        // Touch handing
        //----------------------------------------------------------------------

        public void OnPointerDown(PointerEventData data)
        {
            Vector2 pos = NormalizedScreenPos(data.position);
            if (pos.x < 0.5)
            {
                if (_leftPointer.isPressed) { return; }
                InitPointer(_leftPointer, data.pointerId, pos);
            }
            else
            {
                if (_rightPointer.isPressed) { return; }
                InitPointer(_rightPointer, data.pointerId, pos);
            }
        }

        void InitPointer(PointerInfo pointerInfo, int pointerId, Vector2 pos)
        {
            pointerInfo.pointerId  = pointerId;
            pointerInfo.isPressed  = true;
            pointerInfo.originPos  = pos;
            pointerInfo.currentPos = pos;
        }

        public void OnDrag(PointerEventData data)
        {
            Vector2 pos = NormalizedScreenPos(data.position);
            if (data.pointerId == _leftPointer.pointerId)
            {
                _leftPointer.currentPos = pos;
                UpdateOriginPos(_leftPointer);
            }
            if (data.pointerId == _rightPointer.pointerId)
            {
                _rightPointer.currentPos = pos;
            }
        }

        public void OnPointerUp(PointerEventData data)
        {
            Vector2 pos = NormalizedScreenPos(data.position);
            if (data.pointerId == _leftPointer.pointerId)
            {
                _leftPointer.isPressed = false;
            }
            if (data.pointerId == _rightPointer.pointerId)
            {
                _rightPointer.isPressed = false;
            }
        }

        //----------------------------------------------------------------------
        // private
        //----------------------------------------------------------------------

        /// <summary>
        /// 0 〜 1 に正規化した画面内座標を返す。
        /// 左下が (0, 0) で右上が (1, 1)
        /// </summary>
        Vector2 NormalizedScreenPos(Vector2 inputPos)
        {
            float x = inputPos.x / Screen.width;
            float y = inputPos.y / Screen.height;
            return new Vector2(x, y);
        }

        /// <summary>
        /// 指が動きすぎたときに軸も追従するように動かす
        /// </summary>
        void UpdateOriginPos(PointerInfo pointer)
        {
            Vector2 vec = pointer.currentPos - pointer.originPos;
            if (vec.magnitude < stickSlack) { return; }

            vec.Normalize();
            Vector2 newVec = vec * stickSlack;
            pointer.originPos = pointer.currentPos - newVec;
        }
    }
}
