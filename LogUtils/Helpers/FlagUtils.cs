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

        public static int GetHighestBit(int value)
        {
            if (value <= 0) return 0;

            //We know value is somewhere between nearestBitValue, and zero
            int nearestBitValue = Mathf.ClosestPowerOfTwo(value);
            bool matchFound;
            do
            {
                matchFound = (value & nearestBitValue) != 0;

                if (!matchFound)
                    nearestBitValue /= 2;
            }
            while (!matchFound);
            return nearestBitValue;
        }

        public static int GetLowestBit(int value)
        {
            if (value <= 0) return 0;

            int currentBitValue = 1;
            bool matchFound;
            do
            {
                matchFound = (value & currentBitValue) != 0;

                if (!matchFound)
                    currentBitValue *= 2;
            }
            while (!matchFound);
            return currentBitValue;
        }

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

    /// <summary>
    /// Enum type representing options for matching flag enums, or composite <see cref="ExtEnum{T}"/> types
    /// </summary>
    public enum FlagSearchOption
    {
        /// <summary>
        /// All specified flags need to be present to qualify as a match
        /// </summary>
        MatchAll,
        /// <summary>
        /// Any specified flag needs to be present to qualify as a match
        /// </summary>
        MatchAny
    }
}
