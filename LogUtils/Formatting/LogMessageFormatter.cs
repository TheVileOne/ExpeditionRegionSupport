using LogUtils.Events;
using LogUtils.Properties.Formatting;
using System;
using System.Linq;

namespace LogUtils.Formatting
{
    /// <summary>
    /// Applies format logic to message strings
    /// </summary>
    public class LogMessageFormatter
    {
        /// <summary>
        /// The default implementation that applies to most write implementations
        /// </summary>
        public static LogMessageFormatter Default = new LogMessageFormatter();

        /// <summary>
        /// Applies color-based message format changes
        /// </summary>
        public IColorFormatProvider ColorFormatter;

        /// <summary>
        /// Constructs a LogMessageFormatter instance
        /// </summary>
        public LogMessageFormatter() : this(new EmptyColorFormatProvider())
        {
        }

        /// <summary>
        /// Constructs a LogMessageFormatter instance
        /// </summary>
        /// <param name="colorFormatter">Determines how color format arguments are handled</param>
        public LogMessageFormatter(IColorFormatProvider colorFormatter)
        {
            ColorFormatter = colorFormatter;
        }

        /// <summary>
        /// Formats message data into a log ready format
        /// </summary>
        public virtual string Format(LogRequestEventArgs messageData)
        {
            IFormattable formattable = messageData.MessageObject as IFormattable;

            string message = formattable != null
                           ? formattable.ToString(null, ColorFormatter)
                           : messageData.Message;

            var activeRules = messageData.Rules.Where(r => r.IsEnabled);

            foreach (LogRule rule in activeRules)
                rule.Apply(this, ref message, messageData);
            return message;
        }
    }
}
