using System;

namespace AltoLib
{
    public class EnumUtil
    {
        public static T FromInt<T>(int value)
        {
            return (T)Enum.ToObject(typeof(T), value);
        }

        /// <summary>
        /// "1" や "2" といった数字文字列を Enum の値に変換する。
        /// 空文字列の場合は 0 として扱う
        /// </summary>
        public static T FromNumericString<T>(string str)
        {
            if (str == String.Empty)
            {
                return EnumUtil.FromInt<T>(0);
            }
            return (T)Enum.ToObject(typeof(T), int.Parse(str));
        }
    }
}
