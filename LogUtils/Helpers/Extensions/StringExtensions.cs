using System.Collections.Generic;
using System.Linq;

namespace LogUtils
{
    public static partial class ExtensionMethods
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

        public static string ReplaceAll(this string str, char[] replaceTargets, char replaceWith)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            int index = -1;
            do
            {
                index = str.IndexOfAny(replaceTargets, ++index);

                if (index != -1) //Optimized with the expectation that a replace is rarely necessary
                {
                    char[] chars = str.ToCharArray();
                    chars[index] = replaceWith;
                    str = new string(chars);
                }
            }
            while (index != -1 && index < str.Length - 1);
            return str;
        }
    }
}
