using UnityEngine;

namespace AltoLib.UnityExt
{
    public static class TransformExtension
    {
        static Vector3 _vec;

        //---------------------------------------------------------------------
        // Position
        //---------------------------------------------------------------------

        public static void SetPosition(this Transform transform, float? x, float? y, float? z)
        {
            _vec.Set(
                x ?? transform.position.x,
                y ?? transform.position.y,
                z ?? transform.position.z
            );
            transform.position = _vec;
        }

        public static void AddPosition(this Transform transform, float x, float y, float z)
        {
            _vec.Set(
                transform.position.x + x,
                transform.position.y + y,
                transform.position.z + z
            );
            transform.position = _vec;
        }

        public static void AddPosition(this Transform transform, Vector3 vec)
        {
            transform.AddPosition(vec.x, vec.y, vec.z);
        }

        //---------------------------------------------------------------------
        // LocalPosition
        //---------------------------------------------------------------------

        public static void SetLocalPosition(this Transform transform, float? x, float? y, float? z)
        {
            _vec.Set(
                x ?? transform.localPosition.x,
                y ?? transform.localPosition.y,
                z ?? transform.localPosition.z
            );
            transform.localPosition = _vec;
        }

        public static void AddLocalPosition(this Transform transform, float x, float y, float z)
        {
            _vec.Set(
                transform.localPosition.x + x,
                transform.localPosition.y + y,
                transform.localPosition.z + z
            );
            transform.localPosition = _vec;
        }

        public static void AddLocalPosition(this Transform transform, Vector3 vec)
        {
            transform.AddLocalPosition(vec.x, vec.y, vec.z);
        }

        //---------------------------------------------------------------------
        // EulerAngles
        //---------------------------------------------------------------------

        public static void SetEulterAngles(this Transform transform, float? x, float? y, float? z)
        {
            _vec.Set(
                x ?? transform.eulerAngles.x,
                y ?? transform.eulerAngles.y,
                z ?? transform.eulerAngles.z
            );
            transform.eulerAngles = _vec;
        }

        public static void AddEulerAngles(this Transform transform, float x, float y, float z)
        {
            _vec.Set(
                transform.eulerAngles.x + x,
                transform.eulerAngles.y + y,
                transform.eulerAngles.z + z
            );
            transform.eulerAngles = _vec;
        }

        public static void AddEulerAngles(this Transform transform, Vector3 vec)
        {
            transform.AddEulerAngles(vec.x, vec.y, vec.z);
        }

        public static void SetLocalEulterAngles(this Transform transform, float? x, float? y, float? z)
        {
            _vec.Set(
                x ?? transform.localEulerAngles.x,
                y ?? transform.localEulerAngles.y,
                z ?? transform.localEulerAngles.z
            );
            transform.localEulerAngles = _vec;
        }

        public static void AddLocalEulerAngles(this Transform transform, float x, float y, float z)
        {
            _vec.Set(
                transform.localEulerAngles.x + x,
                transform.localEulerAngles.y + y,
                transform.localEulerAngles.z + z
            );
            transform.localEulerAngles = _vec;
        }

        public static void AddLocalEulerAngles(this Transform transform, Vector3 vec)
        {
            transform.AddLocalEulerAngles(vec.x, vec.y, vec.z);
        }
    }
}
