using LogUtils.Helpers;
using LogUtils.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LogUtils
{
    public static class LogFilterParser
    {
        public static void ParseFile()
        {
            string filterPath = Path.Combine(Paths.StreamingAssetsPath, "logfilter.txt");

            if (!File.Exists(filterPath)) return;

            var stringComparer = EqualityComparer.StringComparerIgnoreCase;

            foreach (string line in File.ReadAllLines(filterPath))
            {
                string[] lineData = line.Split('[', ']');

                if (lineData.Length <= 1)
                {
                    LogFilter.AddFilterEntry(LogID.Unity, new FilteredStringEntry(line.Trim('"', ' '), FilterDuration.Always));
                    continue;
                }

                List<LogID> parsedLogIDs = null;
                bool isRegexPattern = false;
                SetupPeriod? filterActivePeriod = null;

                int dataIndex = 0;
                while (dataIndex < lineData.Length)
                {
                    string data = lineData[dataIndex].Trim();

                    if (parsedLogIDs == null)
                    {
                        parsedLogIDs = new List<LogID>();
                        string[] logNames = data.Split(',');

                        foreach (string name in logNames.Select(l => l.Trim()))
                        {
                            foreach (var properties in LogProperties.PropertyManager.Properties)
                            {
                                if (name.MatchAny(stringComparer, properties.ID.value, properties.Filename, properties.AltFilename))
                                    parsedLogIDs.Add(properties.ID);
                            }
                        }

                        //Default to Unity log when no LogID is specified
                        if (parsedLogIDs.Count == 0)
                        {
                            parsedLogIDs.Add(LogID.Unity);
                            continue;
                        }
                    }
                    else if (data.Length == 1)
                    {
                        isRegexPattern = stringComparer.Equals(data, "r");
                    }
                    else if (Enum.TryParse(data, out SetupPeriod activePeriod))
                    {
                        filterActivePeriod = activePeriod;
                    }

                    dataIndex++;

                    if (dataIndex == lineData.Length) //Last array index must contain the filter string
                        dataIndex = lineData.Length;
                }

                string filterString = lineData[lineData.Length - 1].Trim('"');

                foreach (LogID logID in parsedLogIDs)
                {
                    LogFilter.AddFilterEntry(logID, new FilteredStringEntry(filterString, FilterDuration.Always)
                    {
                        IsRegex = isRegexPattern
                    });
                }
            }
        }
    }
}
