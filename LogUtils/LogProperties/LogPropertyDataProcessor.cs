using LogUtils.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using static LogUtils.UtilityConsts;

namespace LogUtils.Properties
{
    public class LogPropertyDataProcessor
    {
        private readonly LogPropertyData data;

        /// <summary>
        /// The result of processing LogPropertyData
        /// </summary>
        public LogProperties Results;

        public LogPropertyDataProcessor(LogPropertyData data)
        {
            this.data = data;
        }

        /// <summary>
        /// Processes LogPropertyData into a LogProperties result
        /// </summary>
        public void Process()
        {
            LogPropertyStringDictionary dataFields = data.Fields;

            bool processedWithErrors = false;
            bool hasGroupTag = processTags(out string[] tags);

            LogPropertyMetadata metadata = extractMetadata(dataFields);

            string id = dataFields[DataFields.LOGID];

            bool hasID = !string.IsNullOrEmpty(id);
            bool hasFilename = !string.IsNullOrEmpty(metadata.Filename);

            processedWithErrors = !hasID || (!hasGroupTag && !hasFilename);

            if (processedWithErrors)
            {
                //These properties are necessary - at least one of them must contain valid data
                if (!hasID && !hasFilename)
                {
                    UtilityLogger.LogWarning("Malformed data in properties file");
                    Results = null;
                    return;
                }

                //Inherit from the other value if one value happens to be invalid
                if (!hasID)
                    id = LogID.Sanitize(metadata.Filename);
                else if (!hasFilename)
                    metadata.Filename = id;
            }

            var result = metadata.PopulateMissingPathValues();

            if (result == MetadataPathResult.MissingPathResolved || result == MetadataPathResult.UnableToResolve)
            {
                processedWithErrors = true; //Even for log groups this indicates a problem with the property entries
                UtilityLogger.LogWarning("Malformed path data in properties file");
            }

            LogProperties properties;
            if (!hasGroupTag)
                properties = new LogProperties(id, metadata);
            else
            {
                id = LogID.CreateIDValue(id, LogIDType.Group);
                properties = new LogGroupProperties(id, metadata);
            }

            properties.Tags = tags;

            #pragma warning disable IDE0055 //Fix formatting
            //Property setters are inaccesible. Define delegate wrappers for each one, and store in a dictionary
            OrderedDictionary fieldAssignments = new OrderedDictionary
            {
                [DataFields.VERSION]              = new Action(() => properties.Version            = Version.Parse(dataFields[DataFields.VERSION])),
                [DataFields.CONSOLEIDS]           = new Action(() => properties.ConsoleIDs         .AddRange(parseConsoleIDs(dataFields[DataFields.CONSOLEIDS]))),
                [DataFields.LOGS_FOLDER_AWARE]    = new Action(() => properties.LogsFolderAware    = bool.Parse(dataFields[DataFields.LOGS_FOLDER_AWARE])),
                [DataFields.LOGS_FOLDER_ELIGIBLE] = new Action(() => properties.LogsFolderEligible = bool.Parse(dataFields[DataFields.LOGS_FOLDER_ELIGIBLE])),
                [DataFields.SHOW_LOGS_AWARE]      = new Action(() => properties.ShowLogsAware      = bool.Parse(dataFields[DataFields.SHOW_LOGS_AWARE])),
                [DataFields.Intro.MESSAGE]        = new Action(() => properties.IntroMessage       = dataFields[DataFields.Intro.MESSAGE]),
                [DataFields.Intro.TIMESTAMP]      = new Action(() => properties.ShowIntroTimestamp = bool.Parse(dataFields[DataFields.Intro.TIMESTAMP])),
                [DataFields.Outro.MESSAGE]        = new Action(() => properties.OutroMessage       = dataFields[DataFields.Outro.MESSAGE]),
                [DataFields.Outro.TIMESTAMP]      = new Action(() => properties.ShowOutroTimestamp = bool.Parse(dataFields[DataFields.Outro.TIMESTAMP])),
                [DataFields.TIMESTAMP_FORMAT]     = new Action(() => properties.DateTimeFormat     = parseDateTimeFormat(dataFields[DataFields.TIMESTAMP_FORMAT])),

                [DataFields.Rules.SHOW_CATEGORIES] = new Action(() => properties.ShowCategories.IsEnabled = bool.Parse(dataFields[DataFields.Rules.SHOW_CATEGORIES])),
                [DataFields.Rules.SHOW_LINE_COUNT] = new Action(() => properties.ShowLineCount.IsEnabled = bool.Parse(dataFields[DataFields.Rules.SHOW_LINE_COUNT])),
                [DataFields.Rules.LOG_TIMESTAMP]   = new Action(() => properties.ShowLogTimestamp.IsEnabled = bool.Parse(dataFields[DataFields.Rules.LOG_TIMESTAMP]))
            };
            #pragma warning restore IDE0055 //Fix formatting

            IDictionaryEnumerator enumerator = fieldAssignments.GetEnumerator();

            //Iterate through every pending assignment
            while (enumerator.MoveNext())
            {
                string dataField = (string)enumerator.Key;
                Action setDataField = (Action)enumerator.Value;

                try
                {
                    setDataField(); //Apply the action that was stored earlier
                }
                catch (Exception ex) when (ex is ArgumentNullException || ex is NullReferenceException || ex is FormatException)
                {
                    onProcessError(dataField);
                }
            }

            properties.ProcessedWithErrors = processedWithErrors;

            if (properties.ProcessedWithErrors)
            {
                string reportMessage;
                if (!hasGroupTag)
                    reportMessage = "There were issues while processing LogID " + id;
                else
                    reportMessage = "There were issues while processing GroupID " + id;
                UtilityLogger.LogWarning(reportMessage);
            }

            Results = properties;

            LogPropertyMetadata extractMetadata(LogPropertyStringDictionary dataFields)
            {
                return new LogPropertyMetadata()
                {
                    IsOptional = hasGroupTag,

                    Filename = dataFields[DataFields.FILENAME],
                    AltFilename = dataFields[DataFields.ALTFILENAME],
                    Path = dataFields[DataFields.PATH],
                    OriginalPath = dataFields[DataFields.ORIGINAL_PATH],
                    LastKnownPath = dataFields[DataFields.LAST_KNOWN_PATH]
                };
            }

            bool processTags(out string[] tags)
            {
                tags = parseTags(dataFields[DataFields.TAGS]);

                if (tags == null)
                {
                    tags = [];
                    onProcessError(DataFields.TAGS);
                }
                return tags.Contains(PropertyTag.LOG_GROUP);
            }

            void onProcessError(string dataField)
            {
                UtilityLogger.LogWarning($"Expected data field '{dataField}' was missing or malformatted");
                processedWithErrors = true;
            }
        }

        private static IEnumerable<ConsoleID> parseConsoleIDs(string dataEntry)
        {
            foreach (string idValue in dataEntry.Split(','))
            {
                ConsoleID.TryParse(idValue, out ConsoleID id);

                if (id != null)
                    yield return new ConsoleID(idValue, register: false);
            }
            yield break;
        }

        private static DateTimeFormat parseDateTimeFormat(string dataEntry)
        {
            if (string.IsNullOrEmpty(dataEntry))
                return null;
            return new DateTimeFormat(dataEntry);
        }

        private static string[] parseTags(string dataEntry)
        {
            if (dataEntry == null) //Avoid hiding that data field was null
                return null;
            return dataEntry.Split(',');
        }
    }
}
