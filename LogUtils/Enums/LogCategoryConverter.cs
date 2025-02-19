using BepInEx.Logging;
using LogUtils.Helpers.Extensions;
using System;
using UnityEngine;

namespace LogUtils.Enums
{
    public partial class LogCategory
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
    }
}
