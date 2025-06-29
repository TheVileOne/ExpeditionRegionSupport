using UnityEngine;

namespace LogUtils.Formatting
{
    /// <summary>
    /// A data class for handling color formats
    /// </summary>
    public class ColorPlaceholder
    {
        private FormatData _data;

        /// <summary>
        /// The color to use
        /// </summary>
        public Color Color;

        /// <summary>
        /// The index position within a string where the color will be used
        /// </summary>
        public int StartIndex => _data.Position;

        /// <summary>
        /// The number of characters beginning at the start index that should use this color, if set to 0, or less, color will last until string terminates or another
        /// color placeholder is encountered
        /// </summary>
        public int Length => _data.CommaArgument;

        /// <summary>
        /// Creates a new ColorPlaceholder object
        /// </summary>
        internal ColorPlaceholder(Color color, FormatData data)
        {
            Color = color;
            _data = data;
        }
    }
}
