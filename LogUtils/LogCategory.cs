using BepInEx.Logging;
using UnityEngine;

namespace LogUtils
{
    public class LogCategory : SharedExtEnum<LogCategory>
    {
        /// <summary>
        /// The index offset where custom conversions of LogCategory values exist as LogLevel or LogType 
        /// </summary>
        public const int CONVERSION_OFFSET = 150;

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
                    _bepInExConversion = bepInExEquivalent ?? (LogLevel)(CONVERSION_OFFSET + index);
                    _unityConversion = unityEquivalent ?? (LogType)(CONVERSION_OFFSET + index);
                }
                else
                {
                    _bepInExConversion = bepInExEquivalent ?? LogLevel.Info;
                    _unityConversion = unityEquivalent ?? LogType.Log;
                }
            }
            else if (!ManagedReference.Registered)
            {
                ManagedReference._bepInExConversion = bepInExEquivalent ?? (LogLevel)(CONVERSION_OFFSET + index);
                ManagedReference._unityConversion = unityEquivalent ?? (LogType)(CONVERSION_OFFSET + index);
            }
        }

        public LogCategory(string value, bool register = false) : base(value, register)
        {
            if (ReferenceEquals(ManagedReference, this)) //Fields only have to be updated by the reference object
            {
                if (Registered)
                {
                    _bepInExConversion = (LogLevel)(CONVERSION_OFFSET + index);
                    _unityConversion = (LogType)(CONVERSION_OFFSET + index);
                }
            }
            else if (!ManagedReference.Registered && Registered)
            {
                ManagedReference._bepInExConversion = (LogLevel)(CONVERSION_OFFSET + index);
                ManagedReference._unityConversion = (LogType)(CONVERSION_OFFSET + index);
            }
        }

        public static LogCategory ToCategory(string value)
        {
            return new LogCategory(value);
        }

        public static LogCategory ToCategory(LogLevel logLevel)
        {
            int enumValue = (int)logLevel;

            //A high enum value indicates that we are handling a custom LogCategory converted to an enum type
            if (enumValue >= CONVERSION_OFFSET)
            {
                int categoryIndex = enumValue - CONVERSION_OFFSET;
                string[] categoryNames = GetNames(typeof(LogCategory));

                return new LogCategory(categoryNames[categoryIndex]);
            }

            //More typical enum type values can be translated directly to string
            return new LogCategory(logLevel.ToString());
        }

        public static LogCategory ToCategory(LogType logLevel)
        {
            int enumValue = (int)logLevel;

            //A high enum value indicates that we are handling a custom LogCategory converted to an enum type
            if (enumValue >= CONVERSION_OFFSET)
            {
                int categoryIndex = enumValue - CONVERSION_OFFSET;
                string[] categoryNames = GetNames(typeof(LogCategory));

                return new LogCategory(categoryNames[categoryIndex]);
            }

            //More typical enum type values can be translated directly to string
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
