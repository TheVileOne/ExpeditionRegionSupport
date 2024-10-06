using LogUtils.Properties;
using System;
using System.Linq;
using System.Text;

namespace LogUtils.Helpers
{
    internal static class Extensions
    {
        /// <summary>
        /// Evaluates whether a string is equal to any of the provided values
        /// </summary>
        /// <param name="str">The string to evaluate</param>
        /// <param name="comparer">A StringComparer to use for the evaluation</param>
        /// <param name="values">The values to compare the string against</param>
        /// <returns>Whether a match was found</returns>
        public static bool MatchAny(this string str, StringComparer comparer, params string[] values)
        {
            return values.Contains(str, comparer);
        }

        public static void AppendPropertyString(this StringBuilder sb, string name, string value = "")
        {
            sb.AppendLine(LogProperties.ToPropertyString(name, value));
        }
    }
}
