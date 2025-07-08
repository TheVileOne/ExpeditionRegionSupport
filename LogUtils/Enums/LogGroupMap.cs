using BepInEx.Logging;
using LogUtils.Helpers.Extensions;
using System;

namespace LogUtils.Enums
{
    public static class LogGroupMap
    {
        public static LogGroup DefaultGroup = LogGroup.Info;

        /// <summary>
        /// Implementation doesn't account for composites
        /// </summary>
        internal static LogGroup GetEquivalent(in LogLevel logLevel, bool callingFromConstructor)
        {
            return logLevel switch
            {
                LogLevel.None => LogGroup.None,
                LogLevel.Fatal => LogGroup.Fatal,
                LogLevel.Error => LogGroup.Error,
                LogLevel.Warning => LogGroup.Warning,
                LogLevel.Message => LogGroup.Message,
                LogLevel.Info => LogGroup.Info,
                LogLevel.Debug => LogGroup.Debug,
                LogLevel.All => LogGroup.All,
                _ => callingFromConstructor ? DefaultGroup : LogCategory.ToCategory(logLevel).Group
            };
        }

        /// <summary>
        /// Implementation accounts for composites
        /// </summary>
        internal static LogGroup GetEquivalentSlow(in LogLevel logLevel)
        {
            if (logLevel.IsComposite())
            {
                LogLevel[] flags = logLevel.Deconstruct();

                //Take all of the flags from the first enum and stitch them back together into the enum we need
                LogGroup composite = LogGroup.None;
                for (int i = 0; i < flags.Length; i++)
                    composite |= GetEquivalent(flags[i], false);
                return composite;
            }
            return GetEquivalent(logLevel, false);
        }
    }

#pragma warning disable CS1591 //Missing XML comment for publicly visible type or member
    /// <summary>
    /// A slightly modified set of values based on BepInEx.Logging.LogLevel
    /// </summary>
    public enum LogGroup
    {
        None = 0,
        Fatal = 1,
        Error = 2,
        Warning = 4,
        Assert = 8,
        Important = 16,
        Message = 32,
        Info = 64,
        Debug = 128,
        All = int.MaxValue
    }
#pragma warning restore CS1591 //Missing XML comment for publicly visible type or member
}
