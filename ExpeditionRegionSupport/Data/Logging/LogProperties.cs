using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace ExpeditionRegionSupport.Data.Logging
{
    public class LogProperties
    {
        public static bool RequestLoad;

        public static bool HasReadPropertiesFile;

        public static List<LogProperties> AllProperties = new List<LogProperties>();

        private LoggerID _logID;
        public LoggerID LogID
        {
            get
            {
                if (_logID == null)
                    _logID = new LoggerID(Filename, false);
                return _logID;
            }
        }

        /// <summary>
        /// The filename that will be used in the typical write path for the log file
        /// </summary>
        public string Filename;

        /// <summary>
        /// The filename that will be used if the write path is the LogManager Logs directory. May be null if same as Filename
        /// </summary>
        public string AltFilename;

        /// <summary>
        /// A list of filenames that should be considered equal to Filename/AltFilename
        /// </summary>
        public List<string> Aliases = new List<string>();

        protected List<LogRule> Rules = new List<LogRule>();

        public void AddRule(LogRule rule)
        {
            if (Rules.Exists(r => r.ID == rule.ID)) return;

            if (Rules.Count == 0)
            {
                Rules.Add(rule);
                return;
            }

            //The code below ensures that priority rules are last in the list. Rules that are applied last in the list are applied last to the log message
            switch (rule.ID)
            {
                case LogRule.Rule.ShowCategory:
                    if (Rules[Rules.Count - 1].ID == LogRule.Rule.ShowLineCount) //Line count should apply before category formatting
                        Rules.Insert(Rules.Count - 1, rule);
                    else
                        Rules.Add(rule); //Category formatting takes priority over every other rule
                    break;
                case LogRule.Rule.ShowLineCount:
                    Rules.Add(rule);
                    break;
                case LogRule.Rule.Unknown:
                    if (Rules[Rules.Count - 1].ID == LogRule.Rule.ShowCategory) //Insert before prioritized rules
                        Rules.Insert(Rules.Count - 1, rule);
                    else if (Rules[Rules.Count - 1].ID == LogRule.Rule.ShowLineCount) //ShowLineCount takes priority
                    {
                        //Inserts before one, or both prioritized rules
                        if (Rules.Count == 1 || Rules[Rules.Count - 2].ID != LogRule.Rule.ShowCategory)
                            Rules.Insert(Rules.Count - 1, rule);
                        else
                            Rules.Insert(Rules.Count - 2, rule);
                    }
                    else
                        Rules.Add(rule); //There are no prioritized rules if this triggers
                    break;
            }
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static LogProperties Deserialize(string propertyString)
        {
            return JsonConvert.DeserializeObject<LogProperties>(propertyString);
        }

        public static void LoadProperties()
        {
            AllProperties.Clear();

            if (DataTransferController.UnhandledMessages.Count > 0)
            {
                bool propertiesHandled = true;
                foreach (string stringToProcess in DataTransferController.UnhandledMessages)
                {
                    Exception error = null;

                    try
                    {
                        LogProperties properties = Deserialize(stringToProcess);

                        if (properties != null)
                        {
                            Debug.LogWarning($"Transfered properties for log {properties.LogID} successfully");
                            propertiesHandled = true;
                            AllProperties.Add(properties);
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        error = ex;
                    }

                    Debug.LogWarning("Unable to transfer log properties");

                    if (error != null)
                        Debug.LogError(error);
                }

                if (propertiesHandled)
                {
                    HasReadPropertiesFile = true; //Another mod has read the file already
                    return;
                }
            }

            ReadPropertiesFromFile();
        }

        public static void ReadPropertiesFromFile()
        {
            AllProperties.Clear();

            string propertiesFile = AssetManager.ResolveFilePath("logs.txt");

            if (File.Exists(propertiesFile))
            {
                string[] propertyStrings = File.ReadAllLines(propertiesFile);

                //Read all lines and serialize them into LogProperties
                for (int i = 0; i < propertyStrings.Count(); i++)
                {
                    string propertyString = propertyStrings[i];

                    if (string.IsNullOrWhiteSpace(propertyString) || propertyString.StartsWith("//")) continue;

                    try
                    {
                        LogProperties properties = Deserialize(propertyString);

                        if (properties != null)
                            AllProperties.Add(properties);

                        DataTransferController.SendMessage(propertyString); //Send serialized data string to be handled by other loggers
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("An error occured while processing log property line " + i);
                        Debug.LogError(ex);
                    }
                }
            }

            HasReadPropertiesFile = true;
        }
    }

    public class LogRule
    {
        public Rule ID = Rule.Unknown; 

        public virtual string ApplyRule(string message)
        {
            return message;
        }

        public enum Rule
        {
            Unknown = -1,
            ShowCategory,
            ShowLineCount
        }
    }
}
