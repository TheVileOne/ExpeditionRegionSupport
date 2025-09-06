using BepInEx.Logging;
using LogUtils.Collections;
using LogUtils.Diagnostics.Tools;
using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Helpers;
using LogUtils.Helpers.Comparers;
using LogUtils.Helpers.FileHandling;
using LogUtils.Properties.Custom;
using LogUtils.Properties.Formatting;
using LogUtils.Requests;
using LogUtils.Threading;
using LogUtils.Timers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataFields = LogUtils.UtilityConsts.DataFields;
using RainWorldPath = LogUtils.Helpers.Paths.RainWorld;

namespace LogUtils.Properties
{
    public partial class LogProperties : IEquatable<LogProperties>
    {
        /// <summary>
        /// The latest properties version recognized by LogUtils
        /// </summary>
        /// <remarks>Major&lt;Reserved for LogUtils&gt; Minor (or less)&lt;Reserved for mod usage&gt;</remarks>
        public static readonly Version LatestVersion = new Version(0, 5, 0);

        public static PropertyDataController PropertyManager => UtilityCore.PropertyManager;

        public CustomLogPropertyCollection CustomProperties = new CustomLogPropertyCollection();

        /// <summary>
        /// A prioritized order of process actions that must be applied to a message string before logging it to file 
        /// </summary>
        public LogRuleCollection Rules;

        /// <summary>
        /// Events triggers at the start, or the end of a log session
        /// </summary>
        public event LogStreamEventHandler OnLogSessionStart, OnLogSessionFinish;

        /// <summary>
        /// Ensures thread safety while accessing the log file
        /// </summary>
        public FileLock FileLock;

        /// <summary>
        /// This field contains the last known LogRequest handle state for this LogID, particularly the rejection status, and the reason for rejection of the request
        /// </summary>
        public LogRequestRecord HandleRecord;

        public LogProfiler Profiler = new LogProfiler();

        /// <summary>
        /// The log file has been created, its initialization process has run successfully, and it isn't adding to stale log file data 
        /// </summary>
        public bool LogSessionActive { get; internal set; }

        /// <summary>
        /// The amount of messages logged to file, or stored in the WriteBuffer since the last logging session was started
        /// </summary>
        public uint MessagesHandledThisSession;

        /// <summary>
        /// A list of persistent FileStreams known to be open for this log file
        /// </summary>
        public List<PersistentLogFileHandle> PersistentStreamHandles = new List<PersistentLogFileHandle>();

        /// <summary>
        /// Indicates that this instance was read from file, but one or more fields could not be processed
        /// </summary>
        public bool ProcessedWithErrors;

        private ScheduledEvent readOnlyRestoreEvent;
        private ScheduledEvent recentlyCreatedCutoffEvent;

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

                        Action setReadonly = new Action(() =>
                        {
                            ReadOnly = true;
                        });

                        readOnlyRestoreEvent = UtilityCore.Scheduler.Schedule(setReadonly, frameInterval: disable_frames_allowed, invokeLimit: 1);
                    }
                    UtilityLogger.Logger.LogDebug(reportMessage);
                }
                else
                {
                    //UtilityLogger.Log($"Read Only mode enabled for {ID}");

                    readOnlyRestoreEvent?.Cancel();
                    readOnlyRestoreEvent = null;
                }
                _readOnly = value;
            }
        }

        /// <summary>
        /// A flag that indicates that a low amount of frames (less than or equal to 10) have passed since instance was created
        /// </summary>
        public bool IsNewInstance { get; private set; }

        /// <summary>
        /// Indicates that the startup routine for this log file should not be run
        /// </summary>
        internal bool SkipStartupRoutine;

        /// <summary>
        /// Contains messages that have passed all validation checks, and are waiting to be written to file
        /// </summary>
        public MessageBuffer WriteBuffer = new MessageBuffer();

        #region Properties
        /// <summary>
        /// The earliest period that the log file may start a new log session through a log event
        /// It is recommended to keep at the earliest possible write period, or a period that is close to when the log file is used by a mod's logger
        /// </summary>
        public SetupPeriod AccessPeriod = SetupPeriod.Pregame;

        /// <summary>
        /// Should the logging system handle requests targeting this log file
        /// </summary>
        public bool AllowLogging = true;

        /// <summary>
        /// A flag that indicates whether a log session can be, or already is established
        /// </summary>
        public bool CanBeAccessed => LogSessionActive || RWInfo.LatestSetupPeriodReached >= AccessPeriod;

        private LogID _id;
        private string _idValue;
        private ManualLogSource _logSource;
        private bool _readOnly;
        private Version _version = LatestVersion;
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

        /// <summary>
        /// List of targeted ConsoleIDs to send requests to when logging to file
        /// </summary>
        public ValueCollection<ConsoleID> ConsoleIDs;

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
        public Version Version
        {
            get => _version;
            set
            {
                if (_version == value) return;

                if (value == null)
                    throw new ArgumentNullException(nameof(Version));

                //Detect outdated versions, updating to the latest major revision
                if (LatestVersion.Major > value.Major)
                    value = value.Bump(VersionCode.Major, LatestVersion.Major - value.Major);

                ReadOnly = false; //Updating the version exposes LogProperties to changes
                _version = value;
            }
        }

        /// <summary>
        /// A flag, when true, indicates it is not safe to attempt to receive write access, or write directly to the log file
        /// </summary>
        public bool IsWriteRestricted;

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
        #endregion

        public LogProperties(string filename, string relativePathNoFile = UtilityConsts.PathKeywords.STREAMING_ASSETS) : this(FileExtension.Remove(filename), filename, relativePathNoFile)
        {
        }

        internal LogProperties(string propertyID, string filename, string relativePathNoFile = UtilityConsts.PathKeywords.STREAMING_ASSETS)
        {
            FileLock = new FileLock(new Lock.ContextProvider(() => ID));

            ReadOnlyProvider readOnlyProvider = new ReadOnlyProvider(() => ReadOnly);

            Rules = new LogRuleCollection(readOnlyProvider);
            ConsoleIDs = new ValueCollection<ConsoleID>(readOnlyProvider);

            UtilityLogger.DebugLog("Generating properties for " + propertyID);
            UtilityLogger.Log("Generating properties for " + propertyID);
            _idValue = propertyID;

            Filename = new LogFilename(filename);
            FolderPath = GetContainingPath(relativePathNoFile);

            CurrentFilename = ReserveFilename = Filename;
            CurrentFolderPath = OriginalFolderPath = FolderPath;

            EnsurePathDoesNotConflict();
            LastKnownFilePath = CurrentFilePath;

            IDHash = CreateIDHash(_idValue, OriginalFolderPath);

            const int framesUntilCutoff = 10; //Number of frames before instance is no longer considered a 'new' instance

            IsNewInstance = true;

            Action onCreationCutoffReached = new Action(() =>
            {
                IsNewInstance = false;
                recentlyCreatedCutoffEvent = null;
            });

            recentlyCreatedCutoffEvent = UtilityCore.Scheduler.Schedule(onCreationCutoffReached, frameInterval: framesUntilCutoff, invokeLimit: 1);

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

            OnLogSessionStart += onLogSessionStart;
            OnLogSessionFinish += onLogSessionFinish;
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

        private void onLogSessionStart(LogStreamEventArgs e)
        {
            if (IsWriteRestricted) return;

            if (IntroMessage != null)
                e.Writer.WriteLine(IntroMessage);

            if (ShowIntroTimestamp)
                e.Writer.WriteLine($"[{DateTime.Now}]");

            Profiler.Start();
        }

        private void onLogSessionFinish(LogStreamEventArgs e)
        {
            if (IsWriteRestricted) return;

            if (OutroMessage != null)
                e.Writer.WriteLine(OutroMessage);

            if (ShowOutroTimestamp)
                e.Writer.WriteLine($"[{DateTime.Now}]");

            Profiler.Stop();
        }

        public void AddTag(string tag)
        {
            if (Tags == null)
            {
                Tags = [tag];
                return;
            }

            if (!Tags.Contains(tag))
            {
                Array.Resize(ref Tags, Tags.Length + 1);
                Tags[Tags.Length - 1] = tag;
            }
        }

        public void RemoveTag(string tag)
        {
            if (Tags == null || !Tags.Contains(tag)) return;

            var list = Tags.ToList(); //Convert to list for better thread safety

            list.Remove(tag);
            Tags = list.ToArray();
        }

        public bool ContainsTag(string tag)
        {
            return Tags != null && Tags.Contains(tag);
        }

        public FileStatus CreateTempFile(bool copyOnly = false)
        {
            if (!File.Exists(LastKnownFilePath))
                return FileStatus.NoActionRequired;

            ReplacementFilePath = Path.ChangeExtension(LastKnownFilePath, FileExt.TEMP);

            using (FileLock.Acquire())
            {
                FileStatus status;
                if (copyOnly)
                {
                    FileLock.SetActivity(FileAction.Copy);
                    status = LogFile.Copy(LastKnownFilePath, ReplacementFilePath, true);
                }
                else
                {
                    FileLock.SetActivity(FileAction.Move);
                    status = LogFile.Move(LastKnownFilePath, ReplacementFilePath, true);
                }

                if (status == FileStatus.MoveComplete || status == FileStatus.CopyComplete)
                    BackupListener.OnTempFileCreated(ID);

                return status;
            }
        }

        public void RemoveTempFile()
        {
            FileUtils.TryDelete(ReplacementFilePath, "Unable to delete temporary file");
        }

        /// <summary>
        /// Initiates the routine that applies at the start of a log session. Handle initial file write operations through this process
        /// </summary>
        public void BeginLogSession()
        {
            if (LogSessionActive || RWInfo.IsShuttingDown) return;

            LogID logID = ID;
            UtilityLogger.Log($"Attempting to start log session [{logID}]");

            try
            {
                using (FileLock.Acquire())
                {
                    FileLock.SetActivity(FileAction.SessionStart);

                    using (FileStream stream = LogFile.Create(logID))
                    {
                        FileExists = stream != null;

                        if (FileExists && stream.CanWrite)
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

            //Handle all pending requests, or buffered content before ending the session
            UtilityCore.RequestHandler.ProcessRequests(logID);

            UtilityLogger.Log($"Log session ended [{logID}]");

            if (LogFilter.FilteredStrings.TryGetValue(logID, out List<FilteredStringEntry> filter))
                filter.RemoveAll(entry => entry.Duration == FilterDuration.OnClose);

            MessagesHandledThisSession = 0;

            if (!FileExists)
            {
                LogSessionActive = false;
                return;
            }

            try
            {
                using (FileLock.Acquire())
                {
                    FileLock.SetActivity(FileAction.SessionEnd);

                    using (FileStream stream = LogFile.OpenNoCreate(logID))
                    {
                        FileExists = stream != null;

                        if (FileExists && stream.CanWrite)
                        {
                            using (StreamWriter writer = new StreamWriter(stream))
                            {
                                OnLogSessionFinish(new LogStreamEventArgs(logID, writer));
                            }
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
        /// Triggers the <see cref="UtilityEvents.OnMovePending"/> event
        /// </summary>
        /// <param name="movePath">The pending log path for this instance (include filename with extension if filename has changed)</param>
        public void NotifyPendingMove(string movePath)
        {
            movePath = PathUtils.PathWithoutFilename(movePath, out string filename);
            UtilityEvents.OnMovePending?.Invoke(new LogMovePendingEventArgs(this, movePath, filename));
        }

        /// <summary>
        /// Triggers the <see cref="UtilityEvents.OnMoveAborted"/> event
        /// </summary>
        public void NotifyPendingMoveAborted()
        {
            UtilityEvents.OnMoveAborted?.Invoke(new Events.LogEventArgs(this));
        }

        /// <summary>
        /// The hashcode representing the log filepath at the time of instantiation
        /// </summary>
        /// <remarks>This value is intended to be a unique identifier for this LogProperties instance, and will not change even if the file metadata changes</remarks>
        internal readonly int IDHash = 0;

        /// <summary>
        /// The hashcode produced by the write string cached when properties are read from file
        /// </summary>
        /// <remarks>If the value remains at zero, it means that the properties instance hasn't been updated</remarks>
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

        /// <inheritdoc/>
        public bool Equals(LogProperties other)
        {
            return other != null && IDHash.Equals(other.IDHash);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return IDHash;
        }

        public LogPropertyData ToData(List<CommentEntry> comments = null)
        {
            return new LogPropertyData(ToDictionary(), comments);
        }

        public LogPropertyStringDictionary ToDictionary()
        {
            string[] oldTags = Tags;

            //This tag does not need to be saved to file - leave it out of the dictionary
            RemoveTag(UtilityConsts.PropertyTag.CONFLICT);

            //Restore the tags array here
            string[] dataTags = Tags;
            Tags = oldTags;

            #pragma warning disable IDE0055 //Fix formatting
            var fields = new LogPropertyStringDictionary
            {
                [DataFields.LOGID]                = ID.Value,
                [DataFields.FILENAME]             = Filename.WithExtension(),
                [DataFields.ALTFILENAME]          = AltFilename?.WithExtension(),
                [DataFields.VERSION]              = Version.ToString(),
                [DataFields.CONSOLEIDS]           = string.Join(",", ConsoleIDs),
                [DataFields.TAGS]                 = dataTags != null ? string.Join(",", dataTags) : string.Empty,
                [DataFields.LOGS_FOLDER_AWARE]    = LogsFolderAware.ToString(),
                [DataFields.LOGS_FOLDER_ELIGIBLE] = LogsFolderEligible.ToString(),
                [DataFields.SHOW_LOGS_AWARE]      = ShowLogsAware.ToString(),
                [DataFields.PATH]                 = PathUtils.GetPathKeyword(FolderPath) ?? FolderPath,
                [DataFields.ORIGINAL_PATH]        = PathUtils.GetPathKeyword(OriginalFolderPath) ?? OriginalFolderPath,
                [DataFields.LAST_KNOWN_PATH]      = LastKnownFilePath,
                [DataFields.Intro.MESSAGE]        = IntroMessage,
                [DataFields.Intro.TIMESTAMP]      = ShowIntroTimestamp.ToString(),
                [DataFields.Outro.MESSAGE]        = OutroMessage,
                [DataFields.Outro.TIMESTAMP]      = ShowOutroTimestamp.ToString(),

                [DataFields.Rules.HEADER] = string.Empty //Not an actual property field
            };
            #pragma warning restore IDE0055 //Fix formatting

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

        /// <inheritdoc/>
        public override string ToString()
        {
            return GetWriteString();
        }

        internal string[] GetFilenamesToCompare(CompareOptions compareOptions)
        {
            List<string> compareStrings = new List<string>();

            if ((compareOptions & CompareOptions.ID) != 0)
                addString(_idValue);
            if ((compareOptions & CompareOptions.Filename) != 0)
                addString(Filename);
            if ((compareOptions & CompareOptions.CurrentFilename) != 0)
            {
                string filename = CurrentFilename;
                if (ContainsTag(UtilityConsts.PropertyTag.CONFLICT) && (compareOptions & CompareOptions.IgnoreBracketInfo) != 0)
                    filename = FileUtils.RemoveBracketInfo(filename);

                addString(filename);
            }
            if ((compareOptions & CompareOptions.AltFilename) != 0)
                addString(AltFilename);

            void addString(string value)
            {
                if (!string.IsNullOrEmpty(value))
                    compareStrings.Add(value);
            }

            return compareStrings.ToArray();
        }

        public string GetWriteString(List<CommentEntry> comments = null)
        {
            LogPropertyStringDictionary dataFields = ToDictionary();

            return dataFields.ToString(comments, true);
        }

        /// <summary>
        /// Compares a filename against one, or more filename fields controlled by the properties instance
        /// </summary>
        /// <remarks>Filename is not case sensitive; file extension is unused</remarks>
        /// <param name="filename">The filename to compare</param>
        /// <param name="compareOptions">Represents options for specific filename fields</param>
        public bool HasFilename(string filename, CompareOptions compareOptions)
        {
            if (filename != null)
            {
                filename = FileExtension.Remove(filename);
                return filename.MatchAny(ComparerUtils.StringComparerIgnoreCase, GetFilenamesToCompare(compareOptions));
            }
            return false;
        }

        /// <inheritdoc cref="HasFilename(string, CompareOptions)"/>
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

        /// <summary>
        /// Creates an identifiable hashcode representation of a filename, and path
        /// </summary>
        public static int CreateIDHash(string filename, string path)
        {
            filename ??= string.Empty;
            path ??= string.Empty;

            //TODO: This code needs to be refactored when mod path support is added
            string hashString = Path.Combine(path, filename);
            return hashString.GetHashCode();
        }

        /// <summary>
        /// Resolves a path, or path keyword into a usable log path
        /// </summary>
        public static string GetContainingPath(string relativePath)
        {
            if (relativePath == null)
                return RainWorldPath.StreamingAssetsPath;

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
            return Path.Combine(RainWorldPath.StreamingAssetsPath, relativePath);
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
        IgnoreBracketInfo = 16, //Comparison will ignore bracket info
        Basic = ID | Filename | CurrentFilename,
        All = Basic | AltFilename
    }
}
