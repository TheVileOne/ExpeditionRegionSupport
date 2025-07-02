using System;
using System.Collections.Generic;

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
    }
}
