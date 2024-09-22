using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }

    public enum FilterDuration
    {
        OnClose,
        Always
    }
}
