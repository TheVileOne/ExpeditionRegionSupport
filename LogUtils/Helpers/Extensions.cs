using BepInEx.Logging;
using LogUtils.Diagnostics;
using LogUtils.Enums;
using LogUtils.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LogUtils.Helpers
{
    public static class Extensions
    {    
        public static bool IsComposite(this LogLevel logLevel)
        {
            int value = (int)logLevel;

            //Two's complement encoding allows us to detect multiple flags using this bit operation
            return value > 0 && (value & -value) != value;
        }

        /// <summary>
        /// Evaluates whether the enum is composed of two or more distinct flagged values
        /// </summary>
        public static bool IsComposite(this LogType logType)
        {
            LogType? flags = logType.GetFlags();
            int value = (int)flags;

            //Two's complement encoding allows us to detect multiple flags using this bit operation
            return value > 0 && (value & -value) != value;
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

            List<LogLevel> composition = new List<LogLevel>();

            var enumerator = powersOfTwo().GetEnumerator();

            //Cycle through powers of 2, recording all set flags
            while (enumerator.MoveNext())
            {
                LogLevel flag = (LogLevel)enumerator.Current;

                bool hasFlag = (logLevel & flag) != 0;

                if (hasFlag)
                    composition.Add(flag);
            }
            return composition.ToArray();
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

            List<LogType> composition = new List<LogType>();

            //Check for values in the non-flag part of the array
            byte firstByte = BitConverter.GetBytes((int)logType)[0];

            if (firstByte != 0)
            {
                UtilityLogger.Log("Composite log enum exists with non-composite values");
                composition.Add((LogType)firstByte);
            }

            //Composition cannot includes values < 256, because Unity does not support bit flags for this enum type
            //Any composite values will exclusively be set via the custom conversion offset
            var enumerator = powersOfTwo()
                             .Skip(LogCategory.CONVERSION_OFFSET_POWER - 1)
                             .GetEnumerator();

            //Cycle through powers of 2, recording all set flags
            while (enumerator.MoveNext())
            {
                LogType flag = (LogType)enumerator.Current;

                bool hasFlag = (logType & flag) != 0;

                //TODO: Check for non-composite equivalents here?
                if (hasFlag)
                    composition.Add(flag);
            }
            return composition.ToArray();
        }

        /// <summary>
        /// Extracts any bitflag compatible values, masking all others
        /// </summary>
        public static LogType? GetFlags(this LogType logType)
        {
            int value = (int)logType;

            if (value < LogCategory.CONVERSION_OFFSET)
                return null;

            int skipOverBits = LogCategory.CONVERSION_OFFSET_POWER;

            //Remove the bits we know are not flags
            value = (value >> skipOverBits) << skipOverBits;

            return value > 0 ? (LogType)value : null;
        }

        private static IEnumerable<uint> powersOfTwo()
        {
            uint value = 1; //2^0
            do
            {
                yield return value;
                value *= 2;
            }
            while (value < int.MaxValue);
            yield break;
        }

        /// <summary>
        /// Evaluates whether a string is equal to any of the provided values
        /// </summary>
        /// <param name="str">The string to evaluate</param>
        /// <param name="comparer">An IEqualityComparer to use for the evaluation</param>
        /// <param name="values">The values to compare the string against</param>
        /// <returns>Whether a match was found</returns>
        public static bool MatchAny(this string str, IEqualityComparer<string> comparer, params string[] values)
        {
            return values.Contains(str, comparer);
        }

        internal static void AppendComments(this StringBuilder sb, string commentOwner, List<CommentEntry> comments)
        {
            var applicableComments = comments.Where(entry => entry.Owner == commentOwner);

            foreach (string comment in applicableComments.Select(entry => entry.Message))
                sb.AppendLine(comment);
        }

        internal static void AppendPropertyString(this StringBuilder sb, string name, string value = "")
        {
            sb.AppendLine(LogProperties.ToPropertyString(name, value));
        }

        public static ResultAnalyzer GetAnalyzer(this IEnumerable<Condition.Result> results)
        {
            return new ResultAnalyzer(results);
        }

        public static StreamResumer[] InterruptAll<T>(this IEnumerable<T> handles) where T : PersistentFileHandle
        {
            //For best results, this should be treated as a critical section
            return handles.Where(handle => !handle.WaitingToResume)   //Retrieve entries that are available to interrupt
                          .Select(handle => handle.InterruptStream()) //Interrupt filestreams and collect resume handles
                          .ToArray();
        }

        public static void ResumeAll(this IEnumerable<StreamResumer> streams)
        {
            foreach (StreamResumer stream in streams)
                stream.Resume();
        }
    }
}
