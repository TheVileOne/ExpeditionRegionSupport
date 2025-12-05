using LogUtils.Enums;
using Microsoft.Extensions.Logging;

namespace LogUtils.Compatibility.DotNet
{
    public static class LoggerUtils
    {
        /// <summary>
        /// Converts a <see cref="LogLevel"/> value to its equivalent <see cref="LogCategory"/> instance
        /// </summary>
        /// <returns>Default <see cref="LogCategory"/> instance will be returned if value provided is not a recognized <see cref="LogLevel"/> value</returns>
        public static LogCategory GetEquivalentCategory(LogLevel category)
        {
            switch (category)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    return LogCategory.Debug;
                case LogLevel.Information:
                    return LogCategory.Info;
                case LogLevel.Warning:
                    return LogCategory.Warning;
                case LogLevel.Error:
                    return LogCategory.Error;
                case LogLevel.Critical:
                    return LogCategory.Fatal;
                case LogLevel.None:
                    return LogCategory.None;
                default:
                    return LogCategory.Default;
            }
        }
    }
}
