using System;
using System.Collections;
using System.Collections.Generic;

namespace AltoLib
{
    /// <summary>
    /// CSV のセル文字列をマスタデータのフィールド型へ変換する。
    /// </summary>
    internal static class MasterDataValueConverter
    {
        internal static bool TryConvert(Type type, string value, out object convertedValue)
        {
            if (IsListType(type))
            {
                convertedValue = ConvertList(type, value);
                return true;
            }

            if (value == "" && type != typeof(string)) { value = "0"; }

            if      (type == typeof(int))    { convertedValue = int.Parse(value); }
            else if (type == typeof(long))   { convertedValue = long.Parse(value); }
            else if (type == typeof(string)) { convertedValue = value; }
            else if (type == typeof(float))  { convertedValue = float.Parse(value); }
            else if (type == typeof(double)) { convertedValue = double.Parse(value); }
            else if (type == typeof(bool))   { convertedValue = value != "0"; }
            else if (type.IsEnum)            { convertedValue = ConvertEnum(type, value); }
            else
            {
                convertedValue = null;
                return false;
            }
            return true;
        }

        static object ConvertList(Type listType, string value)
        {
            Type elementType = listType.GetGenericArguments()[0];
            if (IsListType(elementType))
            {
                throw new NotSupportedException("Nested List fields are not supported.");
            }

            var list = (IList)Activator.CreateInstance(listType);
            if (value == String.Empty) { return list; }

            foreach (string element in value.Split(','))
            {
                string elementValue = elementType == typeof(string)
                    ? element
                    : element.Trim();
                if (elementValue == String.Empty)
                {
                    throw new FormatException(
                        $"List<{ elementType.Name }> contains an empty element."
                    );
                }
                if (!TryConvert(elementType, elementValue, out object convertedElement))
                {
                    throw new NotSupportedException(
                        $"List element type is not supported: { elementType.FullName }"
                    );
                }
                list.Add(convertedElement);
            }
            return list;
        }

        static object ConvertEnum(Type enumType, string value)
        {
            object enumValue;
            try
            {
                enumValue = Enum.Parse(enumType, value);
            }
            catch (ArgumentException exception)
            {
                throw new FormatException(
                    $"Enum value '{ value }' is not valid for { enumType.Name }.",
                    exception
                );
            }

            if (!Enum.IsDefined(enumType, enumValue))
            {
                throw new FormatException(
                    $"Enum value '{ value }' is not defined for { enumType.Name }."
                );
            }
            return enumValue;
        }

        static bool IsListType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        }
    }
}
