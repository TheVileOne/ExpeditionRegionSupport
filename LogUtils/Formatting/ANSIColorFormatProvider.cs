using LogUtils.Console;
using System;
using System.Text;
using UnityEngine;

namespace LogUtils.Formatting
{
    /// <summary>
    /// A type of ColorFormatProvider that replaces color data with ANSI color codes
    /// </summary>
    public class AnsiColorFormatProvider : IColorFormatProvider
    {
        /// <inheritdoc/>
        public Color DefaultMessageColor { get; set; }

        public string ApplyFormat(string message, Color messageColor)
        {
            if (!LogConsole.ANSIColorSupport)
                return message;
            return AnsiColorConverter.ApplyFormat(message, messageColor);
        }

        /// <inheritdoc/>
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg == null)
                return string.Empty;

            FormatData formatData = arg as FormatData;

            if (formatData != null && formatData.IsColorData)
            {
                if (!LogConsole.ANSIColorSupport)
                    return string.Empty;
                return AnsiColorConverter.AnsiToForeground((Color)formatData.Argument);
            }

            IFormattable formattableArg = arg as IFormattable;

            if (formattableArg != null)
                return formattableArg.ToString(format, formatProvider);

            return arg.ToString();
        }

        /// <inheritdoc/>
        public object GetFormat(Type formatType)
        {
            //Determine whether custom formatting object is requested
            if (formatType == typeof(ICustomFormatter))
                return this;
            return null;
        }

        /// <inheritdoc/>
        public void ResetColor(StringBuilder builder, FormatData data)
        {
            UtilityLogger.DebugLog("COLOR RESET");
            UtilityLogger.DebugLog($"Inserting reset code at index {data.LocalPosition} - Actual position {data.Position}");

            if (!LogConsole.ANSIColorSupport)
                return;
            builder.Insert(data.LocalPosition, AnsiColorConverter.AnsiToForeground(DefaultMessageColor));
        }
    }
}
