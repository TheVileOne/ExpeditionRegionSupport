using BepInEx.Logging;
using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Helpers;
using LogUtils.Helpers.Comparers;
using LogUtils.Helpers.Extensions;
using LogUtils.Helpers.FileHandling;
using LogUtils.Properties.Custom;
using LogUtils.Requests;
using LogUtils.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataFields = LogUtils.UtilityConsts.DataFields;

namespace LogUtils.Properties
{
    public class LogProperties
    {
        public static PropertyDataController PropertyManager => UtilityCore.PropertyManager;
        public CustomLogPropertyCollection CustomProperties = new CustomLogPropertyCollection();

        /// <summary>
        /// Events triggers at the start, or the end of a log session
        /// </summary>
        public event LogStreamEventHandler OnLogSessionStart, OnLogSessionFinish;

        //TODO: This still might not be necessary
        protected event LogMessageEventHandler OnLogReceived; //TODO: Implement

        public void Subscribe(string filename, LogMessageEventHandler logCallback)
        {
            //TODO: Convert filename to either non-temp LogID if it exists, or temp LogID if it doesn't exist
        }

        public void Subscribe(LogID logFile, LogMessageEventHandler logCallback)
        {
        }

        Dictionary<LogID, LogMessageEventHandler> Subscribers;

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
        /// This field contains the last known LogRequest handle state for this LogID, particularly the rejection status, and the reason for rejection of the request
        /// </summary>
        public LogRequestRecord HandleRecord;

        /// <summary>
        /// The log file has been created, its initialization process has run successfully, and it isn't adding to stale log file data 
        /// </summary>
        public bool LogSessionActive { get; internal set; }

        /// <summary>
        /// The earliest period that the log file may start a new log session through a log event
        /// It is recommended to keep at the earliest possible write period, or a period that is close to when the log file is used by a mod's logger
        /// </summary>
        public SetupPeriod AccessPeriod = SetupPeriod.Pregame;

        /// <summary>
        /// A flag that indicates whether a log session can be, or already is established
        /// </summary>
        public bool CanBeAccessed => LogSessionActive || RWInfo.LatestSetupPeriodReached >= AccessPeriod;

        /// <summary>
        /// Indicates that this instance was read from file, but one or more fields could not be processed
        /// </summary>
        public bool ProcessedWithErrors;

        /// <summary>
        /// Indicates that the startup routine for this log file should not be run
        /// </summary>
        internal bool SkipStartupRoutine;

        private ScheduledEvent readOnlyRestoreEvent;
        public ScheduledEvent recentlyCreatedCutoffEvent;

        public bool ReadOnly
        {
            get => _readOnly;
            set
            {
                if (_readOnly == value) return;

                if (!value)
                {
                    const int disable_frames_allowed = 3;

                    string reportMessage = $"Read Only mode for {ID} disabled for the rest of the game loading period";

                    //When ReadOnly is set to false, set a short frame window to allow for edits
                    if (!PropertyManager.IsEditGracePeriod)
                    {
                        reportMessage = $"Read Only mode for {ID} disabled for {disable_frames_allowed} frames";

                        readOnlyRestoreEvent = UtilityCore.Scheduler.Schedule(() =>
                        {
                            ReadOnly = true;
                        }, disable_frames_allowed);
                    }
                    UtilityLogger.Log(LogCategory.Debug, reportMessage);
                }
                else
                {
                    //UtilityLogger.Log($"Read Only mode enabled for {ID}");

                    readOnlyRestoreEvent?.Cancel();
                    readOnlyRestoreEvent = null;
                }
                _readOnly = Rules.ReadOnly = value;
            }
        }

        /// <summary>
        /// A flag that indicates that a low amount of frames (<= 10) have passed since instance was created
        /// </summary>
        public bool IsNewInstance { get; private set; }

        /// <summary>
        /// The ID strings of the mod(s) that control these log properties 
        /// </summary>
        public List<string> AssociatedModIDs = new List<string>();

        private LogID _id;
        private string _idValue;
        private ManualLogSource _logSource;
        private bool _fileExists;
        private bool _readOnly;
        private string _version = "0.5.0";
        private string _filename = string.Empty;
        private string _altFilename = string.Empty;
        private string _folderPath = string.Empty;
        private string _originalFolderPath = string.Empty;
        private bool _logsFolderAware;
        private bool _logsFolderEligible = true;

        private string _introMessage, _outroMessage;
        private bool _showIntroTimestamp, _showOutroTimestamp;
        private bool _showLogsAware;

        /// <summary>
        /// The LogID associated with the log properties
        /// </summary>
        public LogID ID
        {
            get
            {
                //LogID is lazy loaded here, because it would trigger an infinite loop if it were defined in the constructor
                if (_id == null)
                {
                    if (_idValue == null)
                        return null;
                    _id = new LogID(this, _idValue, OriginalFolderPath, false);
                }
                return _id;
            }
        }

        public ManualLogSource LogSource
        {
            get
            {
                if (_logSource == null)
                    _logSource = BepInEx.Logging.Logger.CreateLogSource(LogSourceName ?? _idValue);
                return _logSource;
            }
            set => _logSource = value;
        }

        /// <summary>
        /// The name of the BepInEx logging source associated with this log file
        /// </summary>
        public string LogSourceName;

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
                    throw new ArgumentNullException(nameof(FolderPath) + " cannot be null. Use root, or customroot as a value instead.");
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
                    throw new ArgumentNullException(nameof(OriginalFolderPath) + " cannot be null. Use root, or customroot as a value instead.");
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

        public string PreferredFileExt = FileExt.DEFAULT;

        /// <summary>
        /// A flag, when true, indicates it is not safe to attempt to receive write access, or write directly to the log file
        /// </summary>
        public bool IsWriteRestricted;

        /// <summary>
        /// Ensures thread safety while accessing the log file
        /// </summary>
        public FileLock FileLock = new FileLock();

        /// <summary>
        /// When the log file properties are first initialized, the log file can have its path changed to target the Logs folder if it exists, disabled by default
        /// </summary>
        public bool LogsFolderAware
        {
            get => _logsFolderAware;
            set
            {
                if (_logsFolderAware == value || ReadOnly) return;

                _logsFolderAware = value;

                //During post initialization changes can still be recognized and handled for a short period of time
                if (UtilityCore.IsInitialized)
                    LogsFolder.OnEligibilityChanged(new Events.LogEventArgs(this));
            }
        }

        /// <summary>
        /// A property that informs the utility that the log file should not use the Logs folder
        /// </summary>
        public bool LogsFolderEligible
        {
            get => _logsFolderEligible;
            set
            {
                if (_logsFolderEligible == value || ReadOnly) return;

                _logsFolderEligible = value;

                //During post initialization changes can still be recognized and handled for a short period of time
                if (UtilityCore.IsInitialized)
                    LogsFolder.OnEligibilityChanged(new Events.LogEventArgs(this));
            }
        }

        /// <summary>
        /// A list of persistent FileStreams known to be open for this log file
        /// </summary>
        public List<PersistentLogFileHandle> PersistentStreamHandles = new List<PersistentLogFileHandle>();

        /// <summary>
        /// An array of value identifiers for a specific log
        /// </summary>
        public string[] Tags;

        /// <summary>
        /// A message that will be logged at the start of a log session
        /// </summary>
        public string IntroMessage
        {
            get => _introMessage;
            set
            {
                if (ReadOnly) return;
                _introMessage = value;
            }
        }

        /// <summary>
        /// A message that will be logged at the end of a log session
        /// </summary>
        public string OutroMessage
        {
            get => _outroMessage;
            set
            {
                if (ReadOnly) return;
                _outroMessage = value;
            }
        }

        /// <summary>
        /// A flag that indicates whether a timestamp should be logged at the start of a log session
        /// </summary>
        public bool ShowIntroTimestamp
        {
            get => _showIntroTimestamp;
            set
            {
                if (ReadOnly) return;
                _showIntroTimestamp = value;
            }
        }

        /// <summary>
        /// A flag that indicates whether a timestamp should be logged at the end of a log session
        /// </summary>
        public bool ShowOutroTimestamp
        {
            get => _showOutroTimestamp;
            set
            {
                if (ReadOnly) return;
                _showOutroTimestamp = value;
            }
        }

        /// <summary>
        /// A flag that controls whether log is allowed to be used when RainWorld.ShowLogs is false
        /// </summary>
        public bool ShowLogsAware
        {
            get => _showLogsAware;
            set
            {
                if (ReadOnly) return;
                _showLogsAware = value;
            }
        }

        public LogRule ShowCategories => Rules.FindByType<ShowCategoryRule>();
        public LogRule ShowLineCount => Rules.FindByType<ShowLineCountRule>();

        /// <summary>
        /// A prioritized order of process actions that must be applied to a message string before logging it to file 
        /// </summary>
        public LogRuleCollection Rules = new LogRuleCollection();

        public int MessagesLoggedThisSession;

        public LogProperties(string propertyID, string filename, string relativePathNoFile = UtilityConsts.PathKeywords.STREAMING_ASSETS)
        {
            UtilityLogger.DebugLog("Generating properties for " + propertyID);
            UtilityLogger.Log("Generating properties for " + propertyID);
            _idValue = propertyID;

            Filename = filename;
            FolderPath = GetContainingPath(relativePathNoFile);

            CurrentFilename = Filename;
            CurrentFolderPath = OriginalFolderPath = FolderPath;
            LastKnownFilePath = CurrentFilePath;

            const int framesUntilCutoff = 10; //Number of frames before instance is no longer considered a 'new' instance

            IsNewInstance = true;

            recentlyCreatedCutoffEvent = UtilityCore.Scheduler.Schedule(() =>
            {
                IsNewInstance = false;
                recentlyCreatedCutoffEvent = null;
            }, framesUntilCutoff);

            //Utility packaged rules get added to every log file disabled by default 
            Rules.Add(new ShowCategoryRule(false));
            Rules.Add(new ShowLineCountRule(false));

            CustomProperties.OnPropertyAdded += onCustomPropertyAdded;
            CustomProperties.OnPropertyRemoved += onCustomPropertyRemoved;

            //Some game logs have hardcoded intro messages - Display these messages before any other content
            if (propertyID == UtilityConsts.LogNames.Expedition)
            {
                OnLogSessionStart += (LogStreamEventArgs e) =>
                {
                    e.Writer.WriteLine("[EXPEDITION LOGGER] - " + DateTime.Now);
                };
            }
            else if (propertyID == UtilityConsts.LogNames.JollyCoop)
            {
                OnLogSessionStart += (LogStreamEventArgs e) =>
                {
                    e.Writer.WriteLine(string.Format("############################################\n Jolly Coop Log {0} [DEBUG LEVEL: {1}]\n", 0, RWInfo.Build));
                };
            }

            OnLogSessionStart += LogProperties_OnLogSessionStart;
            OnLogSessionFinish += LogProperties_OnLogSessionFinish;
        }

        public LogProperties(string filename, string relativePathNoFile = UtilityConsts.PathKeywords.STREAMING_ASSETS) : this(filename, filename, relativePathNoFile)
        {
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

        private void LogProperties_OnLogSessionStart(LogStreamEventArgs e)
        {
            if (IsWriteRestricted) return;

            if (IntroMessage != null)
                e.Writer.WriteLine(IntroMessage);

            if (ShowIntroTimestamp)
                e.Writer.WriteLine($"[{DateTime.Now}]");
        }

        private void LogProperties_OnLogSessionFinish(LogStreamEventArgs e)
        {
            if (IsWriteRestricted) return;

            if (OutroMessage != null)
                e.Writer.WriteLine(OutroMessage);

            if (ShowOutroTimestamp)
                e.Writer.WriteLine($"[{DateTime.Now}]");
        }

        public void AddTag(string tag)
        {
            if (Tags == null)
            {
                Tags = new string[] { tag };
                return;
            }

            if (!Tags.Contains(tag))
            {
                Array.Resize(ref Tags, Tags.Length + 1);
                Tags[Tags.Length - 1] = tag;
            }
        }

        public void ChangePath(string newPath)
        {
            newPath = PathUtils.PathWithoutFilename(newPath, out string newFilename);

            bool changesPresent = false;

            lock (FileLock)
            {
                //Compare the current filename to the new filename
                if (newFilename != null && !ComparerUtils.FilenameComparer.Equals(CurrentFilename, newFilename, true))
                {
                    CurrentFilename = FileUtils.RemoveExtension(newFilename);
                    changesPresent = true;
                }

                //Compare the current path to the new path
                if (!PathUtils.PathsAreEqual(CurrentFolderPath, newPath)) //The paths are different
                {
                    CurrentFolderPath = newPath;
                    changesPresent = true;
                }

                //Loggers need to be notified of any changes that might affect managed LogIDs
                if (changesPresent)
                {
                    FileExists = File.Exists(CurrentFilePath);
                    LastKnownFilePath = CurrentFilePath;
                    NotifyPathChanged();
                }
            }
        }

        public void ChangePath(string newPath, string newFilename)
        {
            ChangePath(Path.Combine(newPath, newFilename));
        }

        public FileStatus CreateTempFile(bool copyOnly = false)
        {
            if (!File.Exists(LastKnownFilePath))
                return FileStatus.NoActionRequired;

            ReplacementFilePath = Path.ChangeExtension(LastKnownFilePath, FileExt.TEMP);

            lock (FileLock)
            {
                if (copyOnly)
                {
                    FileLock.SetActivity(ID, FileAction.Copy);
                    return LogFile.Copy(LastKnownFilePath, ReplacementFilePath);
                }

                FileLock.SetActivity(ID, FileAction.Move);
                return LogFile.Move(LastKnownFilePath, ReplacementFilePath);
            }
        }

        public void RemoveTempFile()
        {
            if (!File.Exists(ReplacementFilePath))
                return;

            try
            {
                File.Delete(ReplacementFilePath);
            }
            catch (Exception ex)
            {
                UtilityLogger.LogError(null, new IOException("Unable to delete temporary file", ex));
            }
        }

        /// <summary>
        /// Initiates the routine that applies at the start of a log session. Handle initial file write operations through this process
        /// </summary>
        public void BeginLogSession()
        {
            if (LogSessionActive) return;

            LogID logID = ID;
            UtilityLogger.Log($"Attempting to start log session [{logID}]");

            try
            {
                lock (FileLock)
                {
                    FileLock.SetActivity(logID, FileAction.SessionStart);

                    using (FileStream stream = LogFile.Create(logID))
                    {
                        FileExists = stream != null;

                        if (FileExists)
                        {
                            StreamWriter writer = new StreamWriter(stream);

                            try
                            {
                                OnLogSessionStart(new LogStreamEventArgs(logID, writer));
                            }
                            finally
                            {
                                writer.Close();

                                LogSessionActive = true;
                                LastKnownFilePath = CurrentFilePath;
                            }
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                UtilityLogger.LogError("File handling error occurred", ex);
            }

            if (!LogSessionActive)
                UtilityLogger.LogWarning($"Session failed to start");
        }

        /// <summary>
        /// Initiates the routine that applies at the end of a log session
        /// </summary>
        public void EndLogSession()
        {
            if (!LogSessionActive) return;

            LogID logID = ID;
            UtilityLogger.Log($"Log session ended [{logID}]");

            if (LogFilter.FilteredStrings.TryGetValue(logID, out List<FilteredStringEntry> filter))
                filter.RemoveAll(entry => entry.Duration == FilterDuration.OnClose);

            MessagesLoggedThisSession = 0;

            if (!FileExists)
            {
                LogSessionActive = false;
                return;
            }

            try
            {
                lock (FileLock)
                {
                    FileLock.SetActivity(logID, FileAction.SessionEnd);

                    using (FileStream stream = LogFile.OpenNoCreate(logID))
                    {
                        FileExists = stream != null;

                        if (FileExists)
                        {
                            using (StreamWriter writer = new StreamWriter(stream))
                            {
                                OnLogSessionFinish(new LogStreamEventArgs(logID, writer));
                            };
                        }
                    }
                }
            }
            catch (IOException ex) //Some issue other than the file existing occurred
            {
                UtilityLogger.LogError("File handling error occurred", ex);
            }
            finally
            {
                LogSessionActive = false;
            }
        }

        /// <summary>
        /// Triggers the UtilityEvents.OnPathChanged event for this instance
        /// </summary>
        public void NotifyPathChanged()
        {
            UtilityEvents.OnPathChanged?.Invoke(new Events.LogEventArgs(this));
        }

        /// <summary>
        /// Triggers the UtilityEvents.OnMovePending event for this instance
        /// </summary>
        /// <param name="movePath">The pending log path for this instance (include filename with extension if filename has changed)</param>
        public void NotifyPendingMove(string movePath)
        {
            movePath = PathUtils.PathWithoutFilename(movePath, out string filename);
            UtilityEvents.OnMovePending?.Invoke(new LogMovePendingEventArgs(this, movePath, filename));
        }

        public void NotifyPendingMoveAborted()
        {
            UtilityEvents.OnMoveAborted?.Invoke(new Events.LogEventArgs(this));
        }

        /// <summary>
        /// The hashcode produced by the write string cached when properties are read from file
        /// Note: If the value remains at zero, it means that the properties instance hasn't been updated
        /// </summary>
        internal int WriteHash = 0;

        /// <summary>
        /// Checks whether this instance has writeable data that hasn't yet been written to file
        /// </summary>
        public bool HasModifiedData()
        {
            if (WriteHash == 0) return true;

            int oldHash = WriteHash;
            UpdateWriteHash(); //Update write hash temporarily in order to check state

            if (oldHash != WriteHash)
            {
                WriteHash = oldHash;
                return true;
            }
            return false;
        }

        public void UpdateWriteHash()
        {
            string writeString = GetWriteString();
            WriteHash = writeString.GetHashCode();
        }

        public LogPropertyData ToData(List<CommentEntry> comments = null)
        {
            return new LogPropertyData(ToDictionary(), comments);
        }

        public LogPropertyStringDictionary ToDictionary()
        {
            var fields = new LogPropertyStringDictionary
            {
                [DataFields.LOGID] = ID.value,
                [DataFields.FILENAME] = Filename,
                [DataFields.ALTFILENAME] = AltFilename,
                [DataFields.VERSION] = Version,
                [DataFields.TAGS] = Tags != null ? string.Join(",", Tags) : string.Empty,
                [DataFields.LOGS_FOLDER_AWARE] = LogsFolderAware.ToString(),
                [DataFields.LOGS_FOLDER_ELIGIBLE] = LogsFolderEligible.ToString(),
                [DataFields.SHOW_LOGS_AWARE] = ShowLogsAware.ToString(),
                [DataFields.PATH] = PathUtils.GetPathKeyword(FolderPath) ?? FolderPath,
                [DataFields.ORIGINAL_PATH] = PathUtils.GetPathKeyword(OriginalFolderPath) ?? OriginalFolderPath,
                [DataFields.LAST_KNOWN_PATH] = LastKnownFilePath,
                [DataFields.Intro.MESSAGE] = IntroMessage,
                [DataFields.Intro.TIMESTAMP] = ShowIntroTimestamp.ToString(),
                [DataFields.Outro.MESSAGE] = OutroMessage,
                [DataFields.Outro.TIMESTAMP] = ShowOutroTimestamp.ToString(),

                [DataFields.Rules.HEADER] = string.Empty //Not an actual property field
            };

            fields.Add(ShowLineCount.PropertyString);
            fields.Add(ShowCategories.PropertyString);

            PropertyManager.UnrecognizedFields.TryGetValue(this, out LogPropertyStringDictionary unrecognizedFields);

            bool hasCustomFields = CustomProperties.Any() || unrecognizedFields != null;

            if (hasCustomFields)
            {
                fields[DataFields.CUSTOM] = string.Empty; //Not an actual property field

                if (CustomProperties.Any())
                {
                    //TODO: Check behavior
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

                        //TODO: Don't allow custom property strings to overwrite existing property strings
                        fields.Add(propertyString);
                    }
                }

                if (unrecognizedFields != null)
                {
                    foreach (DictionaryEntry field in unrecognizedFields)
                        fields[(string)field.Key] = (string)field.Value;
                }
            }
            return fields;
        }

        public override string ToString()
        {
            return GetWriteString();
        }

        internal string[] GetFilenamesToCompare(CompareOptions compareOptions)
        {
            if ((compareOptions & CompareOptions.Basic) != 0)
            {
                return new string[3]
                {
                    _idValue,
                    Filename,
                    CurrentFilename,
                };
            }

            if ((compareOptions & CompareOptions.All) != 0)
            {
                return new string[4]
                {
                    _idValue,
                    Filename,
                    CurrentFilename,
                    AltFilename
                };
            }

            List<string> compareStrings = new List<string>();

            if ((compareOptions & CompareOptions.ID) != 0)
                compareStrings.Add(_idValue);
            if ((compareOptions & CompareOptions.Filename) != 0)
                compareStrings.Add(Filename);
            if ((compareOptions & CompareOptions.CurrentFilename) != 0)
                compareStrings.Add(CurrentFilename);
            if ((compareOptions & CompareOptions.AltFilename) != 0)
                compareStrings.Add(AltFilename);
            return compareStrings.ToArray();
        }

        public string GetWriteString(List<CommentEntry> comments = null)
        {
            LogPropertyStringDictionary dataFields = ToDictionary();

            return dataFields.ToString(comments, true);
        }

        /// <summary>
        /// Compares a filename against one, or more filename fields controlled by the properties instance
        /// <br>
        /// Filename is not case sensitive; file extension is unused
        /// </br>
        /// <param name="filename">The filename to compare</param>
        /// <param name="compareOptions">Represents options for specific filename fields</param>
        public bool HasFilename(string filename, CompareOptions compareOptions)
        {
            if (filename != null)
            {
                filename = Path.GetFileNameWithoutExtension(filename);
                return filename.MatchAny(ComparerUtils.StringComparerIgnoreCase, GetFilenamesToCompare(compareOptions));
            }
            return false;
        }

        /// <summary>
        /// Compares a filename against one, or more filename fields controlled by the properties instance
        /// <br>
        /// Filename is not case sensitive; file extension is unused
        /// </br>
        /// </summary>
        /// <param name="filename">The filename to compare</param>
        /// <param name="relativePathNoFile">The filepath to compare. When set to null, the filepath check will be skipped</param>
        /// <param name="compareOptions">Represents options for specific filename fields</param>
        public bool HasFilename(string filename, string relativePathNoFile, CompareOptions compareOptions)
        {
            if (!HasFilename(filename, compareOptions))
                return false;

            if (IsPathWildcard(relativePathNoFile))
                return true;

            return HasFolderPath(GetContainingPath(relativePathNoFile));
        }

        /// <summary>
        /// Compares a folder path to the original, and current folder path fields
        /// </summary>
        /// <param name="relativePathNoFile">The path to compare</param>
        /// <returns>Returns whether a match was found</returns>
        public bool HasFolderPath(string relativePathNoFile)
        {
            return PathUtils.PathsAreEqual(relativePathNoFile, OriginalFolderPath)
                || PathUtils.PathsAreEqual(relativePathNoFile, CurrentFolderPath);
        }

        public static string GetContainingPath(string relativePath)
        {
            if (relativePath == null)
                return Paths.StreamingAssetsPath;

            //Apply some preprocessing to the path based on whether it is a partial, or full path
            string path;
            if (Path.IsPathRooted(relativePath))
            {
                UtilityLogger.Log("Processing a rooted path when expecting a partial one");

                if (Directory.Exists(relativePath)) //As long as it exists, we shouldn't care if it is rooted
                    return relativePath;

                UtilityLogger.Log("Rooted path could not be found. Unrooting...");

                //Unrooting allows us to still find a possibly valid Rain World path
                relativePath = PathUtils.Unroot(relativePath);

                path = Path.GetFullPath(relativePath);

                if (PathUtils.PathRootExists(path))
                {
                    UtilityLogger.Log("Unroot successful");
                    return path;
                }

                path = relativePath; //We don't know where this path is, but we shouldn't default to the Rain World root here 
            }
            else
            {
                path = PathUtils.GetPathFromKeyword(relativePath);
            }

            if (PathUtils.PathRootExists(path)) //No need to change the path when it is already valid
                return path;

            UtilityLogger.Log("Attempting to resolve path");

            //Resolve directory the game supported way if we're not too early to do so (most likely will be too early)
            if (RWInfo.IsRainWorldRunning)
                return AssetManager.ResolveDirectory(path);

            UtilityLogger.Log("Defaulting to custom root. Path check run too early to resolve");

            //This is what AssetManager.ResolveDirectory would have returned as a fallback path
            return Path.Combine(Paths.StreamingAssetsPath, relativePath);
        }

        public static bool IsPathWildcard(string path)
        {
            return path == null;
        }

        public static string ToPropertyString(string name, string value = "")
        {
            return name + ':' + value;
        }
    }

    [Flags]
    public enum CompareOptions
    {
        None = 0,
        ID = 1, //Compare against the ID field
        Filename = 2, //Compare against the Filename field
        CurrentFilename = 4, //Compare against the CurrentFilename field
        AltFilename = 8, //Compare against the AltFilename field
        Basic = ID & Filename & CurrentFilename,
        All = Basic & AltFilename
    }
}
