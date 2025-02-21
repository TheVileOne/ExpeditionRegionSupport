using BepInEx.Logging;
using LogUtils.Enums;
using System;
using System.Linq;
using UnityEngine;

namespace LogUtils.Helpers.Extensions
{
    public static class LogEnumExtensions
    {
        public static bool HasConvertedFlags(this LogLevel logLevel)
        {
            return FlagUtils.HasConvertedFlags((int)logLevel);
        }

        public static bool HasConvertedFlags(this LogType logType)
        {
            return FlagUtils.HasConvertedFlags((int)logType);
        }

        public static bool IsComposite(this LogLevel logLevel)
        {
            return FlagUtils.HasMultipleFlags((int)logLevel);
        }

        /// <summary>
        /// Evaluates whether the enum is composed of two or more distinct flagged values
        /// </summary>
        public static bool IsComposite(this LogType logType)
        {
            return FlagUtils.HasMultipleFlags((int)logType.GetFlags());
        }

        /// <summary>
        /// Deconstructs an enum into its flagged values
        /// </summary>
        public static LogLevel[] Deconstruct(this LogLevel logLevel)
        {
            var allFlag = LogCategory.All.BepInExCategory;

            //There are two values reserved for this flag. Check both
            if ((logLevel & (allFlag | (LogLevel)LogCategory.All.FlagValue)) != 0)
                return [allFlag];

            if (!logLevel.IsComposite())
                return [logLevel];

            return FlagUtils.ToFlagEnumerable(logLevel).ToArray();
        }

        /// <summary>
        /// Deconstructs an enum into its flagged values
        /// </summary>
        public static LogType[] Deconstruct(this LogType logType)
        {
            var allFlag = LogCategory.All.UnityCategory;

            if ((logType & allFlag) != 0)
                return [allFlag];

            if (!logType.IsComposite())
                return [logType];

            var flagsEnumerable = FlagUtils.ToFlagEnumerable(logType);

            //Check for values in the non-flag part of the array, hopefully this never happens
            byte firstByte = BitConverter.GetBytes((int)logType)[0];

            if (firstByte != 0)
            {
                UtilityLogger.Log("Composite log enum exists with non-composite values");

                flagsEnumerable = [(LogType)firstByte, .. flagsEnumerable];
            }
            return flagsEnumerable.ToArray();
        }

        /// <summary>
        /// Extracts any bitflag compatible values, masking all others
        /// </summary>
        public static LogType? GetFlags(this LogType logType)
        {
            int value = (int)logType;

            if (!FlagUtils.HasConvertedFlags(value))
                return null;

            int skipOverBits = LogCategory.CONVERSION_OFFSET_POWER;

            //Remove the bits we know are not flags
            value = (value >> skipOverBits) << skipOverBits;

            return value > 0 ? (LogType)value : null;
        }
    }
}
