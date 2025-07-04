using LogUtils.Console;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LogUtils.Formatting
{
    /// <summary>
    /// A type of ColorFormatProvider that replaces color data with ANSI color codes
    /// </summary>
    public class AnsiColorFormatProvider : IColorFormatProvider
    {
        private List<FormatData> _formatObjects = new List<FormatData>();

        /// <inheritdoc/>
        public List<FormatData> FormatObjects => _formatObjects;

        /// <inheritdoc/>
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg == null)
                return string.Empty;

            FormatData formatData = arg as FormatData;

            if (formatData != null && formatData.IsColorData)
            {
                FormatObjects.Add(formatData);
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
            builder.Insert(data.LocalPosition, AnsiColorConverter.AnsiReset);
        }
    }
}
