using System.Text;

namespace LogUtils.Helpers
{
    public static class FormatUtils
    {
        public const string DIVIDER = "--------------------------------------------";

        public static string CreateHeader(StringBuilder builder, string header)
        {
            return builder.AppendLine(DIVIDER)
                          .AppendLine(header)
                          .AppendLine(DIVIDER)
                          .ToString();
        }
    }
}
