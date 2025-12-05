using BepInEx.Logging;
using LogUtils.Helpers;
using System;
using UnityEngine;

namespace LogUtils.Enums
{
    public partial class LogCategory
    {
        /// <summary>
        /// The bit-oriented value position of an enum value (<see cref="BepInEx.Logging.LogLevel"/> or <see cref="LogType"/>) reserved for custom conversions of <see cref="LogCategory"/> values
        /// </summary>
        /// <remarks>Value must be compliant with <see cref="BepInEx.Logging.LogLevel"/>, which assigns a max value of 63.
        /// This value must be at least 64 or greater for compatibility purposes</remarks>
        public const short CONVERSION_OFFSET = 256;

        /// <summary>
        /// The power of two used to produce the conversion offset
        /// </summary>
        public const byte CONVERSION_OFFSET_POWER = 8;

        /// <summary>
        /// Converts a <see cref="string"/> to its equivalent <see cref="LogCategory"/> instance
        /// </summary>
        /// <returns><see cref="LogCategory"/> instance will be unregistered if no registered equivalent exists</returns>
        public static LogCategory ToCategory(string value)
        {
            return new LogCategory(value);
        }

        /// <summary>
        /// Converts a <see cref="BepInEx.Logging.LogLevel"/> value to its equivalent <see cref="LogCategory"/> instance
        /// </summary>
        /// <returns><see cref="LogCategory"/> instance will be unregistered if no registered equivalent exists</returns>
        public static LogCategory ToCategory(LogLevel logLevel)
        {
            var flags = logLevel.Deconstruct();
            int flagCount = flags.Length;

            if (flagCount == 0)
                return None;

            if (flagCount == 1)
            {
                LogLevel flag = flags[0];

                if (flag == All.BepInExCategory)
                    return All;

                return GetEquivalent(logLevel);
            }
            return CompositeLogCategory.FromFlags(flags);
        }

        /// <summary>
        /// Converts a <see cref="LogType"/> value to its equivalent <see cref="LogCategory"/> instance
        /// </summary>
        /// <returns><see cref="LogCategory"/> instance will be unregistered if no registered equivalent exists</returns>
        public static LogCategory ToCategory(LogType logType)
        {
            var flags = logType.Deconstruct();
            int flagCount = flags.Length;

            if (flagCount == 0)
                return None;

            if (flagCount == 1)
            {
                LogType flag = flags[0];

                if (flag == LOG_TYPE_DEFAULT)
                    return Default;

                if (flag == All.UnityCategory)
                    return All;

                return GetEquivalent(logType);
            }
            return CompositeLogCategory.FromFlags(flags);
        }

        /// <summary>
        /// Converts a <see cref="BepInEx.Logging.LogLevel"/> value to its equivalent <see cref="LogCategoryLevels"/> flags
        /// </summary>
        public static LogCategoryLevels ToCategoryLevel(LogLevel logLevel)
        {
            return LogCategoryLevelMap.GetEquivalentSlow(logLevel);
        }

        /// <summary>
        /// An internal helper that assumes that composite checks have already been handled, and input is not a composite
        /// </summary>
        internal static LogCategory GetEquivalent(LogLevel logLevel)
        {
            int enumValue = (int)logLevel;

            if (FlagUtils.HasConvertedFlags(enumValue))
                return valueToCategory(enumValue);

            //More typical enum type values can be translated directly to string
            return new LogCategory(logLevel.ToString());
        }

        /// <summary>
        /// An internal helper that assumes that composite checks have already been handled, and input is not a composite
        /// </summary>
        internal static LogCategory GetEquivalent(LogType logType)
        {
            int enumValue = (int)logType;

            if (FlagUtils.HasConvertedFlags(enumValue))
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
                UtilityLogger.LogWarning("Invalid conversion offset processed during LogCategory conversion. Offset: " + enumValue);
            }
            return Default;
        }
    }
}
