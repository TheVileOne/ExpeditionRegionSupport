using LogUtils.Enums;
using LogUtils.Events;

namespace LogUtils.Properties.Formatting
{
    public class ErrorsOnlyHeaderRule : ShowCategoryRule
    {
        public ErrorsOnlyHeaderRule(bool enabled) : base(enabled)
        {
        }

        protected override string ApplyRule(string message, LogMessageEventArgs logEventData)
        {
            if (!LogCategory.IsErrorCategory(logEventData.Category))
                return message;

            return base.ApplyRule(message, logEventData);
        }
    }
}
