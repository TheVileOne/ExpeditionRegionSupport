using System.Reflection;
using UnityEngine;

namespace LogUtils.Compatibility.Unity
{
    internal static class UnityAdapter
    {
        /// <summary>
        /// Replaces Unity log handler with a LogUtils managed one
        /// </summary>
        public static void Run()
        {
            //Debug.unityLogger.logHandler = new UnityLogHandler(); //Doesn't work for some reason
        }

        internal static void ReplaceLogger()
        {
            FieldInfo loggerField = typeof(Debug).GetField("s_Logger", BindingFlags.NonPublic | BindingFlags.Static);
            loggerField.SetValue(null, new UnityLogHandler());
        }
    }
}
