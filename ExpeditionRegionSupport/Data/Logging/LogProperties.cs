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
        protected IOrderedEnumerable<LogRule> Rules => _rules.OrderBy(r => r.ApplyPriority);

        public LogProperties(string filename, string relativePathNoFile = "customroot")
        {
            Filename = filename;
            ContainingFolderPath = GetContainingPath(relativePathNoFile);

            CustomProperties.OnPropertyAdded += onCustomPropertyAdded;
            CustomProperties.OnPropertyRemoved += onCustomPropertyRemoved;
        }

        private void onCustomPropertyAdded(CustomLogProperty property)
        {
            if (property.IsEnabled && property.IsLogRule)
            {
                LogRule customRule = property.CreateRule();

                //Allows custom LogRules to be searchable
                customRule.Name = property.Name;
                AddRule(property.CreateRule());
            }

            //TODO: Define non-rule based properties
        }

        private void onCustomPropertyRemoved(CustomLogProperty property)
        {
            if (property.IsLogRule)
                RemoveRule(property.Name);
        }

        public void AddRule(LogRule rule)
        {
            _rules.Add(rule);
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

        public bool HasPath(string path)
        {
            //TODO: Strip filename
            return LogUtils.ComparePaths(ContainingFolderPath, GetContainingPath(path));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("filename:" + Filename);
            sb.AppendLine("altfilename:" + AltFilename);
            sb.AppendLine("version:" + Version);
            sb.AppendLine("tags:" + (Tags != null ? string.Join(",", Tags) : string.Empty));
            sb.AppendLine("path:" + LogUtils.ToPlaceholderPath(ContainingFolderPath));
            sb.AppendLine("logrules:");
            sb.AppendLine("showlinecount:" + Rules.Exists(r => r is ShowLineCountRule));
            sb.AppendLine("showcategories:" + Rules.Exists(r => r is ShowCategoryRule));
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
    }
}
