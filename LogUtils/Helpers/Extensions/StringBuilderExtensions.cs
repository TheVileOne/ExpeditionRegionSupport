using LogUtils.Properties;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogUtils
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
    }
}
