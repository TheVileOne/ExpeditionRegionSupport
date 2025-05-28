using LogUtils.Enums;
using LogUtils.Helpers.Comparers;
using LogUtils.Helpers.FileHandling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LogUtils.Properties
{
    public partial class LogProperties
    {
        private bool _fileExists;
        private string _filename = string.Empty;
        private string _altFilename = string.Empty;
        private string _folderPath = string.Empty;
        private string _originalFolderPath = string.Empty;

        public bool FileExists
        {
            get => _fileExists;
            set
            {
                if (_fileExists == value) return;

                _fileExists = value;
                LogSessionActive = false; //A new session needs to apply when file is created or removed
            }
        }

        /// <summary>
        /// The active filename of the log file (without file extension)
        /// </summary>
        public string CurrentFilename { get; private set; }

        /// <summary>
        /// The active filename of the log file (with file extension)
        /// </summary>
        public string CurrentFilenameWithExtension => CurrentFilename + PreferredFileExt;

        /// <summary>
        /// The active filepath of the log file (with filename)
        /// </summary>
        public string CurrentFilePath => Path.Combine(CurrentFolderPath, CurrentFilenameWithExtension);

        /// <summary>
        /// The path to the log file when it has been slated to be replaced or removed
        /// </summary>
        public string ReplacementFilePath { get; private set; }

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
                    throw new ArgumentNullException(nameof(FolderPath), "Property is not allowed to be null. Use root, or customroot as a value instead.");
                _folderPath = value;
            }
        }

        /// <summary>
        /// The path that was first assigned when the log file was first registered
        /// </summary>
        public string OriginalFolderPath
        {
            get => _originalFolderPath;
            set
            {
                if (_originalFolderPath == value || ReadOnly) return;

                if (value == null)
                    throw new ArgumentNullException(nameof(OriginalFolderPath), "Property is not allowed to be null. Use root, or customroot as a value instead.");
                _originalFolderPath = value;
            }
        }

        /// <summary>
        /// The path of the last known location of the log file
        /// </summary>
        public string LastKnownFilePath { get; internal set; }

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
                    throw new ArgumentNullException(nameof(Filename));
                _filename = value;
            }
        }

        /// <summary>
        /// The filename that will be used if the write path is the Logs directory. May be null, or empty if same as Filename
        /// </summary>
        public string AltFilename
        {
            get => _altFilename;
            set
            {
                if (_altFilename == value || ReadOnly) return;

                if (value == null)
                    throw new ArgumentNullException(nameof(AltFilename));

                //The reserve filename should never be the same value as the alternate filename
                if (ComparerUtils.FilenameComparer.Equals(AltFilename, ReserveFilename))
                    ReserveFilename = null;

                _altFilename = value;
            }
        }

        /// <summary>
        /// The filename that will serve as the replacement filename if the alternate filename needs to be renamed due to a conflict
        /// </summary>
        public string ReserveFilename { get; private set; }

        /// <summary>
        /// Given the last available current filename, and the property assign AltFilename, this method returns the option not represented as the current path
        /// </summary>
        /// <returns>The filename that is either the last available current filename, or the alt filename depending on the assignment of CurrentFilename.
        /// <br>If all options refer to the current path, or the unused path is not defined - this method returns null</br></returns>
        internal string GetUnusedFilename()
        {
            string filename = CurrentFilename;

            if (ContainsTag(UtilityConsts.PropertyTag.CONFLICT))
                filename = FileUtils.RemoveBracketInfo(filename);

            if (string.IsNullOrEmpty(AltFilename) || ComparerUtils.FilenameComparer.Equals(filename, AltFilename))
            {
                if (ComparerUtils.FilenameComparer.Equals(filename, ReserveFilename)) //Both reserve, and alt filename are either used, or unavailable
                    return null;
                return ReserveFilename;
            }
            return AltFilename;
        }

        /// <summary>
        /// Ensures that current file path info is unique for the current log file
        /// </summary>
        public void EnsurePathDoesNotConflict()
        {
            string filename = CurrentFilename;

            if (ContainsTag(UtilityConsts.PropertyTag.CONFLICT))
                filename = FileUtils.RemoveBracketInfo(filename);

            if (!pathWillConflict(filename))
            {
                CurrentFilename = filename;
                RemoveTag(UtilityConsts.PropertyTag.CONFLICT);
                return;
            }

            //First - check whether we can use the other available filename
            string secondaryFilename = GetUnusedFilename();

            if (secondaryFilename != null && !pathWillConflict(secondaryFilename))
            {
                RemoveTag(UtilityConsts.PropertyTag.CONFLICT);
                CurrentFilename = secondaryFilename;
                return;
            }

            int availableDesignation = getAvailableConflictDesignation(filename);

            AddTag(UtilityConsts.PropertyTag.CONFLICT);
            CurrentFilename = FileUtils.ApplyBracketInfo(filename, availableDesignation.ToString());
            UtilityLogger.Log("Conflicting filename is now resolved");
        }

        private int getAvailableConflictDesignation(string filename)
        {
            //If that does not resolve the conflict, apply bracket info to the filename
            IEnumerable<LogID> results = LogID.FindAll(filename, CompareOptions.CurrentFilename | CompareOptions.IgnoreBracketInfo);

            List<byte> takenDesignations = new List<byte>();
            IEnumerable<LogID> conflictedLogFiles = results.Where(logFile => !logFile.Equals(ID)
                                                                          && logFile.Properties.HasFolderPath(CurrentFolderPath)
                                                                          && logFile.Properties.ContainsTag(UtilityConsts.PropertyTag.CONFLICT));

            foreach (LogID logFile in conflictedLogFiles)
            {
                string bracketInfo = FileUtils.GetBracketInfo(logFile.Properties.CurrentFilename);

                if (!byte.TryParse(bracketInfo, out byte currentDesignation))
                {
                    UtilityLogger.LogWarning("Unable to parse conflict designation from filename");
                    UtilityLogger.LogWarning("FilePath: " + logFile.Properties.CurrentFilename + " INFO: " + bracketInfo);
                    continue;
                }
                takenDesignations.Add(currentDesignation);
            }

            takenDesignations.Sort();

            int designationCount = takenDesignations.Count;

            UtilityLogger.LogWarning("Path conflict detected");

            int availableDesignation = -1;

            if (designationCount == 0 || takenDesignations[0] > 1)
            {
                availableDesignation = 1;
            }
            else if (designationCount == 1) //Must be valued 1 or 0, because of above check
            {
                availableDesignation = 2;
            }
            else
            {
                for (int i = 1; i < takenDesignations.Count; i++)
                {
                    byte value1 = takenDesignations[i - 1];
                    byte value2 = takenDesignations[i];

                    if (value2 - value1 > 1) //Check for value gaps
                    {
                        availableDesignation = value1 + 1;
                        break;
                    }
                }

                if (availableDesignation == -1) //There are no gaps - assign a new highest designation
                    availableDesignation = takenDesignations[designationCount - 1] + 1;
            }
            return availableDesignation;
        }

        private bool pathWillConflict(string filename)
        {
            var results = LogID.FindAll(filename, CompareOptions.All);

            //Search through all of the results for conflicting filepaths
            return results.Any(logFile => !logFile.Equals(ID) && logFile.Properties.HasFolderPath(CurrentFolderPath));
        }

        public string PreferredFileExt = FileExt.DEFAULT;

        public void ChangeFilename(string newFilename)
        {
            if (newFilename == null)
                throw new ArgumentNullException(nameof(newFilename));

            newFilename = FileUtils.RemoveExtension(newFilename).Trim();

            if (newFilename == string.Empty)
                throw new ArgumentException("Filename cannot be empty");

            UpdateCurrentPath(CurrentFolderPath, newFilename);
        }

        public void ChangePath(string newPath)
        {
            newPath = PathUtils.PathWithoutFilename(newPath, out string newFilename);

            if (newPath == null)
                throw new ArgumentException("Directory provided cannot be null");

            newPath = GetContainingPath(newPath);

            //Any log file that becomes part of the Logs folder directory should use its alt filename by default
            bool useAltFilename = !string.IsNullOrEmpty(AltFilename) && LogsFolder.IsCurrentPath(newPath);

            if (useAltFilename)
            {
                if (newFilename != null)
                    UtilityLogger.LogWarning("Provided filename ignored - using alternate filename instead");

                newFilename = AltFilename;
            }
            else if (string.IsNullOrWhiteSpace(newFilename))
            {
                newFilename = CurrentFilename;
            }

            UpdateCurrentPath(newPath, newFilename);
        }

        internal void UpdateCurrentPath(string path, string filename)
        {
            using (FileLock.Acquire())
            {
                string lastFilePath = CurrentFilePath;

                bool usingAltFilename = ComparerUtils.FilenameComparer.Equals(filename, AltFilename);

                //Cache the last non-alternate filename in order to go back to it when the alt filename is no longer necessary
                if (!usingAltFilename)
                    ReserveFilename = filename;

                CurrentFilename = filename;
                CurrentFolderPath = path;

                EnsurePathDoesNotConflict();

                bool changesPresent = !PathUtils.PathsAreEqual(CurrentFilePath, lastFilePath);

                if (changesPresent)
                {
                    FileExists = File.Exists(CurrentFilePath);
                    LastKnownFilePath = CurrentFilePath;
                    NotifyPathChanged();
                }
            }
        }
    }
}
