using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace AltoFramework
{
    /// <summary>
    ///   Custom Console Log.
    ///   Define "ALTO_DEBUG" symbol to enable this log.
    /// </summary>
    public class AltoLog
    {
        #if ALTO_DEBUG_LIGHT_SKIN
        public const string COLOR_FW      = "007777";  // Filtered by [ALTO_FW]
        public const string COLOR_FW_WARN = "770077";  // Filtered by [ALTO_FW_WARN]
        public const string COLOR_VERBOSE = "606060";  // Filtered by [@]
        public const string COLOR_DEFAULT = "000000";  // Filtered by [@@]
        public const string COLOR_SUCCESS = "008811";  // Filtered by [@@@]
        public const string COLOR_FAIL    = "990099";  // Filtered by [@@@]
        public const string COLOR_NOTICE  = "440099";  // Filtered by [@@@@]
        public const string COLOR_WARN    = "996600";  // Filtered by [@@@@@]
        public const string COLOR_ERROR   = "aa0000";  // Filtered by [@@@@@@]
        #else
        public const string COLOR_FW      = "22cccc";  // Filtered by [ALTO_FW]
        public const string COLOR_FW_WARN = "ff33ff";  // Filtered by [ALTO_FW_WARN]
        public const string COLOR_VERBOSE = "8f8f8f";  // Filtered by [@]
        public const string COLOR_DEFAULT = "cccccc";  // Filtered by [@@]
        public const string COLOR_SUCCESS = "33ee00";  // Filtered by [@@@]
        public const string COLOR_FAIL    = "22aaff";  // Filtered by [@@@]
        public const string COLOR_NOTICE  = "eeee33";  // Filtered by [@@@@]
        public const string COLOR_WARN    = "ff9900";  // Filtered by [@@@@@]
        public const string COLOR_ERROR   = "ff3322";  // Filtered by [@@@@@@]
        #endif

        [Conditional("ALTO_DEBUG")]
        public static void FW(object message, Object context = null, string color = COLOR_FW)
        {
            UnityEngine.Debug.Log($"<color=#{color}>{message}</color>\n[ALTO_FW]", context);
        }

        [Conditional("ALTO_DEBUG")]
        public static void FW_Warn(object message, Object context = null, string color = COLOR_FW_WARN)
        {
            UnityEngine.Debug.Log($"<color=#{color}>{message}</color>\n[ALTO_FW_WARN]", context);
        }

        [Conditional("ALTO_DEBUG")]
        public static void Verb(object message, Object context = null, string color = COLOR_VERBOSE)
        {
            UnityEngine.Debug.Log($"<color=#{color}>{message}</color>\n[@]", context);
        }

        [Conditional("ALTO_DEBUG")]
        public static void Info(object message, Object context = null, string color = COLOR_DEFAULT)
        {
            UnityEngine.Debug.Log($"<color=#{color}>{message}</color>\n[@@]", context);
        }

        [Conditional("ALTO_DEBUG")]
        public static void Success(object message, Object context = null, string color = COLOR_SUCCESS)
        {
            UnityEngine.Debug.Log($"<color=#{color}>{message}</color>\n[@@@]", context);
        }

        [Conditional("ALTO_DEBUG")]
        public static void Fail(object message, Object context = null, string color = COLOR_FAIL)
        {
            UnityEngine.Debug.Log($"<color=#{color}>{message}</color>\n[@@@]", context);
        }

        [Conditional("ALTO_DEBUG")]
        public static void Notice(object message, Object context = null, string color = COLOR_NOTICE)
        {
            UnityEngine.Debug.Log($"<color=#{color}>{message}</color>\n[@@@@]", context);
        }

        [Conditional("ALTO_DEBUG")]
        public static void Warn(object message, Object context = null, string color = COLOR_WARN)
        {
            UnityEngine.Debug.LogWarning($"<color=#{color}>{message}</color>\n[@@@@@]", context);
        }

        [Conditional("ALTO_DEBUG")]
        public static void Error(object message, Object context = null, string color = COLOR_ERROR)
        {
            UnityEngine.Debug.LogError($"<color=#{color}>{message}</color>\n[@@@@@@]", context);
        }

        //----------------------------------------------------------------------
        // Utilities
        //----------------------------------------------------------------------

        [Conditional("ALTO_DEBUG")]
        public static void Clear()
        {
            #if UNITY_EDITOR
            var assembly = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.Editor));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
            #endif
        }

        [Conditional("ALTO_DEBUG")]
        public static void DumpList<T>(IEnumerable<T> list, Object context = null, string color = COLOR_VERBOSE)
        {
            string output = "";
            int index = 0;
            foreach (var item in list)
            {
                output += $"[{index}] : {item.ToString()}\n";
                ++index;
            }
            UnityEngine.Debug.Log($"<color=#{color}>{output}</color>\n[@@@@@@]", context);
        }

        [Conditional("ALTO_DEBUG")]
        public static void DumpDictionary<T1, T2>(Dictionary<T1, T2> dictionary, Object context = null, string color = COLOR_VERBOSE)
        {
            string output = "";
            foreach (KeyValuePair<T1, T2> kv in dictionary)
            {
                output += $"{kv.Key} : {kv.Value}\n";
            }
            UnityEngine.Debug.Log($"<color=#{color}>{output}</color>\n[@@@@@@]", context);
        }
    }
}
