using System;
using System.Text;

namespace LogUtils.Formatting
{
    /// <summary>
    /// An interface for handling color data processed through a FormattableString
    /// </summary>
    public interface IColorFormatProvider : IFormatProvider, ICustomFormatter
    {
        /// <summary>
        /// Applies color format to a message string
        /// </summary>
        string ApplyFormat(string message, Color messageColor);

        /// <summary>
        /// Resets the text color back to a default value
        /// </summary>
        void ResetColor(StringBuilder builder, FormatData data);
    }
}
