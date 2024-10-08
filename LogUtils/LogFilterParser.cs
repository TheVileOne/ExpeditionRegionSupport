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

            if (File.Exists(filterPath))
            {
                foreach (string line in File.ReadAllLines(filterPath))
                {
                    HeaderInfo header = getHeaderData(line);

                    string filterString = header.Length == 0 ? line : line.Substring(header.Length + 1);
                    FilterDuration duration = header.ExpectedLifetime == SetupPeriod.PostMods ? FilterDuration.Always : FilterDuration.UseLifetime;

                    FilteredStringEntry entry = new FilteredStringEntry(filterString, duration);

                    if (duration == FilterDuration.UseLifetime)
                        entry.ExpedtedLifetime = header.ExpectedLifetime;

                    foreach (LogID logID in header.TargetIDs)
                        LogFilter.AddFilterEntry(logID, entry);
                }
            }
        }

        /// <summary>
        /// Parses out data necessary to process the filter
        /// </summary>
        private static HeaderInfo getHeaderData(string line)
        {
            //This doesn't guarantee that data is a header, we must validate the data to be sure
            bool headerTokenDetected = line.StartsWith("[");

            HeaderInfo headerInfo = new HeaderInfo()
            {
                ExpectedLifetime = SetupPeriod.PostMods
            };

            if (headerTokenDetected)
            {
                int headerEnd = line.IndexOf(']');

                if (headerEnd != -1)
                {
                    string[] headerData = line.Substring(0, headerEnd).Split(',');

                    bool hasData = false;
                    foreach (string value in headerData.Select(s => s.Trim()))
                    {
                        if (EqualityComparer.StringComparerIgnoreCase.Equals(value, "regex"))
                        {
                            headerInfo.IsRegex = true;
                            hasData = true;
                        }
                        else if (Enum.TryParse(value, true, out SetupPeriod period))
                        {
                            headerInfo.ExpectedLifetime = period;
                            hasData = true;
                        }
                        else
                        {
                            IEnumerable<LogID> nameMatches = LogID.FindAll(value, null);
                            IEnumerable<LogID> tagMatches = LogID.FindByTag(value, null);

                            headerInfo.TargetIDs = nameMatches.Union(tagMatches).ToArray();

                            if (headerInfo.TargetIDs.Length > 0)
                                hasData = true;
                        }
                    }

                    if (hasData)
                        headerInfo.Length = headerEnd;
                }
            }

            //A default LogID is used when one is not specified
            if (headerInfo.TargetIDs == null || headerInfo.TargetIDs.Length == 0)
                headerInfo.TargetIDs = new[] { LogID.Unity };
            return headerInfo;
        }

        private struct HeaderInfo
        {
            public int Length;

            public LogID[] TargetIDs;

            public bool IsRegex;

            /// <summary>
            /// The latest time when the filter will be functional
            /// </summary>
            public SetupPeriod ExpectedLifetime;
        }
    }
}
