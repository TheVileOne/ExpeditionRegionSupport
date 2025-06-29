using LogUtils.Console;
using LogUtils.Events;
using LogUtils.Properties.Formatting;
using System;
using System.Linq;
using UnityEngine;

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

            string message = formattable != null ? formattable.ToString(null, ColorFormatter)
                                                 : messageData.Message;

            message = ApplyColor(message, messageData.Category.ConsoleColor); //Message inherits the color associated with its category

            var activeRules = messageData.Rules.Where(r => r.IsEnabled);

            foreach (LogRule rule in activeRules)
                rule.Apply(this, ref message, messageData);
            return message;
        }

        /// <summary>
        /// Formats color data into a provided message string
        /// </summary>
        public virtual string ApplyColor(string message, Color messageColor)
        {
            if (ColorFormatter is EmptyColorFormatProvider)
                return message;

            if (ColorFormatter is ANSIColorFormatProvider)
                return AnsiColorConverter.ApplyFormat(message, messageColor);

            return string.Format(ColorFormatter, "{0}{1}{2}", messageColor, message, ConsoleColorMap.DefaultColor);
        }
    }
}
