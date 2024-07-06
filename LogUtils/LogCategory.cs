using BepInEx.Logging;
using UnityEngine;

namespace LogUtils
{
    public class LogCategory : SharedExtEnum<LogCategory>
    {
        private LogLevel _bepInExConversion = LogLevel.Info;
        private LogType _unityConversion = LogType.Log;

        /// <summary>
        /// The category value translated to the category enum used for BepInEx logging
        /// </summary>
        public LogLevel BepInExCategory
        {
            get
            {
                if (!ReferenceEquals(ManagedReference, this))
                    return ManagedReference.BepInExCategory;
                return _bepInExConversion;
            }
        }

        /// <summary>
        /// The category value translated to the category enum used for Unity logging
        /// </summary>
        public LogType UnityCategory
        {
            get
            {
                if (!ReferenceEquals(ManagedReference, this))
                    return ManagedReference.UnityCategory;
                return _unityConversion;
            }
        }

        public LogCategory(string value, LogLevel? bepInExEquivalent, LogType? unityEquivalent) : base(value, true)
        {
            if (ReferenceEquals(ManagedReference, this)) //Fields only have to be updated by the reference object
            {
                if (Registered)
                {
                    _bepInExConversion = bepInExEquivalent ?? (LogLevel)(150 + index);
                    _unityConversion = unityEquivalent ?? (LogType)(150 + index);
                }
                else
                {
                    _bepInExConversion = bepInExEquivalent ?? LogLevel.Info;
                    _unityConversion = unityEquivalent ?? LogType.Log;
                }
            }
            else if (!ManagedReference.Registered)
            {
                ManagedReference._bepInExConversion = bepInExEquivalent ?? (LogLevel)(150 + index);
                ManagedReference._unityConversion = unityEquivalent ?? (LogType)(150 + index);
            }
        }

        public LogCategory(string value, bool register = false) : base(value, register)
        {
            if (ReferenceEquals(ManagedReference, this)) //Fields only have to be updated by the reference object
            {
                if (Registered)
                {
                    _bepInExConversion = (LogLevel)(150 + index);
                    _unityConversion = (LogType)(150 + index);
                }
            }
            else if (!ManagedReference.Registered && Registered)
            {
                ManagedReference._bepInExConversion = (LogLevel)(150 + index);
                ManagedReference._unityConversion = (LogType)(150 + index);
            }
        }

        public static LogCategory ToCategory(string value)
        {
            return new LogCategory(value);
        }

        public static LogCategory ToCategory(LogLevel logLevel)
        {
            return new LogCategory(logLevel.ToString());
        }

        public static LogCategory Default => Info;

        public static readonly LogCategory All = new LogCategory("All", LogLevel.All, null);
        public static readonly LogCategory None = new LogCategory("None", LogLevel.None, LogType.Log);
        public static readonly LogCategory Debug = new LogCategory("Debug", LogLevel.Debug, null);
        public static readonly LogCategory Info = new LogCategory("Info", LogLevel.Info, LogType.Log);
        public static readonly LogCategory Message = new LogCategory("Message", LogLevel.Message, LogType.Log);
        public static readonly LogCategory Important = new LogCategory("Important", null, null);
        public static readonly LogCategory Warning = new LogCategory("Warning", LogLevel.Warning, LogType.Warning);
        public static readonly LogCategory Error = new LogCategory("Error", LogLevel.Error, LogType.Error);
        public static readonly LogCategory Fatal = new LogCategory("Fatal", LogLevel.Fatal, LogType.Error);
    }
}
