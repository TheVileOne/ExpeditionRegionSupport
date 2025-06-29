using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LogUtils.Formatting
{
    internal struct FormatData
    {
        /// <summary>
        /// The format argument
        /// </summary>
        public object Argument;

        /// <summary>
        /// The index in the argument array associated with the format placeholder 
        /// </summary>
        public int ArgumentIndex;

        /// <summary>
        /// The value that appears after a comma in the format placeholder
        /// </summary>
        /// <remarks>Example {0,4}</remarks>
        public int CommaArgument;

        /// <summary>
        /// The index of the first character of the format placeholder in the format string
        /// </summary>
        public int Position;

        /// <summary>
        /// The string representation of the format placeholder
        /// </summary>
        public string Format;
    }

    internal static class FormatDataCWT
    {
        private static readonly ConditionalWeakTable<IColorFormatProvider, Data> colorFormatCWT = new();

        internal static Data GetData(this IColorFormatProvider self) => colorFormatCWT.GetValue(self, _ => new());

        internal class Data
        {
            /// <summary>
            /// The current placeholder state
            /// </summary>
            public FormatData CurrentPlaceholder;

            /// <summary>
            /// A list of format placeholders collected over the course of a format operation
            /// </summary>
            /// <remarks>This list is cleared before, and after a format operation completes</remarks>
            public List<FormatData> PlaceholderData = new List<FormatData>();
        }
    }
}
