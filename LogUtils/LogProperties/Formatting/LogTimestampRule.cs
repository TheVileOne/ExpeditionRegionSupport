using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Formatting;
using System;
using System.Runtime.Serialization;
using UnityEngine;

namespace LogUtils.Properties.Formatting
{
    public class LogTimestampRule : LogRule
    {
        public LogTimestampRule(bool enabled) : base(UtilityConsts.DataFields.Rules.LOG_TIMESTAMP, enabled)
        {
        }

        /// <inheritdoc/>
        protected override string ApplyRule(LogMessageFormatter formatter, string message, LogRequestEventArgs logEventData)
        {
            LogCategory category = logEventData.Category;
            Color headerColor = category.ConsoleColor;

            DateTime time = GetTime(logEventData);
            DateTimeFormat format = logEventData.Properties.DateTimeFormat;
            string messageHeader = string.Format("{0} ", logEventData.IsTargetingConsole
                ? time.ToLongTimeString()
                : time.ToString(format.FormatString, format.FormatProvider));

            messageHeader = formatter.ApplyColor(messageHeader, headerColor);
            return messageHeader + message;
        }

        /// <summary>
        /// Gets a <see cref="DateTime"/> instance to use as a timestamp
        /// </summary>
        protected DateTime GetTime(LogRequestEventArgs logEventData)
        {
            var timeData = logEventData.FindData<TimeEventArgs>();

            if (timeData != null)
                return timeData.Time;

            //When logging to file, or the console, we want output to reflect local system time
            return DateTime.Now;
        }

        /// <inheritdoc/>
        protected override float GetPriority()
        {
            return 0.997f;
        }
    }
}
