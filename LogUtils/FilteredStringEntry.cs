using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LogUtils
{
    public struct FilteredStringEntry
    {
        public FilterDuration Duration;
        public bool IsRegex;
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
    }

    public enum FilterDuration
    {
        OnClose,
        Always
    }
}
