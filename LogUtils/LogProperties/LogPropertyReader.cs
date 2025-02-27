﻿using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace LogUtils.Properties
{
    internal class LogPropertyReader : IEnumerable<LogPropertyData>
    {
        private readonly LogPropertyFile propertyFile;

        public LogPropertyReader(LogPropertyFile file)
        {
            propertyFile = file;
        }

        public IEnumerator<LogPropertyData> GetEnumerator()
        {
            //Avoid creating the property file during an attempt to read from it
            if (propertyFile.IsClosed)
                yield break;

            propertyFile.PrepareStream();

            bool expectingNextEntry = true;

            bool expectedFieldsMatch = true;
            int fieldMatchCount = 0; //Amount of consecuative fields that are consistent with the expected write order 
            int commentCountForThisField = 0;
            List<CommentEntry> commentEntries = new List<CommentEntry>();

            StreamReader reader = new StreamReader(propertyFile.Stream);

            LogPropertyStringDictionary propertyInFile = null;
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine().Trim();

                //An empty line separates each properties section
                if (line == string.Empty)
                {
                    expectingNextEntry = true;
                    continue;
                }

                //Comments are stored until they can be paired with a valid field
                if (line.StartsWith("//"))
                {
                    commentCountForThisField++;
                    commentEntries.Add(new CommentEntry()
                    {
                        Message = line
                    });
                    continue;
                }

                //Parse the data into an array by using the data separator char - Non-comments and field data is ignored
                string[] lineData = line.Split(':');

                if (lineData.Length > 1)
                {
                    //A valid data field will begin with a header string, followed by a data string
                    string header = lineData[0];
                    string value = lineData[1];

                    if (lineData.Length > 2) //This shouldn't be a thing, but lets handle it to be safe
                        value = line.Substring(line.IndexOf(":") + 1);

                    //Comment entries can now be associated with an owner
                    if (commentCountForThisField > 0)
                    {
                        for (int i = commentEntries.Count - commentCountForThisField; i < commentEntries.Count; i++)
                        {
                            CommentEntry comment = commentEntries[i];

                            comment.Owner = header;
                            commentEntries[i] = comment;
                        }
                        commentCountForThisField = 0;
                    }

                    //Check if the data represents a new log file
                    //All log file data should have this header, but by checking for duplicate keys after an empty line, we can handle cases when it is missing
                    bool startNextEntry = header == UtilityConsts.DataFields.LOGID || (expectingNextEntry && (propertyInFile == null || propertyInFile.ContainsKey(header)));

                    if (startNextEntry)
                    {
                        var lastProcessed = propertyInFile;
                        var lastComments = commentEntries;

                        //Start a new entry for the next set of data fields
                        propertyInFile = new LogPropertyStringDictionary();

                        if (lastProcessed != null)
                        {
                            commentEntries = new List<CommentEntry>();

                            LogPropertyData data = new LogPropertyData(lastProcessed, lastComments)
                            {
                                FieldOrderMismatch = !expectedFieldsMatch
                            };
                            yield return data; //Data collection has finished for the last entry - return the data
                        }

                        //Reset values to default
                        expectedFieldsMatch = true;
                        fieldMatchCount = 0;
                    }

                    //Check that the actual field order read from file matches the field order expected by the utility
                    if (expectedFieldsMatch && fieldMatchCount < UtilityConsts.DataFields.OrderedFields.Length)
                    {
                        expectedFieldsMatch = header == UtilityConsts.DataFields.OrderedFields[fieldMatchCount];

                        if (expectedFieldsMatch)
                            fieldMatchCount++;
                    }

                    //Store each data field in a dictionary until all lines pertaining to the current property are read
                    propertyInFile[header] = value;
                }
            }

            //The loop has finished, but the last entry has not yet been returned
            if (propertyInFile != null)
            {
                if (commentEntries.Count > 0 && commentEntries[commentEntries.Count - 1].Owner == null)
                    UtilityLogger.LogWarning("End of file comments are unsupported");

                LogPropertyData data = new LogPropertyData(propertyInFile, commentEntries)
                {
                    FieldOrderMismatch = !expectedFieldsMatch
                };
                yield return data;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<LogPropertyData> ReadData()
        {
            var enumerator = GetEnumerator();

            while (enumerator.MoveNext())
                yield return enumerator.Current;
        }
    }

    public record struct CommentEntry(string Owner, string Message);
}
