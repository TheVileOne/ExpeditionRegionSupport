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
            StringDictionary dataFields = data.Fields;

            bool processedWithErrors = false;

            string idString = dataFields[DataFields.LOGID];
            string filename = dataFields[DataFields.FILENAME];
            string path = dataFields[DataFields.PATH];

            //idString, and filename are closely related, and commonly contain the same value
            if (idString == null)
            {
                processedWithErrors = true;
                if (filename == null)
                    filename = UtilityConsts.LogNames.Unknown; //Use a fallback log file when important id data is missing
                idString = filename;
            }
            else if (filename == null)
            {
                processedWithErrors = true;
                filename = idString;
            }
            processedWithErrors |= path == null; //Path string is null safe here, but path should not be null

            LogProperties properties = new LogProperties(idString, filename, path);

            //Property setters are inaccesible. Define delegate wrappers for each one, and store in a dictionary
            OrderedDictionary fieldAssignments = new OrderedDictionary();

            fieldAssignments[DataFields.VERSION]               = new Action(() => properties.Version             = dataFields[DataFields.VERSION]);
            fieldAssignments[DataFields.ALTFILENAME]           = new Action(() => properties.AltFilename         = dataFields[DataFields.ALTFILENAME]);
            fieldAssignments[DataFields.ORIGINAL_PATH]         = new Action(() => properties.OriginalFolderPath  = dataFields[DataFields.ORIGINAL_PATH]);
            fieldAssignments[DataFields.LAST_KNOWN_PATH]       = new Action(() => properties.LastKnownFilePath   = dataFields[DataFields.LAST_KNOWN_PATH]);
            fieldAssignments[DataFields.TAGS]                  = new Action(() => properties.Tags                = dataFields[DataFields.TAGS].Split(','));
            fieldAssignments[DataFields.LOGS_FOLDER_AWARE]     = new Action(() => properties.LogsFolderAware     = bool.Parse(dataFields[DataFields.LOGS_FOLDER_AWARE]));
            fieldAssignments[DataFields.LOGS_FOLDER_ELIGIBLE]  = new Action(() => properties.LogsFolderEligible  = bool.Parse(dataFields[DataFields.LOGS_FOLDER_ELIGIBLE]));
            fieldAssignments[DataFields.SHOW_LOGS_AWARE]       = new Action(() => properties.ShowLogsAware       = bool.Parse(dataFields[DataFields.SHOW_LOGS_AWARE]));
            fieldAssignments[DataFields.Intro.MESSAGE]         = new Action(() => properties.IntroMessage        = dataFields[DataFields.Intro.MESSAGE]);
            fieldAssignments[DataFields.Outro.MESSAGE]         = new Action(() => properties.OutroMessage        = dataFields[DataFields.Outro.MESSAGE]);
            fieldAssignments[DataFields.Intro.TIMESTAMP]       = new Action(() => properties.ShowIntroTimestamp  = bool.Parse(dataFields[DataFields.Intro.TIMESTAMP]));
            fieldAssignments[DataFields.Outro.TIMESTAMP]       = new Action(() => properties.ShowOutroTimestamp  = bool.Parse(dataFields[DataFields.Outro.TIMESTAMP]));

            fieldAssignments[DataFields.Rules.SHOW_CATEGORIES] = new Action(() => properties.ShowCategories.IsEnabled = bool.Parse(dataFields[DataFields.Rules.SHOW_CATEGORIES]));
            fieldAssignments[DataFields.Rules.SHOW_LINE_COUNT] = new Action(() => properties.ShowLineCount.IsEnabled  = bool.Parse(dataFields[DataFields.Rules.SHOW_LINE_COUNT]));

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

                    UtilityCore.BaseLogger.LogError(ex);
                    processedWithErrors = true;
                }
            }

            properties.ProcessedWithErrors = processedWithErrors;
            Results = properties;
        }
    }
}
