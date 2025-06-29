using LogUtils.Events;
using LogUtils.Formatting;

namespace LogUtils.Properties.Formatting
{
    internal class BepInExHeaderRule : ShowCategoryRule
    {
        public BepInExHeaderRule(bool enabled) : base(enabled)
        {
        }

        protected override string ApplyRule(LogMessageFormatter formatter, string message, LogRequestEventArgs logEventData)
        {
            return string.Format("[{0,-7}:{1,10}] {2}", logEventData.Category, logEventData.LogSource?.SourceName ?? "Unknown", message);
        }
    }
}
