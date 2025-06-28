using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        List<ColorPlaceholder> FormatObjects { get; }
    }
}
