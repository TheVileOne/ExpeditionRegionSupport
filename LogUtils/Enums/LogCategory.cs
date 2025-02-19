using BepInEx.Logging;
using LogUtils.Helpers;
using System;
using System.Linq;
using UnityEngine;

namespace LogUtils.Enums
{
    public class LogCategory : SharedExtEnum<LogCategory>
    {
        /// <summary>
        /// The bit-oriented value position of an enum value (LogLevel or LogType) reserved for custom conversions of LogCategory values
        /// <br>
        /// Value must be compliant with BepInEx.LogType, which assigns a max value of 63.
        /// This value must be at least 64 or greater for compatibility purposes
        /// </br>
        /// </summary>
        public const short CONVERSION_OFFSET = 256;

        /// <summary>
        /// The power of two used to produce the conversion offset
        /// </summary>
        public const byte CONVERSION_OFFSET_POWER = 8;

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
        /// The bitflag translation representing this LogCategory
        /// TODO: Implement for composites
        /// </summary>
        public virtual int FlagValue => indexToConversionValue();

        public static LogCategory[] RegisteredEntries => values.entries.Select(entry => new LogCategory(entry)).ToArray();

        public static LogCategoryCombiner Combiner = new LogCategoryCombiner();

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
                    int customValue = indexToConversionValue();
                    ManagedReference.updateConversionFields(bepInExEquivalent ?? (LogLevel)customValue, unityEquivalent ?? (LogType)customValue);
                }

                //Make sure that backing fields always have the same values as the managed reference
                updateConversionFields(ManagedReference._bepInExConversion, ManagedReference._unityConversion);
            }
            else
            {
                int customValue = indexToConversionValue();
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
                        int customValue = indexToConversionValue();
                        ManagedReference.updateConversionFields((LogLevel)customValue, (LogType)customValue);
                    }

                    //Make sure that backing fields always have the same values as the managed reference
                    updateConversionFields(ManagedReference._bepInExConversion, ManagedReference._unityConversion);
                }
                else
                {
                    int customValue = indexToConversionValue();
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

        private int indexToConversionValue()
        {
            if (index < 0) return -1;

            //Map the registration index to an unmapped region of values designated for custom enum values
            //Assigned value will be a valid bit flag position
            return (int)Math.Min(CONVERSION_OFFSET * Math.Pow(index, 2), int.MaxValue);
        }

        private void updateConversionFields(LogLevel logLevel, LogType logType)
        {
            _bepInExConversion = logLevel;
            _unityConversion = logType;

            conversionFieldsNeedUpdating = false;
        }

        public static LogID GetUnityLogID(LogType logType)
        {
            return !IsErrorCategory(logType) ? LogID.Unity : LogID.Exception;
        }

        public static bool IsErrorCategory(LogCategory category)
        {
            var composite = category as CompositeLogCategory;

            if (composite != null)
            {
                //Exclude the All flag here - not relevant to error handling
                return !composite.Contains(All) && composite.HasAny(Error | Fatal);
            }
            return category == Error || category == Fatal;
        }

        public static bool IsErrorCategory(LogType category)
        {
            return category == LogType.Error || category == LogType.Exception;
        }

        public static bool IsErrorCategory(LogLevel category)
        {
            return (category & (LogLevel.Error | LogLevel.Fatal)) != 0;
        }

        public static LogCategory ToCategory(string value)
        {
            return new LogCategory(value);
        }

        public static LogCategory ToCategory(LogLevel logLevel)
        {
            var composition = logLevel.Deconstruct();
            int flagCount = composition.Length;

            if (flagCount == 0)
                return None;

            if (flagCount == 1)
            {
                LogLevel flag = composition[0];

                if (flag == All.BepInExCategory)
                    return All;

                return Convert(logLevel);
            }

            //Create a composite LogCategory from the available enum flags
            CompositeLogCategory composite = null;
            for (int i = 1; i < composition.Length; i++)
            {
                if (composite == null)
                {
                    composite = Convert(composition[i - 1]) | Convert(composition[i]);
                    continue;
                }

                //Value at i - 1 will already be part of the composition
                composite |= Convert(composition[i]);
            }
            return composite;
        }

        public static LogCategory ToCategory(LogType logType)
        {
            var composition = logType.Deconstruct();
            int flagCount = composition.Length;

            if (flagCount == 0)
                return None;

            if (flagCount == 1)
            {
                LogType flag = composition[0];

                if (flag == LogType.Log)
                    return Default;

                if (flag == All.UnityCategory)
                    return All;

                return Convert(logType);
            }

            //Create a composite LogCategory from the available enum flags
            CompositeLogCategory composite = null;
            for (int i = 1; i < composition.Length; i++)
            {
                if (composite == null)
                {
                    composite = Convert(composition[i - 1]) | Convert(composition[i]);
                    continue;
                }

                //Value at i - 1 will already be part of the composition
                composite |= Convert(composition[i]);
            }
            return composite;
        }

        /// <summary>
        /// An internal helper that assumes that composite checks have already been handled, and input is not a composite
        /// </summary>
        internal static LogCategory Convert(LogLevel logLevel)
        {
            int enumValue = (int)logLevel;

            //A high enum value indicates that we are handling a custom LogCategory converted to an enum type
            if (enumValue >= CONVERSION_OFFSET)
                return valueToCategory(enumValue);

            //More typical enum type values can be translated directly to string
            return new LogCategory(logLevel.ToString());
        }

        /// <summary>
        /// An internal helper that assumes that composite checks have already been handled, and input is not a composite
        /// </summary>
        internal static LogCategory Convert(LogType logType)
        {
            int enumValue = (int)logType;

            //A high enum value indicates that we are handling a custom LogCategory converted to an enum type
            if (enumValue >= CONVERSION_OFFSET)
                return valueToCategory(enumValue);

            //More typical enum type values can be translated directly to string
            return new LogCategory(logType.ToString());
        }

        private static LogCategory valueToCategory(int enumValue)
        {
            int categoryIndex = (int)Math.Log(enumValue - CONVERSION_OFFSET, 2);

            try
            {
                return EntryAt(categoryIndex);
            }
            catch (ArgumentOutOfRangeException)
            {
                UtilityLogger.LogWarning("Invalid conversion offset processed during LogCategory conversion");
            }
            return Default;
        }

        public static CompositeLogCategory operator |(LogCategory a, LogCategory b)
        {
            return Combiner.Combine(a, b);
        }

        public static CompositeLogCategory operator &(LogCategory a, LogCategory b)
        {
            return Combiner.Intersect(a, b);
        }

        public static CompositeLogCategory operator ^(LogCategory a, LogCategory b)
        {
            return Combiner.Distinct(a, b);
        }

        public static LogCategory operator ~(LogCategory target)
        {
            return Combiner.GetComplement(target);
        }

        static LogCategory()
        {
            Default = Info;
        }

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

        public static readonly LogCategory Default;
    }
}
