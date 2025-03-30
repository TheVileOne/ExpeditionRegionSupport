using LogUtils.Enums;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LogUtils.Properties
{
    internal class LogPropertyWriter
    {
        private readonly LogPropertyFile propertyFile;

        public LogPropertyWriter(LogPropertyFile file)
        {
            propertyFile = file;
        }

        /// <summary>
        /// Writes property data to file. If the content already exists, it is overwritten, if it doesn't exist, it is written at the current stream position.
        /// </summary>
        public void Write(List<LogProperties> needToUpdate)
        {
            if (needToUpdate.Count == 0) return;

            string writeString = compileWriteString(needToUpdate);

            propertyFile.PrepareStream();

            StreamWriter writer = new StreamWriter(propertyFile.Stream);

            writer.WriteLine(writeString);
            writer.Flush();
            LogProperties.PropertyManager.NotifyWriteCompleted();
        }

        private string compileWriteString(List<LogProperties> updateList)
        {
            bool hasDuplicateEntries = LogProperties.PropertyManager.HasDuplicateFileEntries;

            if (hasDuplicateEntries)
                UtilityLogger.DebugLog("File contains duplicate entries - Removing unnecessary entries from write string");

            HashSet<int> processedHashes = new HashSet<int>();
            StringBuilder sb = new StringBuilder();

            //Read all data from file
            foreach (LogPropertyData data in propertyFile.Reader.ReadData())
            {
                string dataID = data.GetID();

                if (dataID != null) //Invalid entry when dataID is null - do not include it in write string 
                {
                    //Find all matching comparisons
                    IEnumerable<LogProperties> availableProperties = LogProperties.PropertyManager.GetProperties(new ComparisonLogID(dataID));

                    //Find a comparison match that is contained within updateList
                    LogProperties properties = updateList.Intersect(availableProperties).FirstOrDefault();

                    int idHash = properties != null ? properties.IDHash : data.GetHashCode();

                    //UtilityLogger.DebugLog(idHash);

                    //We found a duplicate entry - don't handle it
                    if (processedHashes.Contains(idHash))
                        continue;

                    processedHashes.Add(idHash);

                    //Determine if data should be added to the write string as is, or if updates are required
                    if (properties == null)
                        sb.AppendLine(data.GetWriteString(true));
                    else
                    {
                        properties.UpdateWriteHash();
                        sb.AppendLine(properties.GetWriteString(data.Comments));

                        updateList.Remove(properties);
                    }
                }
            }

            //All remaining entries must be new, and can be written after all existing entries
            foreach (LogProperties properties in updateList)
            {
                properties.UpdateWriteHash();
                sb.AppendLine(properties.GetWriteString());
            }

            updateList.Clear(); //Task complete - list no longer needed
            return sb.ToString();
        }
    }
}
