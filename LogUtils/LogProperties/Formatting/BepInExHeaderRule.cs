using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Formatting;
using UnityEngine;

namespace LogUtils.Properties.Formatting
{
    internal class BepInExHeaderRule : ShowCategoryRule
    {
        public BepInExHeaderRule(bool enabled) : base(enabled)
        {
        }

        /// <inheritdoc/>
        protected override string ApplyRule(LogMessageFormatter formatter, string message, LogRequestEventArgs logEventData)
        {
            LogCategory category = logEventData.Category;
            string sourceName = logEventData.LogSource?.SourceName ?? "Unknown";

            Color headerColor = category.ConsoleColor;

            string messageHeader = string.Format("[{0,-7}:{1,10}] ", category, sourceName);

            messageHeader = formatter.ApplyColor(messageHeader, headerColor);
            return messageHeader + message;
        }
    }
}
