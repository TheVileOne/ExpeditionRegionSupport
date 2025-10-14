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

            DateTimeFormat format = logEventData.Properties.DateTimeFormat;
            string messageHeader = string.Format("{0} ", logEventData.IsTargetingConsole
                ? DateTime.Now.ToLongTimeString()
                : DateTime.Now.ToString(format.FormatString, format.FormatProvider));

            messageHeader = formatter.ApplyColor(messageHeader, headerColor);
            return messageHeader + message;
        }

        /// <inheritdoc/>
        protected override float GetPriority()
        {
            return 0.997f;
        }
    }
}
