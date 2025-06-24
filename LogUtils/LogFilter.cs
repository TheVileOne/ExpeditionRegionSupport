using Expedition;
using LogUtils.Enums;
using LogUtils.Events;
using System.Collections.Generic;

namespace LogUtils
{
    public static class LogFilter
    {
        /// <summary>
        /// List of filter keywords - A filter entry with a keyword must have that keyword in this list in order for the filter to be effective 
        /// </summary>
        public static List<string> ActiveKeywords = [];

        /// <summary>
        /// Dictionary of strings that should be ignored when a log attempt is made
        /// </summary>
        public static Dictionary<LogID, List<FilteredStringEntry>> FilteredStrings = [];

        public static void AddFilterEntry(LogID logID, FilteredStringEntry entry)
        {
            if (!FilteredStrings.TryGetValue(logID, out List<FilteredStringEntry> filter)) //Ensure list is created
                filter = FilteredStrings[logID] = [];

            if (logID == LogID.Expedition)
                ExpLog.onceText.Add(entry.Value); //No longer used, but keep it updated for legacy purposes

            filter.Add(entry);
        }

        public static bool IsAllowed(LogRequestEventArgs messageData)
        {
            var categoryFilter = LogCategory.GlobalFilter;

            return (categoryFilter == null || categoryFilter.IsAllowed(messageData.Category)) && IsAllowed(messageData.ID, messageData.Message);
        }

        public static bool IsAllowed(LogID logID, string logString)
        {
            bool isFiltered = false;
            if (FilteredStrings.TryGetValue(logID, out List<FilteredStringEntry> filter))
                isFiltered = filter.Exists(entry => entry.CheckValidation() && entry.CheckMatch(logString));
            return !isFiltered;
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
