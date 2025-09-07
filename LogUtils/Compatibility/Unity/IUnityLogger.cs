using LogUtils.Enums;
using System;
using UnityEngine;

namespace LogUtils.Compatibility.Unity
{
    public interface IUnityLogger : UnityEngine.ILogger
    {
        void Log(LogType category, FormattableString messageObj, UnityEngine.Object context);

        void Log(LogType category, string tag, FormattableString messageObj);

        void Log(LogType category, string tag, FormattableString messageobj, UnityEngine.Object context);

        void Log(string tag, FormattableString messageObj);

        void Log(string tag, FormattableString messageObj, UnityEngine.Object context);

        void LogWarning(string tag, FormattableString messageObj);

        void LogWarning(string tag, FormattableString messageObj, UnityEngine.Object context);

        void LogError(string tag, FormattableString messageObj);

        void LogError(string tag, FormattableString messageObj, UnityEngine.Object context);
    }
}
