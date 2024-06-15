using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using RWCustom;
using UnityEngine;

namespace ExpeditionRegionSupport.Data.Logging
{
    public class LogProperties
    {
        public static PropertyDataController PropertyManager;

        public readonly LogID LogID;

        public readonly string ContainingFolderPath;

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

        public LogProperties(LogID logID, string relativePathNoFile = "CustomRoot")
        {
            LogID = logID;
            ContainingFolderPath = GetContainingPath(relativePathNoFile);
        }

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
            if (PropertyManager == null)
            {
                PropertyManager = PropertyDataController.GetOrCreate(out bool created);

                if (created)
                    PropertyManager.ReadFromFile();
            }
        }

        public static string GetContainingPath(string relativePathNoFile)
        {
            if (relativePathNoFile == "CustomRoot")
                return Application.streamingAssetsPath;
            else if (relativePathNoFile == "Root")
                return Application.dataPath; //TODO: Check that this path is correct

            if (Directory.Exists(relativePathNoFile)) //No need to change the path when it is already valid
                return relativePathNoFile;

            if (Custom.rainWorld != null)
            {
                string customPath = AssetManager.ResolveDirectory(relativePathNoFile); //This cannot be called too early in the load process

                if (Directory.Exists(customPath))
                    return customPath;
            }

            return Application.streamingAssetsPath; //Fallback path - Should register custom path later if it needs to be resolved through AssetManager
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
