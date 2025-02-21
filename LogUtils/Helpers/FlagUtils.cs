using BepInEx.Logging;
using LogUtils.Enums;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LogUtils.Helpers
{
    internal static class FlagUtils
    {
        private const int FLAG_OFFSET_BEPINEX = 1;
        private const int FLAG_OFFSET_UNITY = LogCategory.CONVERSION_OFFSET;

        /// <summary>
        /// Check that the value is within what is considered the conversion value range
        /// </summary>
        public static bool HasConvertedFlags(int value)
        {
            return value >= LogCategory.CONVERSION_OFFSET;
        }

        /// <summary>
        /// Check that value satisfies a bitflag value requirement - only evaluates positive integers
        /// </summary>
        public static bool HasMultipleFlags(int value)
        {
            //Two's complement encoding allows us to detect multiple flags using this bit operation
            return value > 0 && (value & -value) != value;
        }

        public static IEnumerable<LogLevel> ToFlagEnumerable(LogLevel logLevel)
        {
            return valueToFlags((int)logLevel, FLAG_OFFSET_BEPINEX).Cast<LogLevel>();
        }

        public static IEnumerable<LogType> ToFlagEnumerable(LogType logType)
        {
            return valueToFlags((int)logType, FLAG_OFFSET_UNITY).Cast<LogType>();
        }

        private static IEnumerable<int> valueToFlags(int fullValue, int firstPowerOfTwo)
        {
            if (fullValue < firstPowerOfTwo)
                yield break;

            int bitValue = firstPowerOfTwo;

            //Keep checking until we have checked all possible bit values inside fullValue
            while (bitValue < fullValue)
            {
                if ((fullValue & bitValue) != 0)
                    yield return bitValue;
                bitValue *= 2;
            }
            yield break;
        }
    }
}
