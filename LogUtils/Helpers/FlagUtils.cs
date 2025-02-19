using BepInEx.Logging;
using LogUtils.Enums;
using System.Collections.Generic;
using UnityEngine;

namespace LogUtils.Helpers
{
    internal static class FlagUtils
    {
        private const int FLAG_OFFSET_BEPINEX = 1;
        private const int FLAG_OFFSET_UNITY = LogCategory.CONVERSION_OFFSET;

        public static IEnumerable<LogLevel> ToFlagEnumerable(LogLevel logLevel)
        {
            var enumerator = powersOfTwo(FLAG_OFFSET_BEPINEX).GetEnumerator();

            //Cycle through powers of 2, recording all set flags
            while (enumerator.MoveNext())
            {
                var flag = (LogLevel)enumerator.Current;

                bool hasFlag = (logLevel & flag) != 0;

                if (hasFlag)
                    yield return flag;
            }
            yield break;
        }

        public static IEnumerable<LogType> ToFlagEnumerable(LogType logType)
        {
            var enumerator = powersOfTwo(FLAG_OFFSET_UNITY).GetEnumerator();

            //Cycle through powers of 2, recording all set flags
            while (enumerator.MoveNext())
            {
                var flag = (LogType)enumerator.Current;

                bool hasFlag = (logType & flag) != 0;

                if (hasFlag)
                    yield return flag;
            }
            yield break;
        }

        private static IEnumerable<uint> powersOfTwo(uint startAt)
        {
            uint value = startAt;
            do
            {
                yield return value;
                value *= 2;
            }
            while (value < int.MaxValue);
            yield break;
        }
    }
}
