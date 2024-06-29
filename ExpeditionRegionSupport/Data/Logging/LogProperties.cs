using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RWCustom;
using UnityEngine;

namespace ExpeditionRegionSupport.Data.Logging
{
    public class LogProperties
    {
        public static PropertyDataController PropertyManager;

        public bool ReadOnly;
        public readonly string ContainingFolderPath;

        public CustomLogPropertyCollection CustomProperties = new CustomLogPropertyCollection();


        private string _version = "0.5.0";
        private string _filename = string.Empty;
        private string _altFilename = string.Empty;
        private string[] _tags;
        private List<LogRule> _rules = new List<LogRule>();

        /// <summary>
        /// A string representation of the content state. This is useful for preventing user sourced changes from being overwritten by mods
        /// </summary>
        public string Version
        {
            get => _version;
            set
            {
                if (_version == value) return;

                if (value == null)
                    throw new ArgumentNullException(nameof(Version) + " cannot be null");

                ReadOnly = false; //Updating the version exposes LogProperties to changes
                _version = value;
            }
        }

        /// <summary>
        /// The filename that will be used in the typical write path for the log file
        /// </summary>
        public string Filename
        {
            get => _filename;
            set
            {
                if (_filename == value || ReadOnly) return;

                if (value == null)
                    throw new ArgumentNullException(nameof(Filename) + " cannot be null. Use root, or customroot as a value instead.");
                _filename = value;
            }
        }

        /// <summary>
        /// The filename that will be used if the write path is the LogManager Logs directory. May be null if same as Filename
        /// </summary>
        public string AltFilename
        {
            get => _altFilename;
            set
            {
                if (_altFilename == value || ReadOnly) return;

                if (value == null)
                    throw new ArgumentNullException(nameof(AltFilename) + " cannot be null. Use root, or customroot as a value instead.");
                _altFilename = value;
            }
        }

        /// <summary>
        /// An array of value identifiers for a specific log
        /// </summary>
        public string[] Tags
        {
            get
            {
                if (ReadOnly)
                    return (string[])_tags.Clone();
                return _tags;
            }
            set
            {
                if (ReadOnly) return;
                _tags = value;
            }
        }

        /// <summary>
        /// A prioritized order of process actions that must be applied to a message string before logging it to file 
        /// </summary>
        public IOrderedEnumerable<LogRule> Rules => _rules.OrderBy(r => r.Priority);

        public LogProperties(string filename, string relativePathNoFile = "customroot")
        {
            Filename = filename;
            ContainingFolderPath = GetContainingPath(relativePathNoFile);

            CustomProperties.OnPropertyAdded += onCustomPropertyAdded;
            CustomProperties.OnPropertyRemoved += onCustomPropertyRemoved;
        }

        private void onCustomPropertyAdded(CustomLogProperty property)
        {
            if (property.IsLogRule)
            {
                LogRule customRule = property.GetRule();
                AddRule(customRule);
            }

            //TODO: Define non-rule based properties
        }

        private void onCustomPropertyRemoved(CustomLogProperty property)
        {
            if (property.IsLogRule)
                RemoveRule(property.Name);
        }

        /// <summary>
        /// Adds a LogRule instance to the collection of Rules
        /// Do not use this for temporary rule changes, use SetTemporaryRule instead 
        /// </summary>
        public void AddRule(LogRule rule)
        {
            if (_rules.Exists(r => r.Name == rule.Name)) //Don't allow more than one rule concept to be added with the same name
            {
                //TODO: Suggest that ReplaceRule be used instead
                return;
            }
            _rules.Add(rule);
        }

        /// <summary>
        /// Replaces an existing rule with another instance
        /// Be warned, running this each time your mod runs will overwrite data being saved, and read from file
        /// Do not replace existing property data values in a way that might break the parse logic
        /// Consider using temporary rules instead, and handle saving of the property values through your mod
        /// In either case, you may want to inherit from the existing property in case a user has changed the property through the file
        /// </summary>
        public void ReplaceRule(LogRule rule)
        {
            int ruleIndex = _rules.FindIndex(r => r.Name == rule.Name);

            if (ruleIndex != -1)
            {
                LogRule replacedRule = _rules[ruleIndex];

                //Transfer over temporary rules as long as replacement rule doesn't have one already
                if (rule.TemporaryOverride == null)
                    rule.TemporaryOverride = replacedRule.TemporaryOverride;
                _rules.RemoveAt(ruleIndex);
            }
            AddRule(rule); //Add rule when there is no existing rule match
        }

        public bool RemoveRule(LogRule rule)
        {
            return _rules.Remove(rule);
        }

        public bool RemoveRule(string name)
        {
            int ruleIndex = _rules.FindIndex(r => r.Name == name);

            if (ruleIndex != -1)
            {
                _rules.RemoveAt(ruleIndex);
                return true;
            }
            return false;
        }

        public void SetTemporaryRule(LogRule rule)
        {
            LogRule targetRule = _rules.Find(r => r.Name == rule.Name);

            if (targetRule != null)
                targetRule.TemporaryOverride = rule;
            else
                AddRule(rule); //No associated rule was found, treat temporary rule as a normal rule
        }

        public void RemoveTemporaryRule(LogRule rule)
        {
            LogRule targetRule = _rules.Find(r => r.TemporaryOverride == rule);

            if (targetRule != null)
                targetRule.TemporaryOverride = null;
        }

        public bool HasPath(string path)
        {
            //TODO: Strip filename
            return LogUtils.ComparePaths(ContainingFolderPath, GetContainingPath(path));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendPropertyString("filename", Filename);
            sb.AppendPropertyString("altfilename", AltFilename);
            sb.AppendPropertyString("version", Version);
            sb.AppendPropertyString("tags", (Tags != null ? string.Join(",", Tags) : string.Empty));
            sb.AppendPropertyString("path", LogUtils.ToPlaceholderPath(ContainingFolderPath));
            sb.AppendPropertyString("logrules");

            LogRule lineCountRule = _rules.Find(r => r is ShowLineCountRule);
            LogRule categoryRule = _rules.Find(r => r is ShowCategoryRule);

            sb.AppendLine(lineCountRule.PropertyString);
            sb.AppendLine(categoryRule.PropertyString);

            if (CustomProperties.Any())
            {
                sb.AppendPropertyString("custom");

                foreach (var customProperty in CustomProperties)
                {
                    //Log properties with names that are not unique are unsupported, and may cause unwanted behavior
                    //Duplicate named property strings will still be written to file the way this is currently handled
                    string propertyString = customProperty.PropertyString;
                    if (customProperty.IsLogRule)
                    {
                        LogRule customRule = _rules.Find(r => r.Name == customProperty.Name);
                        propertyString = customRule.PropertyString;
                    }
                    sb.AppendLine(propertyString);
                }
            }

            return sb.ToString();
        }

        public static string GetContainingPath(string relativePathNoFile)
        {
            if (relativePathNoFile == null)
                return Application.streamingAssetsPath;

            relativePathNoFile = LogUtils.ToPath(relativePathNoFile.ToLower());

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

        public static string ToPropertyString(string name, string value = "")
        {
            return name + ':' + value;
        }
    }
}
