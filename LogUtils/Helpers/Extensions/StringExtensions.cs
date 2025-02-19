using System.Collections.Generic;
using System.Linq;

namespace LogUtils.Helpers.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Evaluates whether a string is equal to any of the provided values
        /// </summary>
        /// <param name="str">The string to evaluate</param>
        /// <param name="comparer">An IEqualityComparer to use for the evaluation</param>
        /// <param name="values">The values to compare the string against</param>
        /// <returns>Whether a match was found</returns>
        public static bool MatchAny(this string str, IEqualityComparer<string> comparer, params string[] values)
        {
            return values.Contains(str, comparer);
        }
    }
}
