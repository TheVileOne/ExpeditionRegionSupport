using System;
using System.Collections;
using System.Collections.Specialized;
using DataFields = LogUtils.UtilityConsts.DataFields;

namespace LogUtils.Properties
{
    public class LogPropertyDataProcessor
    {
        private LogPropertyData data;

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

            string id = dataFields[DataFields.LOGID];
            string filename = dataFields[DataFields.FILENAME];
            string path = dataFields[DataFields.PATH];

            bool hasID = !string.IsNullOrEmpty(id);
            bool hasFilename = !string.IsNullOrEmpty(filename);

            processedWithErrors = !hasID || !hasFilename || path == null;

            if (processedWithErrors)
            {
                //These properties are necessary - at least one of them must contain valid data
                if (!hasID && !hasFilename)
                {
                    UtilityLogger.LogWarning("Malformed data in properties file");
                    processedWithErrors = true;
                    Results = null;
                    return;
                }

                //Inherit from the other value if one value happens to be invalid
                if (!hasID)
                    id = filename;
                else if (!hasFilename)
                    filename = id;
            }

            LogProperties properties = new LogProperties(id, filename, path);

            //Property setters are inaccesible. Define delegate wrappers for each one, and store in a dictionary
            OrderedDictionary fieldAssignments = new OrderedDictionary
            {
                [DataFields.VERSION]              = new Action(() => properties.Version            = dataFields[DataFields.VERSION]),
                [DataFields.ALTFILENAME]          = new Action(() => properties.AltFilename        = dataFields[DataFields.ALTFILENAME]),
                [DataFields.ORIGINAL_PATH]        = new Action(() => properties.OriginalFolderPath = dataFields[DataFields.ORIGINAL_PATH]),
                [DataFields.LAST_KNOWN_PATH]      = new Action(() => properties.LastKnownFilePath  = dataFields[DataFields.LAST_KNOWN_PATH]),
                [DataFields.TAGS]                 = new Action(() => properties.Tags               = dataFields[DataFields.TAGS].Split(',')),
                [DataFields.LOGS_FOLDER_AWARE]    = new Action(() => properties.LogsFolderAware    = bool.Parse(dataFields[DataFields.LOGS_FOLDER_AWARE])),
                [DataFields.LOGS_FOLDER_ELIGIBLE] = new Action(() => properties.LogsFolderEligible = bool.Parse(dataFields[DataFields.LOGS_FOLDER_ELIGIBLE])),
                [DataFields.SHOW_LOGS_AWARE]      = new Action(() => properties.ShowLogsAware      = bool.Parse(dataFields[DataFields.SHOW_LOGS_AWARE])),
                [DataFields.Intro.MESSAGE]        = new Action(() => properties.IntroMessage       = dataFields[DataFields.Intro.MESSAGE]),
                [DataFields.Intro.TIMESTAMP]      = new Action(() => properties.ShowIntroTimestamp = bool.Parse(dataFields[DataFields.Intro.TIMESTAMP])),
                [DataFields.Outro.MESSAGE]        = new Action(() => properties.OutroMessage       = dataFields[DataFields.Outro.MESSAGE]),
                [DataFields.Outro.TIMESTAMP]      = new Action(() => properties.ShowOutroTimestamp = bool.Parse(dataFields[DataFields.Outro.TIMESTAMP])),

                [DataFields.Rules.SHOW_CATEGORIES] = new Action(() => properties.ShowCategories.IsEnabled = bool.Parse(dataFields[DataFields.Rules.SHOW_CATEGORIES])),
                [DataFields.Rules.SHOW_LINE_COUNT] = new Action(() => properties.ShowLineCount.IsEnabled = bool.Parse(dataFields[DataFields.Rules.SHOW_LINE_COUNT]))
            };

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
                catch (Exception ex) when (ex is ArgumentNullException || ex is NullReferenceException)
                {
                    if (dataField == DataFields.TAGS)
                        properties.Tags = Array.Empty<string>();

                    UtilityLogger.LogError(ex);
                    processedWithErrors = true;
                }
            }

            properties.ProcessedWithErrors = processedWithErrors;

            if (properties.ProcessedWithErrors)
                UtilityLogger.LogWarning("There were issues while processing LogID " + id);

            Results = properties;
        }
    }
}
