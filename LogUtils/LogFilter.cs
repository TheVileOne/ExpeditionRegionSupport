using Expedition;
using System.Collections.Generic;

namespace LogUtils
{
    public static class LogFilter
    {
        /// <summary>
        /// List of filter keywords - A filter entry with a keyword must have that keyword in this list in order for the filter to be effective 
        /// </summary>
        public static List<string> ActiveKeywords = new List<string>();

        /// <summary>
        /// Dictionary of strings that should be ignored when a log attempt is made
        /// </summary>
        public static Dictionary<LogID, List<FilteredStringEntry>> FilteredStrings = new Dictionary<LogID, List<FilteredStringEntry>>();

        public static void AddFilterEntry(LogID logID, FilteredStringEntry entry)
        {
            if (!FilteredStrings.TryGetValue(logID, out List<FilteredStringEntry> filter)) //Ensure list is created
                filter = FilteredStrings[logID] = new List<FilteredStringEntry>();

            if (logID == LogID.Expedition)
                ExpLog.onceText.Add(entry.Value); //No longer used, but keep it updated for legacy purposes

            filter.Add(entry);
        }

        public static bool CheckFilterMatch(LogID logID, string logString)
        {
            if (FilteredStrings.TryGetValue(logID, out List<FilteredStringEntry> filter))
                return filter.Exists(entry => entry.CheckValidation() && entry.CheckMatch(logString));
            return false;
        }

        /// <summary>
        /// Designates a keyword as active by adding it to the ActiveKeywords list
        /// </summary>
        public static void ActivateKeyword(string keyword)
        {
            if (!ActiveKeywords.Contains(keyword))
                ActiveKeywords.Add(keyword);
        }

        /// <summary>
        /// Removes keyword from the ActiveKeywords list
        /// </summary>
        public static void DeactivateKeyword(string keyword)
        {
            ActiveKeywords.Remove(keyword);
        }
    }
}
