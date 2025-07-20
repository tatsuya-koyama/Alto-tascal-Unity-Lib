using System;
using System.Diagnostics;
using UnityEngine;

namespace AltoFramework
{
    public class CustomLogger
    {
        public enum FW_LogLevel
        {
            Debug  = 1,
            Warn   = 3,
            Error  = 4,
            Silent = 9,
        }

        public enum LogLevel
        {
            Verb   = 0,
            Debug  = 1,
            Info   = 2,
            Warn   = 3,
            Error  = 4,
            Silent = 9,
        }

        public FW_LogLevel fwLogLevel = FW_LogLevel.Debug;
        public LogLevel logLevel = LogLevel.Debug;

        public const string COLOR_FW        = "22cccc";
        public const string COLOR_FW_WARN   = "ff33ff";
        public const string COLOR_FW_ERROR  = "d23225";
        public const string COLOR_VERBOSE   = "8f8f8f";
        public const string COLOR_DEBUG     = "cccccc";
        public const string COLOR_INFO      = "eeee33";
        public const string COLOR_WARN      = "ff9900";
        public const string COLOR_ERROR     = "ff3322";
        public const string COLOR_EXCEPTION = "ff2288";
        public const string COLOR_SUCCESS   = "33ee00";
        public const string COLOR_FAIL      = "22aaff";

        //----------------------------------------------------------------------
        // AltoFramework 内部から呼ぶ用
        //----------------------------------------------------------------------

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public void FW(object message, object context = null)
        {
            if (fwLogLevel > FW_LogLevel.Debug) { return; }
            UnityEngine.Debug.Log($"{MakeTag(context)} <color=#{COLOR_FW}>{message}</color>");
        }

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public void FW_Warn(object message, object context = null)
        {
            if (fwLogLevel > FW_LogLevel.Warn) { return; }
            UnityEngine.Debug.LogWarning($"{MakeTag(context)} <color=#{COLOR_FW_WARN}>{message}</color>");
        }

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public void FW_Error(object message, object context = null)
        {
            if (fwLogLevel > FW_LogLevel.Error) { return; }
            UnityEngine.Debug.LogError($"{MakeTag(context)} <color=#{COLOR_FW_ERROR}>{message}</color>");
        }

        //----------------------------------------------------------------------
        // カスタムログ
        //----------------------------------------------------------------------

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public void Verb(object message, object context = null)
        {
            if (logLevel > LogLevel.Verb) { return; }
            if (_logFilter == null) { return; }
            if (!_logFilter.IsVerbose(context)) { return; }
            UnityEngine.Debug.Log($"{MakeTag(context)} <color=#{COLOR_VERBOSE}>{message}</color>");
        }

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public void Debug(object message, object context = null)
        {
            if (logLevel > LogLevel.Debug) { return; }
            UnityEngine.Debug.Log($"{MakeTag(context)} <color=#{COLOR_DEBUG}>{message}</color>");
        }

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public void Info(object message, object context = null)
        {
            if (logLevel > LogLevel.Info) { return; }
            UnityEngine.Debug.Log($"{MakeTag(context)} <color=#{COLOR_INFO}>{message}</color>");
        }

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public void Success(object message, object context = null)
        {
            if (logLevel > LogLevel.Info) { return; }
            UnityEngine.Debug.Log($"{MakeTag(context)} <color=#{COLOR_SUCCESS}>{message}</color>");
        }

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public void Fail(object message, object context = null)
        {
            if (logLevel > LogLevel.Info) { return; }
            UnityEngine.Debug.Log($"{MakeTag(context)} <color=#{COLOR_FAIL}>{message}</color>");
        }

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public void Warn(object message, object context = null)
        {
            if (logLevel > LogLevel.Warn) { return; }
            UnityEngine.Debug.LogWarning($"{MakeTag(context)} <color=#{COLOR_WARN}>{message}</color>");
        }

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public void Error(object message, object context = null)
        {
            if (logLevel > LogLevel.Error) { return; }
            UnityEngine.Debug.LogError($"{MakeTag(context)} <color=#{COLOR_ERROR}>{message}</color>");
        }

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public void Exception(Exception exception, object context = null)
        {
            if (logLevel > LogLevel.Error) { return; }
            UnityEngine.Debug.LogException(exception);
        }

        //----------------------------------------------------------------------
        // private
        //----------------------------------------------------------------------

        string MakeTag(object obj)
        {
            if (obj == null) { return string.Empty; }
            string tag = GetTagString(obj);
            string colorCode = GetColorCode(tag);
            return $"<color=#{colorCode}>[{tag}]</color>";
        }

        string GetTagString(object obj)
        {
            if (obj is String) { return obj.ToString(); }
            return obj.GetType().Name;
        }

        string GetColorCode(string str)
        {
            int hash = Math.Abs(str.GetHashCode());
            Color color = Color.HSVToRGB((hash % 360) / 360f, 1.0f, 1.0f);

            // 同じ明度でも人間の目に見える明るさ（輝度）は異なる（青は暗く緑は明るい）ので
            // 輝度を考慮して色味が揃うように調整
            float luminance = (color.r * 0.299f) + (color.g * 0.587f) + (color.b * 0.114f);
            color = Color.HSVToRGB((hash % 360) / 360f, 0.3f + luminance * 0.8f, 1.0f - luminance * 0.2f);

            return ColorUtility.ToHtmlStringRGB(color);
        }

        //----------------------------------------------------------------------
        // Verbose ログのフィルタ機構
        //----------------------------------------------------------------------

        public interface ILogFilter
        {
            bool IsVerbose(object context);
        }
        ILogFilter _logFilter;

        public void SetLogFilter(ILogFilter filter)
        {
            _logFilter = filter;
        }
    }
}
