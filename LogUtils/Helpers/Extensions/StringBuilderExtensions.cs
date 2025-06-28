using LogUtils.Properties;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace LogUtils.Helpers.Extensions
{
    public static partial class ExtensionMethods
    {
        internal const string DIVIDER = "--------------------------------------------";

        /// <summary>
        /// Appends a message in between divider spacers
        /// </summary>
        /// <param name="builder">The builder to append to</param>
        /// <param name="header">The content to use as a header</param>
        public static StringBuilder AppendHeader(this StringBuilder builder, string header)
        {
            return builder.AppendLine(DIVIDER)
                          .AppendLine(header)
                          .AppendLine(DIVIDER);
        }

        internal static StringBuilder AppendComments(this StringBuilder builder, string commentOwner, List<CommentEntry> comments)
        {
            var applicableComments = comments.Where(entry => entry.Owner == commentOwner);

            foreach (string comment in applicableComments.Select(entry => entry.Message))
                builder.AppendLine(comment);
            return builder;
        }

        internal static StringBuilder AppendPropertyString(this StringBuilder builder, string name, string value = "")
        {
            return builder.AppendLine(LogProperties.ToPropertyString(name, value));
        }

        private static readonly ConditionalWeakTable<StringBuilder, StringBuilderCWT> builderCWT = new();

        internal static StringBuilderCWT GetData(this StringBuilder self) => builderCWT.GetValue(self, _ => new());

        internal class StringBuilderCWT
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

        internal struct FormatData
        {
            //The index in the argument array associated with the placeholder 
            public int ArgumentIndex;

            /// <summary>
            /// The char index of the left curly brace of a format placeholder
            /// </summary>
            public int Position;

            /// <summary>
            /// The string representation of the format placeholder
            /// </summary>
            public string Placeholder;
        }
    }
}
