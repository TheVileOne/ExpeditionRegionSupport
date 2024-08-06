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

        /// <summary>
        /// The default conversion type for LogLevel enum
        /// </summary>
        public const LogLevel LOG_LEVEL_DEFAULT = LogLevel.Info;

        /// <summary>
        /// The default conversion type for LogType enum
        /// </summary>
        public const LogType LOG_TYPE_DEFAULT = LogType.Log;

        private LogLevel _bepInExConversion = LOG_LEVEL_DEFAULT;
        private LogType _unityConversion = LOG_TYPE_DEFAULT;

        /// <summary>
        /// A flag that indicates that conversion fields need to be updated
        /// </summary>
        private bool conversionFieldsNeedUpdating;

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

        /// <summary>
        /// Constructs a registered LogCategory instance
        /// </summary>
        /// <param name="value">The ExtEnum value associated with this instance</param>
        /// <param name="bepInExEquivalent">
        /// The enum value to be used when this category is used by the BepInEx logger
        /// Set to null assigns a custom LogLevel, otherwise will take the value of the LogLevel provided
        /// </param>
        /// <param name="unityEquivalent">
        /// The enum value to be used when this category is used by the Unity logger
        /// Set to null assigns a custom LogType, otherwise will take the value of the LogType provided
        /// </param>
        public LogCategory(string value, LogLevel? bepInExEquivalent, LogType? unityEquivalent) : base(value, true)
        {
            if (!ReferenceEquals(ManagedReference, this))
            {
                //When the managed reference is registered at the same time as this instance, propagate constructor parameters to the managed reference
                if (ManagedReference.conversionFieldsNeedUpdating)
                {
                    int customValue = CONVERSION_OFFSET + index;
                    ManagedReference.updateConversionFields(bepInExEquivalent ?? (LogLevel)customValue, unityEquivalent ?? (LogType)customValue);
                }

                //Make sure that backing fields always have the same values as the managed reference
                updateConversionFields(ManagedReference._bepInExConversion, ManagedReference._unityConversion);
            }
            else
            {
                int customValue = CONVERSION_OFFSET + index;
                updateConversionFields(bepInExEquivalent ?? (LogLevel)customValue, unityEquivalent ?? (LogType)customValue);
            }
        }

        /// <summary>
        /// Constructs a LogCategory instance
        /// </summary>
        /// <param name="value">The ExtEnum value associated with this instance</param>
        /// <param name="register">Whether or not this instance should be registered as a unique ExtEnum entry</param>
        public LogCategory(string value, bool register = false) : base(value, register)
        {
            //We can only give custom conversions to registered instances because these instances have a valid index assigned
            if (Registered)
            {
                if (!ReferenceEquals(ManagedReference, this))
                {
                    if (ManagedReference.conversionFieldsNeedUpdating)
                    {
                        int customValue = CONVERSION_OFFSET + index;
                        ManagedReference.updateConversionFields((LogLevel)customValue, (LogType)customValue);
                    }

                    //Make sure that backing fields always have the same values as the managed reference
                    updateConversionFields(ManagedReference._bepInExConversion, ManagedReference._unityConversion);
                }
                else
                {
                    int customValue = CONVERSION_OFFSET + index;
                    updateConversionFields((LogLevel)customValue, (LogType)customValue);
                }
            }
        }

        public override void Register()
        {
            //When a registration is applied, set a flag that allows conversion fields to be overwritten
            if (!ManagedReference.Registered || index < 0)
                conversionFieldsNeedUpdating = true;

            base.Register();
        }

        private void updateConversionFields(LogLevel logLevel, LogType logType)
        {
            _bepInExConversion = logLevel;
            _unityConversion = logType;

            conversionFieldsNeedUpdating = false;
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
        public static readonly LogCategory Assert = new LogCategory("Assert", null, LogType.Assert);
        public static readonly LogCategory Debug = new LogCategory("Debug", LogLevel.Debug, null);
        public static readonly LogCategory Info = new LogCategory("Info", LogLevel.Info, LogType.Log);
        public static readonly LogCategory Message = new LogCategory("Message", LogLevel.Message, LogType.Log);
        public static readonly LogCategory Important = new LogCategory("Important", null, null);
        public static readonly LogCategory Warning = new LogCategory("Warning", LogLevel.Warning, LogType.Warning);
        public static readonly LogCategory Error = new LogCategory("Error", LogLevel.Error, LogType.Error);
        public static readonly LogCategory Fatal = new LogCategory("Fatal", LogLevel.Fatal, LogType.Error);
    }
}
