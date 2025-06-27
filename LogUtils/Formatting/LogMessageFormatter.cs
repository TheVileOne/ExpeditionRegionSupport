using LogUtils.Events;
using LogUtils.Properties.Formatting;
using System.Linq;

namespace LogUtils.Formatting
{
    public class LogMessageFormatter
    {
        public static LogMessageFormatter Default = new LogMessageFormatter();

        /// <summary>
        /// Formats message data into a log ready format
        /// </summary>
        public virtual string Format(LogRequestEventArgs messageData)
        {
            string message = messageData.Message;
            var activeRules = messageData.Rules.Where(r => r.IsEnabled);

            foreach (LogRule rule in activeRules)
                rule.Apply(ref message, messageData);
            return message;
        }
    }
}
