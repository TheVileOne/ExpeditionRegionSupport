using LogUtils.Enums;
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
        private LogFilename _filename;
        private LogFilename _altFilename;
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
        /// The filename that will be used in the typical write path for the log file
        /// </summary>
        public LogFilename Filename
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
        /// The filename that will be used if the write path is the Logs directory. May be null
        /// </summary>
        public LogFilename AltFilename
        {
            get => _altFilename;
            set
            {
                if (_altFilename == value || ReadOnly) return;

                if (value == null)
                    throw new ArgumentNullException(nameof(AltFilename));

                _altFilename = value;
                UpdateReserveFilename();
            }
        }

        /// <summary>
        /// The filename that will serve as the replacement filename if the alternate filename needs to be renamed due to a conflict
        /// </summary>
        public LogFilename ReserveFilename { get; private set; }

        /// <summary>
        /// The active filename of the log file (without file extension)
        /// </summary>
        public LogFilename CurrentFilename { get; private set; }

        /// <summary>
        /// The active filepath of the log file (with filename)
        /// </summary>
        public string CurrentFilePath => Path.Combine(CurrentFolderPath, CurrentFilename.WithExtension());

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
        /// Given the last available current filename, and the property assign AltFilename, this method returns the option not represented as the current path
        /// </summary>
        /// <returns>The filename that is either the last available current filename, or the alt filename depending on the assignment of CurrentFilename.
        /// <br>If all options refer to the current path, or the unused path is not defined - this method returns null</br></returns>
        internal LogFilename GetFallbackFilename()
        {
            string filename = CurrentFilename;

            if (ContainsTag(UtilityConsts.PropertyTag.CONFLICT))
                filename = FileUtils.RemoveBracketInfo(filename);

            LogFilename result = null;
            if (AltFilename != null)
            {
                //When the current filename is equal to the alternate filename, the reserve filename becomes the target
                if (AltFilename.Equals(filename))
                {
                    if (!AltFilename.Equals(ReserveFilename))
                        result = ReserveFilename; //Target ReserveFilename, because it is either available, and not the current filename
                }
                else
                {
                    result = AltFilename; //Target AltFilename, because it is available, and not the current filename
                }
            }

            //When AltFilename is unavailable, no other filename will be available
            return result;
        }

        /// <summary>
        /// Ensures that current file path info is unique for the current log file
        /// </summary>
        internal void EnsurePathDoesNotConflict()
        {
            string filename = CurrentFilename;

            if (ContainsTag(UtilityConsts.PropertyTag.CONFLICT))
                filename = FileUtils.RemoveBracketInfo(filename);

            if (!pathWillConflict(filename))
            {
                CurrentFilename = new LogFilename(filename, CurrentFilename.Extension);
                RemoveTag(UtilityConsts.PropertyTag.CONFLICT);
                return;
            }

            //LogUtils supports specification of a second filename - not all log files may have a second filename specified. For the ones that do have one,
            //we can check to see if we can resolve the conflict through it. If the current filename is that second filename, it will seek the filename
            //before that filename was used
            LogFilename filenameFallback = GetFallbackFilename();

            if (filenameFallback != null && !pathWillConflict(filenameFallback))
            {
                RemoveTag(UtilityConsts.PropertyTag.CONFLICT);
                CurrentFilename = filenameFallback;
                return;
            }

            int availableDesignation = getAvailableConflictDesignation(filename);

            filename = FileUtils.ApplyBracketInfo(filename, availableDesignation.ToString()); //Does not contain file extension information yet

            CurrentFilename = new LogFilename(filename, CurrentFilename.Extension);
            AddTag(UtilityConsts.PropertyTag.CONFLICT);

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

        /// <summary>
        /// Allows the filename, or file extension to be changed
        /// </summary>
        /// <remarks>This will not initiate a file move, or rename any file</remarks>
        /// <param name="newFilename">The new filename</param>
        /// <exception cref="ArgumentException">The filename is null, empty, or contains invalid characters</exception>
        public void ChangeFilename(string newFilename)
        {
            if (PathUtils.IsEmpty(FileUtils.RemoveExtension(newFilename)))
                throw new ArgumentException("Filename cannot be null, or empty");

            UpdateCurrentPath(CurrentFolderPath, new LogFilename(newFilename));
        }

        /// <summary>
        /// Allows the filepath (filename optional) to be changed
        /// </summary>
        /// <remarks>This will not initiate a file move, or rename any file</remarks>
        /// <param name="newPath">The new path</param>
        /// <exception cref="ArgumentException">The directory is null, empty, or contains invalid characters</exception>
        public void ChangePath(string newPath)
        {
            newPath = PathUtils.PathWithoutFilename(newPath, out string filename);

            if (PathUtils.IsEmpty(newPath))
                throw new ArgumentException("Path cannot be null, or empty");

            LogFilename newFilename;
            newPath = GetContainingPath(newPath);
            newFilename = filename == null ? CurrentFilename : new LogFilename(filename);

            UpdateCurrentPath(newPath, newFilename);
        }

        internal void UpdateCurrentPath(string path, LogFilename filename)
        {
            using (FileLock.Acquire())
            {
                bool hasConflictDetails = ContainsTag(UtilityConsts.PropertyTag.CONFLICT);

                if (!CurrentFilename.Equals(filename, hasConflictDetails))
                {
                    string currentFilenameBase = CurrentFilename;

                    //Make sure we don't assign bracket info for the reserve filename
                    if (hasConflictDetails)
                        currentFilenameBase = FileUtils.RemoveBracketInfo(CurrentFilename);

                    LogFilename reserveFilename = null;

                    //Avoid assigning AltFilename as a reserve filename
                    if (AltFilename != null)
                    {
                        if (!AltFilename.Equals(currentFilenameBase)) //Assign existing filename as the reserve
                            reserveFilename = new LogFilename(currentFilenameBase, CurrentFilename.Extension);
                        else if (!AltFilename.Equals(filename)) //Assign incoming filename as the reserve
                            reserveFilename = filename;
                    }
                    else
                    {
                        reserveFilename = filename;
                    }

                    if (reserveFilename != null)
                        ReserveFilename = reserveFilename;
                }

                string lastFilePath = CurrentFilePath;

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

        internal void UpdateReserveFilename()
        {
            if (CurrentFilename == null) return; //ReserveFilename wont be assigned yet

            bool hasConflictDetails = ContainsTag(UtilityConsts.PropertyTag.CONFLICT);

            string currentFilenameBase = CurrentFilename;

            //Make sure we don't assign bracket info for the reserve filename
            if (hasConflictDetails)
                currentFilenameBase = FileUtils.RemoveBracketInfo(CurrentFilename);

            if (ReserveFilename != null && ReserveFilename.Equals(AltFilename)) //AltFilename and ReserveFilename must be different
                ReserveFilename = null;

            if (AltFilename == null || !AltFilename.Equals(currentFilenameBase))
                ReserveFilename = new LogFilename(currentFilenameBase, CurrentFilename.Extension);
        }
    }
}
