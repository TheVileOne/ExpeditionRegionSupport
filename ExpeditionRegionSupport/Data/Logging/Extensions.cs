using System.Text;

namespace ExpeditionRegionSupport.Data.Logging
{
    internal static class Extensions
    {
        public static void AppendPropertyString(this StringBuilder sb, string name, string value = "")
        {
            sb.AppendLine(LogProperties.ToPropertyString(name, value));
        }
    }
}
