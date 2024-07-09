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
        public CustomLogPropertyCollection CustomProperties = new CustomLogPropertyCollection();

        public bool IsCreated;
        public bool ReadOnly;

        /// <summary>
        /// The ID strings of the mod(s) that control these log properties 
        /// </summary>
        public List<string> AssociatedModIDs = new List<string>();

        private string _version = "0.5.0";
        private string _filename = string.Empty;
        private string _altFilename = string.Empty;
        private string _folderPath = string.Empty;
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
        public string CurrentFilename { get; private set; }

        /// <summary>
        /// The active filepath of the log file (with filename)
        /// </summary>
        public string CurrentFilePath => Path.Combine(CurrentFolderPath, CurrentFilename + ".log");

        /// <summary>
        /// The active full path of the directory containing the log file
        /// </summary>
        public string CurrentFolderPath { get; private set; }

        /// <summary>
        /// The full path to the directory containing the log file as recorded from the properties file
        /// </summary>
        public string FolderPath
        {
            get => _folderPath;
            set
            {
                if (_folderPath == value || ReadOnly) return;

                if (value == null)
                    throw new ArgumentNullException(nameof(FolderPath) + " cannot be null. Use root, or customroot as a value instead.");
                _folderPath = value;
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
                    throw new ArgumentNullException(nameof(Filename) + " cannot be null");
                _filename = value;
            }
        }

        /// <summary>
        /// The filename that will be used if the write path is the Logs directory. May be null if same as Filename
        /// </summary>
        public string AltFilename
        {
            get => _altFilename;
            set
            {
                if (_altFilename == value || ReadOnly) return;

                if (value == null)
                    throw new ArgumentNullException(nameof(AltFilename) + " cannot be null");
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
            FolderPath = GetContainingPath(relativePathNoFile);

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

        public void ChangePath(string newPath)
        {
            newPath = PathUtils.RemoveFileFromPath(newPath, out string newFilename);

            bool changesPresent = false;

            //Compare the current filename to the new filename
            if (newFilename != null && !FileUtils.CompareFilenames(CurrentFilename, newFilename))
            {
                CurrentFilename = FileUtils.RemoveExtension(newFilename);
                changesPresent = true;
            }

            //Compare the current path to the new path
            if (!PathUtils.ComparePaths(CurrentFolderPath, newPath)) //The paths are different
            {
                CurrentFolderPath = newPath;
                changesPresent = true;
            }

            //Loggers need to be notified of any changes that might affect managed LogIDs
            if (changesPresent)
            {
                IsCreated = File.Exists(CurrentFilePath);
                //OnPathChanged.Invoke(this); TODO: Create
            }

            //Steps:
            //Determine if it is a relative or full path
            //Remove the filename (if it exists) from the path and set it separately
            //Validate file - throw exception if it's not a .txt, or .log file
            //Change to .log file ext
            //Set path
        }

        public void ChangePath(string newPath, string newFilename)
        {
            ChangePath(Path.Combine(newPath, newFilename));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendPropertyString("filename", Filename);
            sb.AppendPropertyString("altfilename", AltFilename);
            sb.AppendPropertyString("version", Version);
            sb.AppendPropertyString("tags", Tags != null ? string.Join(",", Tags) : string.Empty);
            sb.AppendPropertyString("path", PathUtils.ToPlaceholderPath(FolderPath));
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

            //Apply some preprocessing to the path based on whether it is a partial, or full path
            string path;
            if (Path.IsPathRooted(relativePath))
            {
                UtilityCore.BaseLogger.LogInfo("Processing a rooted path when expecting a partial one");

                if (Directory.Exists(relativePath)) //As long as it exists, we shouldn't care if it is rooted
                    return relativePath;

                UtilityCore.BaseLogger.LogInfo("Rooted path could not be found. Unrooting...");

                //Unrooting allows us to still find a possibly valid Rain World path
                relativePath = PathUtils.Unroot(relativePath);

                path = Path.GetFullPath(relativePath);

                if (PathUtils.PathRootExists(path))
                {
                    UtilityCore.BaseLogger.LogInfo("Unroot successful");
                    return path;
                }

                path = relativePath; //We don't know where this path is, but we shouldn't default to the Rain World root here 
            }
            else
            {
                path = PathUtils.ToPath(relativePath);
            }

            if (PathUtils.PathRootExists(path)) //No need to change the path when it is already valid
                return path;

            UtilityCore.BaseLogger.LogInfo("Attempting to resolve path");

            //Resolve directory the game supported way if we're not too early to do so (most likely will be too early)
            if (Custom.rainWorld != null)
                return AssetManager.ResolveDirectory(path);

            UtilityCore.BaseLogger.LogInfo("Defaulting to custom root. Path check run too early to resolve");

            //This is what AssetManager.ResolveDirectory would have returned as a fallback path
            return Path.Combine(Application.streamingAssetsPath, relativePath);
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
