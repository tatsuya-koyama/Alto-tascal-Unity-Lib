using AltoFramework;
using System;
using System.Linq;
using System.Reflection;

namespace AltoLib
{
    public class ObjectUtil
    {
        /// <summary>
        /// from のフィールドの内容をリフレクションで to に反映
        /// </summary>
        public static bool CopyFields(Object from, Object to)
        {
            var fromFields = from.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance).ToList();
            var toFields   = to  .GetType().GetFields(BindingFlags.Public | BindingFlags.Instance).ToList();

            if (fromFields.Count == 0)
            {
                AltoLog.Error($"[ObjectUtil] FieldInfo of from-object is null.");
                return false;
            }
            if (toFields.Count == 0)
            {
                AltoLog.Error($"[ObjectUtil] FieldInfo of to-object is null.");
                return false;
            }

            foreach (var toField in toFields)
            {
                var fromField = fromFields.Find(field => {
                    return field.Name == toField.Name
                        && field.MemberType == toField.MemberType;
                });
                if (fromField == null)
                {
                    AltoLog.Error($"[ObjectUtil] Field not found : {toField.Name}");
                    return false;
                }

                object fromValue = fromField.GetValue(from);
                toField.SetValue(to, fromValue);
            }
            return true;
        }

        /// <summary>
        /// from のプロパティの内容をリフレクションで to に反映
        /// </summary>
        public static bool CopyProps(Object from, Object to)
        {
            var fromProps = from.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
            var toProps   = to  .GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();

            if (fromProps.Count == 0)
            {
                AltoLog.Error($"[ObjectUtil] PropertyInfo of from-object is null.");
                return false;
            }
            if (toProps.Count == 0)
            {
                AltoLog.Error($"[ObjectUtil] PropertyInfo of to-object is null.");
                return false;
            }

            foreach (var toProp in toProps)
            {
                var fromProp = fromProps.Find(prop => {
                    return prop.Name == toProp.Name
                        && prop.PropertyType == toProp.PropertyType;
                });
                if (fromProp == null)
                {
                    AltoLog.Error($"[ObjectUtil] Property not found : {toProp.Name}");
                    return false;
                }

                object fromValue = fromProp.GetValue(from, null);
                toProp.SetValue(to, fromValue, null);
            }
            return true;
        }
    }
}
