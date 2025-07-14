using System;
using System.Text;
using UnityEngine;

namespace LogUtils.Formatting
{
    /// <summary>
    /// A type of ColorFormatProvider that replaces color data with empty strings
    /// </summary>
    public class EmptyColorFormatProvider : IColorFormatProvider
    {
        /// <inheritdoc/>
        public Color? MessageColor { get; set; }

        /// <inheritdoc/>
        public string ApplyFormat(string message, Color messageColor)
        {
            return message;
        }

        /// <inheritdoc/>
        public virtual string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg == null)
                return string.Empty;

            FormatData formatData = arg as FormatData;

            if (formatData != null && formatData.IsColorData)
                return string.Empty;

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
        }
    }
}
