using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Formatting;

namespace LogUtils.Properties.Formatting
{
    public class ErrorsOnlyHeaderRule : ShowCategoryRule
    {
        public ErrorsOnlyHeaderRule(bool enabled) : base(enabled)
        {
        }

        /// <inheritdoc/>
        protected override string ApplyRule(LogMessageFormatter formatter, string message, LogRequestEventArgs logEventData)
        {
            if (!LogCategory.IsErrorCategory(logEventData.Category))
                return message;

            return base.ApplyRule(formatter, message, logEventData);
        }
    }
}
