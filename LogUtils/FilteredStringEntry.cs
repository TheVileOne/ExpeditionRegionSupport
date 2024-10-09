using System.Collections.Generic;
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
            return Keywords.Count == 0 || Keywords.TrueForAll(LogFilter.ActiveKeywords.Contains); //All keywords for this filter must be active
        }
    }

    public enum FilterDuration
    {
        OnClose,
        Always
    }
}
