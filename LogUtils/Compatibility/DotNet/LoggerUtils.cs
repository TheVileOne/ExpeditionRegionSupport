using LogUtils.Enums;
using Microsoft.Extensions.Logging;

namespace LogUtils.Compatibility.DotNet
{
    public static class LoggerUtils
    {
        public static LogCategory GetEquivalentCategory(LogLevel category)
        {
            return category switch
            {
                LogLevel.Trace or LogLevel.Debug => LogCategory.Debug,
                LogLevel.Information => LogCategory.Info,
                LogLevel.Warning => LogCategory.Warning,
                LogLevel.Error => LogCategory.Error,
                LogLevel.Critical => LogCategory.Fatal,
                LogLevel.None => LogCategory.None,
                _ => LogCategory.All,//Is this good behavior?
            };
        }
    }
}
