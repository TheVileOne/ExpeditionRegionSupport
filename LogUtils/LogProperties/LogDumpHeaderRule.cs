using LogUtils.Enums;
using LogUtils.Events;

namespace LogUtils.Properties
{
    public class LogDumpHeaderRule : LogRule
    {
        public LogDumpHeaderRule(bool enabled) : base(UtilityConsts.DataFields.Rules.LOG_DUMP, enabled)
        {
        }

        protected override string ApplyRule(string message, LogMessageEventArgs logEventData)
        {
            LogID reportID = extractReportID(logEventData);
            return string.Format(LogRequest.StringFormat, reportID, message);
        }

        private LogID extractReportID(LogMessageEventArgs logEventData)
        {
            LogEventArgs originalEventData = logEventData.ExtraArgs.Find(args => args.Tag == EventTag.Utility);
            return originalEventData.ID;
        }

        protected override float GetPriority()
        {
            return 0.75f;
        }
    }
}
