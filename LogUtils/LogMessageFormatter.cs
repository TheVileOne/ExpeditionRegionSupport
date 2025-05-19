using LogUtils.Events;
using LogUtils.Properties.Formatting;
using System.Linq;

namespace LogUtils
{
    public class LogMessageFormatter
    {
        public static LogMessageFormatter Default = new LogMessageFormatter();

        /// <summary>
        /// Formats message data into a log ready format
        /// </summary>
        public string Format(LogMessageEventArgs messageData)
        {
            string message = messageData.Message;
            var activeRules = messageData.Properties.Rules.Where(r => r.IsEnabled);

            foreach (LogRule rule in activeRules)
                rule.Apply(ref message, messageData);
            return message;
        }
    }
}
