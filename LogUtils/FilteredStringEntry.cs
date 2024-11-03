using LogUtils.Helpers.Comparers;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace LogUtils
{
    public struct FilteredStringEntry
    {
        public FilterDuration Duration;
        public bool IsRegex;
        public List<string> Keywords = new List<string>();
        public string Value;

        public FilteredStringEntry(string entryString, FilterDuration duration)
        {
            Duration = duration;
            Value = entryString;
        }

        public FilteredStringEntry(string entryString, FilterDuration duration, bool useAsRegexPattern) : this(entryString, duration)
        {
            IsRegex = useAsRegexPattern;
        }

        public bool CheckMatch(string testString)
        {
            if (IsRegex)
            {
                Regex regex = new Regex(Value);
                return regex.Match(testString).Success;
            }
            return string.Equals(Value, testString);
        }

        /// <summary>
        /// Checks whether the filter is currently applicable
        /// </summary>
        /// <returns>true, when applicable, false otherwise</returns>
        public bool CheckValidation()
        {
            //time_ prefix describes activation range keywords
            return Keywords.Count == 0 || Keywords.TrueForAll(k =>
            {
                return !k.StartsWith("time_", true, CultureInfo.InvariantCulture) || LogFilter.ActiveKeywords.Contains(k, ComparerUtils.StringComparerIgnoreCase);
            });
        }
    }

    public enum FilterDuration
    {
        OnClose,
        Always
    }
}
