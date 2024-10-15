using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

namespace LogUtils.Properties
{
    internal class LogPropertyReader
    {
        private LogPropertyFile propertyFile;

        public LogPropertyReader(LogPropertyFile file)
        {
            propertyFile = file;
        }

        public IEnumerator<LogPropertyData> GetEnumerator()
        {
            string filePath = propertyFile.FilePath;

            if (!File.Exists(filePath))
                yield break;

            StringDictionary propertyInFile = null;
            foreach (string line in File.ReadAllLines(filePath).Select(l => l.Trim()))
            {
                if (line == string.Empty || line.StartsWith("//")) continue;

                string[] lineData = line.Split(':');

                if (lineData.Length > 1)
                {
                    string header = lineData[0];
                    string value = lineData[1];

                    if (header == UtilityConsts.DataFields.LOGID)
                    {
                        var lastProcessed = propertyInFile;

                        //Start a new entry for the next set of data fields
                        propertyInFile = new StringDictionary();
                        propertyInFile[header] = value;

                        if (lastProcessed != null)
                            yield return new LogPropertyData(lastProcessed); //Data collection has finished for the last entry - return the data
                    }

                    if (lineData.Length > 2) //This shouldn't be a thing, but lets handle it to be safe
                        value = line.Substring(line.IndexOf(":") + 1);

                    //Store each data field in a dictionary until all lines pertaining to the current property are read
                    propertyInFile[header] = value;
                }
            }

            //The loop has finished, but the last entry has not yet been returned
            if (propertyInFile != null)
                yield return new LogPropertyData(propertyInFile);
        }
    }
}
