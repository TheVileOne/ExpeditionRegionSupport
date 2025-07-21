using BepInEx.Logging;
using LogUtils.Enums;
using System;
using System.Linq;
using UnityEngine;

namespace LogUtils.Helpers.Extensions
{
    public static partial class ExtensionMethods
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
            if (LogCategory.IsAllCategory(logLevel))
                return [LogLevel.All];

            if (!logLevel.IsComposite())
                return [logLevel];

            return FlagUtils.ToFlagEnumerable(logLevel).ToArray();
        }

        /// <summary>
        /// Deconstructs an enum into its flagged values
        /// </summary>
        public static LogType[] Deconstruct(this LogType logType)
        {
            if (LogCategory.IsAllCategory(logType))
                return [LogCategory.All.UnityCategory];

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
        /// <param name="logType">The LogType to evaluate</param>
        /// <returns>A LogType only containing bits within the conversion value range if any are present; otherwise defaults to -1</returns>
        public static LogType GetFlags(this LogType logType)
        {
            int value = (int)logType;

            if (!FlagUtils.HasConvertedFlags(value))
            {
                value = -1;
                return (LogType)value;
            }

            int skipOverBits = LogCategory.CONVERSION_OFFSET_POWER;

            //Remove the bits we know are not flags
            value = (value >> skipOverBits) << skipOverBits;
            return (LogType)value;
        }
    }
}
