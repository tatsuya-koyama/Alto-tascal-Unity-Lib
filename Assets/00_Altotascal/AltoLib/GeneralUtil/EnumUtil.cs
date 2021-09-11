using AltoFramework;
using System;

namespace AltoLib
{
    public class EnumUtil
    {
        public static T FromInt<T>(int value) where T : struct
        {
            return (T)Enum.ToObject(typeof(T), value);
        }

        /// <summary>
        /// "1" や "2" といった数字文字列を Enum の値に変換する。
        /// 空文字列の場合は 0 として扱う
        /// </summary>
        public static T FromNumericString<T>(string str) where T : struct
        {
            if (str == String.Empty)
            {
                return EnumUtil.FromInt<T>(0);
            }

            try
            {
                T value = (T)Enum.ToObject(typeof(T), int.Parse(str));
                return value;
            }
            catch (Exception ex)
            {
                AltoLog.Error($"[EnumUtil] Parse error : {str}");
                throw ex;
            }
        }

        /// <summary>
        /// Enum の定義名と同じ文字列から Enum の値に変換する
        /// </summary>
        public static T FromString<T>(string str) where T : struct
        {
            T result;
            bool parsed = Enum.TryParse(str, out result) && Enum.IsDefined(typeof(T), result);
            if (!parsed)
            {
                AltoLog.Error($"[EnumUtil] Parse error : {str}");
            }
            return result;
        }

        /// <summary>
        /// TryParse のラップ版。Enum.TryParse() は "123" といった数字文字列が渡されたときに
        /// エラーにならないので、ここでは定義済みの Enum であるかどうかのチェックを含めている
        /// </summary>
        public static bool TryParse<T>(string str, out T result) where T : struct
        {
            bool parsed = Enum.TryParse(str, out result) && Enum.IsDefined(typeof(T), result);
            if (!parsed)
            {
                AltoLog.Error($"[EnumUtil] Parse error : {str}");
            }
            return parsed;
        }
    }
}
