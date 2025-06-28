using System;
using System.Collections.Generic;

namespace LogUtils.Formatting
{
    /// <summary>
    /// A type of ColorFormatProvider that replaces color data with empty strings
    /// </summary>
    public class EmptyColorFormatProvider : IColorFormatProvider
    {
        private List<ColorPlaceholder> _formatObjects = new List<ColorPlaceholder>();

        /// <inheritdoc/>
        public List<ColorPlaceholder> FormatObjects => _formatObjects;

        /// <inheritdoc/>
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg == null)
                return string.Empty;

            ColorPlaceholder colorData = arg as ColorPlaceholder;

            if (colorData != null)
            {
                FormatObjects.Add(colorData);
                return string.Empty;
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
    }
}
