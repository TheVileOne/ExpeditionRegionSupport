using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RWCustom;
using UnityEngine;

namespace LogUtils
{
    public class LogProperties
    {
        public static PropertyDataController PropertyManager => UtilityCore.PropertyManager;

        public bool ReadOnly;

        public CustomLogPropertyCollection CustomProperties = new CustomLogPropertyCollection();

        /// <summary>
        /// The ID strings of the mod(s) that control these log properties 
        /// </summary>
        public List<string> AssociatedModIDs = new List<string>();

        private string _version = "0.5.0";
        private string _filename = string.Empty;
        private string _altFilename = string.Empty;
        private string[] _tags;

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
        /// The active filename of the log file (without file extension)
        /// </summary>
        public string CurrentFilename
        {
            get
            {
                //Filename field designates the original log filename, while AltFilename is the preferred/current log filename 
                string currentFilename;
                if (!string.IsNullOrEmpty(AltFilename))
                    currentFilename = AltFilename;
                else
                    currentFilename = Filename;
                return currentFilename;
            }
        }

        /// <summary>
        /// The active filepath of the log file (with filename)
        /// </summary>
        public string CurrentFilepath => Path.Combine(ContainingFolderPath, CurrentFilename + ".log");

        /// <summary>
        /// The active filepath of the log file (without filename)
        /// </summary>
        public string ContainingFolderPath { get; private set; }

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
            get => _tags;
            set
            {
                if (ReadOnly) return;
                _tags = value;
            }
        }

        /// <summary>
        /// A prioritized order of process actions that must be applied to a message string before logging it to file 
        /// </summary>
        public LogRuleCollection Rules = new LogRuleCollection();

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
                Rules.Add(property.GetRule());
            //TODO: Define non-rule based properties
        }

        private void onCustomPropertyRemoved(CustomLogProperty property)
        {
            if (property.IsLogRule)
                Rules.Remove(property.Name);
        }

        public bool HasPath(string path)
        {
            return PathUtils.ComparePaths(CurrentFolderPath, GetContainingPath(path));
        }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendPropertyString("filename", Filename);
            sb.AppendPropertyString("altfilename", AltFilename);
            sb.AppendPropertyString("version", Version);
            sb.AppendPropertyString("tags", Tags != null ? string.Join(",", Tags) : string.Empty);
            sb.AppendPropertyString("path", PathUtils.ToPlaceholderPath(ContainingFolderPath));
            sb.AppendPropertyString("logrules");

            LogRule lineCountRule = Rules.FindByType<ShowLineCountRule>();
            LogRule categoryRule = Rules.FindByType<ShowCategoryRule>();

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
                        LogRule customRule = Rules.FindByName(customProperty.Name);
                        propertyString = customRule.PropertyString;
                    }
                    sb.AppendLine(propertyString);
                }
            }

            return sb.ToString();
        }

        public static string GetContainingPath(string relativePath)
        {
            if (relativePath == null)
                return Application.streamingAssetsPath;

            relativePath = PathUtils.ToPath(relativePath.ToLower());

            if (Directory.Exists(relativePath)) //No need to change the path when it is already valid
                return relativePath;

            if (Custom.rainWorld != null)
            {
                string customPath = AssetManager.ResolveDirectory(relativePath); //This cannot be called too early in the load process

                if (Directory.Exists(customPath))
                    return customPath;
            }

            return Application.streamingAssetsPath; //Fallback path - Should register custom path later if it needs to be resolved through AssetManager
        }

        /// <summary>
        /// Compares two names for equality (case insensitive)
        /// </summary>
        public static bool CompareNames(string name, string otherName)
        {
            return string.Equals(name, otherName, StringComparison.InvariantCultureIgnoreCase);
        }

        public static string ToPropertyString(string name, string value = "")
        {
            return name + ':' + value;
        }
    }
}
