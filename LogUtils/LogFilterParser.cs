using LogUtils.Enums;
using LogUtils.Helpers;
using System.Collections.Generic;
using System.Globalization;
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

                    FilteredStringEntry entry = new FilteredStringEntry(filterString, FilterDuration.Always);

                    if (header.Keywords != null)
                        entry.Keywords.AddRange(header.Keywords);

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

            HeaderInfo headerInfo = new HeaderInfo();

            if (headerTokenDetected)
            {
                int headerEnd = line.IndexOf(']');

                if (headerEnd != -1)
                {
                    string[] headerData = line.Substring(0, headerEnd).Split(',');
                    string keywordPrefix = UtilityConsts.FilterKeywords.KEYWORD_PREFIX;

                    List<string> parsedKeywords = new List<string>();

                    bool hasData = false;
                    foreach (string value in headerData.Select(s => s.Trim()))
                    {
                        //Header data falls into two groups: LogID filenames/tags, and filter keywords. Filter keywords are distinguished by a special prefix
                        if (value.StartsWith(keywordPrefix, true, CultureInfo.InvariantCulture))
                        {
                            if (value.Length == keywordPrefix.Length) //Prevent index out of range exception
                                continue;

                            string keyword = value.Substring(keywordPrefix.Length);

                            if (EqualityComparer.StringComparerIgnoreCase.Equals(value, UtilityConsts.FilterKeywords.REGEX))
                                headerInfo.IsRegex = true; //This keyword is stored as a bool unlike other keywords
                            else
                                parsedKeywords.Add(keyword);
                            hasData = true;
                        }
                        else
                        {
                            IEnumerable<LogID> nameMatches = LogID.FindAll(value);
                            IEnumerable<LogID> tagMatches = LogID.FindByTag(value, null);

                            headerInfo.TargetIDs = nameMatches.Union(tagMatches).ToArray();

                            if (headerInfo.TargetIDs.Length > 0)
                                hasData = true;
                        }
                    }

                    if (hasData)
                    {
                        headerInfo.Length = headerEnd;
                        headerInfo.Keywords = parsedKeywords.ToArray();
                    }
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

            public string[] Keywords;
        }
    }
}
