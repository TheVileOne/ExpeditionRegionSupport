using Expedition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogUtils
{
    public static class LogFilter
    {
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
                return filter.Exists(entry => entry.Value == logString);
            return false;
        }
    }
}
