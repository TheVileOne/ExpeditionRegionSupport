using LogUtils.Enums;
using Microsoft.Extensions.Logging;

namespace LogUtils.Compatibility.DotNet
{
    public static class LoggerUtils
    {
        /// <summary>
        /// Converts a LogLevel enum to its LogCategory equivalent value
        /// </summary>
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
                    return LogCategory.All; //Is this good behavior?
            }
        }
    }
}
