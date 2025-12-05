using BepInEx.Logging;
using System;

namespace LogUtils.Enums
{
    /// <summary>
    /// Class for accessing the default <see cref="LogCategoryLevels"/>, and other internal functions for mapping <see cref="BepInEx.Logging.LogLevel"/> to an associated <see cref="LogCategoryLevels"/>
    /// </summary>
    public static class LogCategoryLevelMap
    {
        /// <summary>
        /// The value that is assigned before a more specific category level is applied
        /// </summary>
        public static LogCategoryLevels DefaultLevel = LogCategoryLevels.Info;

        /// <summary>
        /// Implementation doesn't account for composites
        /// </summary>
        internal static LogCategoryLevels GetEquivalent(in LogLevel logLevel, bool callingFromConstructor)
        {
            return logLevel switch
            {
                LogLevel.None => LogCategoryLevels.None,
                LogLevel.Fatal => LogCategoryLevels.Fatal,
                LogLevel.Error => LogCategoryLevels.Error,
                LogLevel.Warning => LogCategoryLevels.Warning,
                LogLevel.Message => LogCategoryLevels.Message,
                LogLevel.Info => LogCategoryLevels.Info,
                LogLevel.Debug => LogCategoryLevels.Debug,
                LogLevel.All => LogCategoryLevels.All,
                _ => callingFromConstructor ? DefaultLevel : LogCategory.ToCategory(logLevel).Level
            };
        }

        /// <summary>
        /// Implementation accounts for composites
        /// </summary>
        internal static LogCategoryLevels GetEquivalentSlow(in LogLevel logLevel)
        {
            if (logLevel.IsComposite())
            {
                LogLevel[] flags = logLevel.Deconstruct();

                //Take all of the flags from the first enum and stitch them back together into the enum we need
                LogCategoryLevels composite = LogCategoryLevels.None;
                for (int i = 0; i < flags.Length; i++)
                    composite |= GetEquivalent(flags[i], false);
                return composite;
            }
            return GetEquivalent(logLevel, false);
        }
    }

#pragma warning disable CS1591 //Missing XML comment for publicly visible type or member
    /// <summary>
    /// A slightly modified set of values based on <see cref="BepInEx.Logging.LogLevel"/>
    /// </summary>
    [Flags]
    public enum LogCategoryLevels
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
