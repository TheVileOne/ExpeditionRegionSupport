using LogUtils.Properties;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogUtils.Helpers.Extensions
{
    public static class StringBuilderExtensions
    {
        internal static void AppendComments(this StringBuilder sb, string commentOwner, List<CommentEntry> comments)
        {
            var applicableComments = comments.Where(entry => entry.Owner == commentOwner);

            foreach (string comment in applicableComments.Select(entry => entry.Message))
                sb.AppendLine(comment);
        }

        internal static void AppendPropertyString(this StringBuilder sb, string name, string value = "")
        {
            sb.AppendLine(LogProperties.ToPropertyString(name, value));
        }
    }
}
