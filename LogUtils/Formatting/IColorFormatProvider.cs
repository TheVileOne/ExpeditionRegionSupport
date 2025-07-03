using System;
using System.Collections.Generic;
using System.Text;

namespace LogUtils.Formatting
{
    /// <summary>
    /// An interface for handling color data processed through a FormattableString
    /// </summary>
    public interface IColorFormatProvider : IFormatProvider, ICustomFormatter
    {
        /// <summary>
        /// List of format objects that contain color data
        /// </summary>
        List<FormatData> FormatObjects { get; }

        /// <summary>
        /// Resets the text color back to a default value
        /// </summary>
        void ResetColor(StringBuilder builder);
    }
}
