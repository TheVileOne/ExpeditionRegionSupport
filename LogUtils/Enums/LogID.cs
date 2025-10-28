using LogUtils.Helpers.FileHandling;
using LogUtils.Policy;
using LogUtils.Properties;
using LogUtils.Properties.Formatting;
using LogUtils.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using BepInExPath = LogUtils.Helpers.Paths.BepInEx;

namespace LogUtils.Enums
{
    public class LogID : SharedExtEnum<LogID>, ILogTarget, IEquatable<LogID>
    {
        /// <summary>
        /// Contains path information, and other settings that affect logging behavior 
        /// </summary>
        public LogProperties Properties { get; protected set; }

        /// <summary>
        /// Acts as a permission flag that affects the behavior of loggers, and the handling of logging requests targeting this LogID
        /// </summary>
        public LogAccess Access;

        /// <summary>
        /// Will this LogID be handled as having a local context when passed to a logger
        /// </summary>
        internal bool HasLocalAccess => !IsGameControlled && (Access == LogAccess.FullAccess || Access == LogAccess.Private);

        /// <summary>
        /// Controls whether messages targetting this log file can be handled by a logger
        /// </summary>
        public bool IsEnabled => UtilityCore.IsControllingAssembly && IsInstanceEnabled && (Properties == null || Properties.AllowLogging);

        /// <summary>
        /// A flag that controls whether logging should be permitted for this LogID instance
        /// </summary>
        public bool IsInstanceEnabled = true;

        /// <summary>
        /// A flag that indicates that this represents an existing game-controlled log file
        /// </summary>
        public bool IsGameControlled;

        public static LogID[] RegisteredEntries => LogProperties.PropertyManager.Properties.Select(p => p.ID).ToArray();

        /// <summary>
        /// Creates a new <see cref="LogID"/> instance.
        /// </summary>
        /// <param name="filename">The filename, and optional path to target and use for logging.<br/>
        /// The ExtEnum value will be equivalent to the filename portion of this parameter without the file extension.<br/>
        /// A filename without a path will default to StreamingAssets directory as a path unless an existing LogID with the specified filename is already registered.
        /// </param>
        /// <param name="access">Modifier that affects who may access and use the log file.<br/>
        /// To access a log file you control, use <b>Private/FullAccess</b>. To access a log file you do not control, use <b>RemoteAccessOnly</b>.
        /// </param>
        /// <param name="register">Sets registration state for the ExtEnum.<br/>
        /// <para>Registration affects whether a LogID gets its own properties that write to file on game close. An unregistered LogID will still get its own properties,<br/>
        /// but those properties, and changes to those properties wont be saved to file.</para>
        /// <para>Avoid registering a LogID that is temporary, and your mod is designated for public release.</para>
        /// </param>
        /// <exception cref="ArgumentNullException">Filename provided is null</exception>
        public LogID(string filename, LogAccess access, bool register = false) : this(new PathWrapper(filename), access, register)
        {
        }

        /// <summary>
        /// Creates a new <see cref="LogID"/> instance without attempting to create properties for it.
        /// </summary>
        protected private LogID(string filename) : base(Sanitize(filename)) //Used by ComparisonLogID to bypass LogProperties creation
        {
            InitializeAccess(LogAccess.RemoteAccessOnly);
        }

        /// <summary>
        /// Creates a new <see cref="LogID"/> instance using a filename, and assuming a default/preexisting registered path.
        /// </summary>

        internal LogID(string filename, bool register) : this(filename, null, LogAccess.RemoteAccessOnly, register)
        {
            //Used for LogID tests and to satisfy Activator parameters for SharedExtEnum
        }

        internal LogID(string filename, string fileExt, string path, bool register) : this(filename + fileExt, path, LogAccess.RemoteAccessOnly, register)
        {
            //Used to initialize game-controlled LogIDs
        }

        private LogID(PathWrapper pathData, LogAccess access, bool register) : this(pathData.Filename, pathData.Path, access, register)
        {
            //Intermediary constructor overload
        }

        private LogID(LogProperties properties, PathWrapper pathData, LogAccess access) : this(properties, pathData.Filename, pathData.Path, access)
        {
            //Intermediary constructor overload
        }

        private LogID(LogProperties properties, string filename, string path, LogAccess access, bool register = false) : base(Sanitize(filename), register)
        {
            InitializeAccess(access);
            bool hasGroupTag = properties != null && properties.ContainsTag(UtilityConsts.PropertyTag.LOG_GROUP);

            //Each instance of a group inherits from the group's shared properties
            Properties = !hasGroupTag ? properties : properties.Clone(filename, path);

            if (Properties == null)
                InitializeProperties(filename, path);
        }

        internal LogID(LogProperties properties, bool register) : base(properties.GetRawID(), register)
        {
            InitializeAccess(LogAccess.RemoteAccessOnly); //Log access is presumed to be unavailable to loggers

            PropertyDataController propertyManager = LogProperties.PropertyManager;
            if (Registered && !propertyManager.Exists(properties))
                propertyManager.SetProperties(properties);

            Properties = properties;
        }

        /// <summary>
        /// Creates a new <see cref="LogID"/> instance.
        /// </summary>
        /// <param name="filename">The filename to target, and use for logging.<br/>
        /// The ExtEnum value will be equivalent to the filename without the file extension.
        /// </param>
        /// <param name="relativePathNoFile">The path to the log file.<br/>
        /// Setting to null will default to the StreamingAssets directory as a path unless an existing LogID with the specified filename is already registered.
        /// </param>
        /// <param name="access">Modifier that affects who may access and use the log file.<br/>
        /// To access a log file you control, use <b>Private/FullAccess</b>. To access a log file you do not control, use <b>RemoteAccessOnly</b>.
        /// </param>
        /// <param name="register">Sets registration state for the ExtEnum.<br/>
        /// <para>Registration affects whether a LogID gets its own properties that write to file on game close. An unregistered LogID will still get its own properties,<br/>
        /// but those properties, and changes to those properties wont be saved to file.</para>
        /// <para>Avoid registering a LogID that is temporary, and your mod is designated for public release.</para>
        /// </param>
        public LogID(string filename, string relativePathNoFile, LogAccess access, bool register = false) : base(Sanitize(filename), register)
        {
            InitializeAccess(access);
            InitializeProperties(filename, relativePathNoFile);
        }

        /// <summary>
        /// Creates a new unregistered <see cref="LogID"/> instance associated with a group identifier.
        /// </summary>
        /// <remarks>Log properties will be inherited by the group id <i>unless</i> the specified logging path is already associated with a registered <see cref="LogID"/> instance.</remarks>
        /// <param name="groupID">The group to associate this instance with.</param>
        /// <param name="filename">The filename, and optional path to target and use for logging.<br/>
        /// The ExtEnum value will be equivalent to the filename portion of this parameter without the file extension.<br/>
        /// A filename without a path will default to StreamingAssets directory as a path unless an existing LogID with the specified filename is already registered.
        /// </param>
        /// <param name="access">Modifier that affects who may access and use the log file.<br/>
        /// To access a log file you control, use <b>Private/FullAccess</b>. To access a log file you do not control, use <b>RemoteAccessOnly</b>.
        /// </param>
        public LogID(LogGroupID groupID, string filename, LogAccess access) : this(groupID.Properties, new PathWrapper(filename), access)
        {
        }

        /// <summary>
        /// Creates a new unregistered <see cref="LogID"/> instance associated with a group identifier.
        /// </summary>
        /// <remarks>Log properties will be inherited by the group id <i>unless</i> the specified logging path is already associated with a registered <see cref="LogID"/> instance.</remarks>
        /// <param name="groupID">The group to associate this instance with.</param>
        /// <param name="filename">The filename to target, and use for logging.<br/>
        /// The ExtEnum value will be equivalent to the filename without the file extension.
        /// </param>
        /// <param name="relativePathNoFile">The path to the log file.<br/>
        /// Setting to null will default to the StreamingAssets directory as a path unless an existing LogID with the specified filename is already registered.
        /// </param>
        /// <param name="access">Modifier that affects who may access and use the log file.<br/>
        /// To access a log file you control, use <b>Private/FullAccess</b>. To access a log file you do not control, use <b>RemoteAccessOnly</b>.
        /// </param>
        public LogID(LogGroupID groupID, string filename, string relativePathNoFile, LogAccess access) : this(groupID.Properties, filename, relativePathNoFile, access)
        {
        }

        protected void InitializeAccess(LogAccess initialAccess)
        {
            IsGameControlled = ManagedReference == this ? UtilityConsts.LogNames.NameMatch(Value) : ManagedReference.IsGameControlled;
            Access = IsGameControlled ? LogAccess.FullAccess : initialAccess;
        }

        protected void InitializeProperties(string filename, string logPath)
        {
            Properties = LogProperties.PropertyManager.GetProperties(this, logPath);

            if (Properties == null)
            {
                Properties = new LogProperties(filename, logPath);

                if (Registered)
                    LogProperties.PropertyManager.SetProperties(Properties);
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="LogID"/> instance is equal to the current instance
        /// </summary>
        /// <param name="idOther">The <see cref="LogID"/> instance to compare with the current instance</param>
        /// <param name="doPathCheck">Whether the folder path should also be considered in the equality check</param>
        public bool Equals(LogID idOther, bool doPathCheck)
        {
            if (!Equals(idOther))
                return false;

            //Let the null case here be considered a wildcard path match
            if (Properties == null || idOther.Properties == null)
                return true;

            return !doPathCheck || Properties.HasFolderPath(idOther.Properties.FolderPath);
        }

        public static LogID CreateTemporaryID(string filename, string relativePathNoFile)
        {
            if (IsRegistered(filename, relativePathNoFile))
                throw new InvalidOperationException("Temporary log ID could not be created; a registered log ID already exists.");

            return new LogID(filename, relativePathNoFile, LogAccess.Private);
        }

        /// <summary>
        /// Finds a registered <see cref="LogID"/> instance with the given filename, and path
        /// </summary>
        /// <remarks>Compares ID, Filename, and CurrentFilename fields</remarks>
        /// <param name="filename">The filename to search for</param>
        /// <param name="relativePathNoFile">The filepath to search for. When set to null, any filename match will be returned with custom root being prioritized</param>
        public static LogID Find(string filename, string relativePathNoFile = null)
        {
            return LogProperties.PropertyManager.Properties.Find(filename, relativePathNoFile);
        }

        /// <summary>
        /// Finds all registered <see cref="LogID"/> instance with the given filename
        /// </summary>
        /// <remarks>Compares ID, Filename, and CurrentFilename fields</remarks>
        /// <param name="filename">The filename to search for</param>
        public static IEnumerable<LogID> FindAll(string filename)
        {
            return LogProperties.PropertyManager.Properties.FindAll(filename, CompareOptions.Basic);
        }

        /// <summary>
        /// Finds all registered <see cref="LogID"/> instances with the given filename
        /// </summary>
        /// <param name="filename">The filename to search for</param>
        /// <param name="compareOptions">Represents options that determine which fields to check against</param>
        public static IEnumerable<LogID> FindAll(string filename, CompareOptions compareOptions)
        {
            return LogProperties.PropertyManager.Properties.FindAll(filename, compareOptions);
        }

        /// <summary>
        /// Finds all registered <see cref="LogID"/> instances that match a predicate
        /// </summary>
        /// <param name="predicate">The predicate to match</param>
        public static IEnumerable<LogID> FindAll(Func<LogProperties, bool> predicate)
        {
            return LogProperties.PropertyManager.Properties.FindAll(predicate);
        }

        /// <summary>
        /// Finds all registered <see cref="LogID"/> instances with the given tag
        /// </summary>
        /// <param name="tag">The tag to search for</param>
        public static IEnumerable<LogID> FindByTag(string tag)
        {
            return LogProperties.PropertyManager.Properties.FindByTag(tag);
        }

        /// <summary>
        /// Checks whether file, and path combination matches the file and path information of an existing registered <see cref="LogID"/> instance
        /// </summary>
        /// <param name="filename">The filename to search for</param>
        /// <param name="relativePathNoFile">The filepath to search for. When set to null, any filename match will be returned with custom root being prioritized</param>
        public static bool IsRegistered(string filename, string relativePathNoFile = null)
        {
            return Find(filename, relativePathNoFile) != null;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            if (Properties != null)
                return Properties.GetHashCode();
            return base.GetHashCode();
        }

        internal static void InitializeEnums()
        {
            //File activity monitoring LogID
            FileActivity = new LogID("LogActivity", UtilityConsts.PathKeywords.ROOT, LogAccess.Private, false);

            //This must be called after FileActivity LogID is created
            DebugPolicy.UpdateAllowConditions();

#pragma warning disable IDE0055 //Fix formatting
            //Game-defined LogIDs
            BepInEx    = new LogID(UtilityConsts.LogNames.BepInEx,    FileExt.LOG,  BepInExPath.RootPath, true);
            Exception  = new LogID(UtilityConsts.LogNames.Exception,  FileExt.TEXT, UtilityConsts.PathKeywords.ROOT, true);
            Expedition = new LogID(UtilityConsts.LogNames.Expedition, FileExt.TEXT, UtilityConsts.PathKeywords.STREAMING_ASSETS, true);
            JollyCoop  = new LogID(UtilityConsts.LogNames.JollyCoop,  FileExt.TEXT, UtilityConsts.PathKeywords.STREAMING_ASSETS, true);
            Unity      = new LogID(UtilityConsts.LogNames.Unity,      FileExt.TEXT, UtilityConsts.PathKeywords.ROOT, true);
#pragma warning restore IDE0055 //Fix formatting

            //This log file should only be activated when there is data to log to it
            if (PatcherPolicy.ShowPatcherLog)
            {
                Patcher = new LogID("LogUtils.VersionLoader.log", UtilityConsts.PathKeywords.ROOT, LogAccess.Private, true);
                Patcher.Properties.AccessPeriod = SetupPeriod.Pregame;
                Patcher.Properties.ShowLogTimestamp.IsEnabled = true;
                Patcher.Properties.DateTimeFormat = new DateTimeFormat("yyyy-MM-dd HH:mm:ss -");
            }

            //Throwaway LogID
            NotUsed = new LogID("NotUsed", UtilityConsts.PathKeywords.ROOT, LogAccess.Private, false);

            BepInEx.Properties.AccessPeriod = SetupPeriod.Pregame;
            BepInEx.Properties.AddTag(nameof(BepInEx));
            BepInEx.Properties.ConsoleIDs.Add(ConsoleID.BepInEx);
            BepInEx.Properties.LogSourceName = nameof(BepInEx);
            BepInEx.Properties.AltFilename = new LogFilename(UtilityConsts.LogNames.BepInExAlt, FileExt.LOG);
            BepInEx.Properties.IsWriteRestricted = true;
            BepInEx.Properties.FileExists = true;
            BepInEx.Properties.LogSessionActive = true; //BepInEx log is active before the utility can initialize
            BepInEx.Properties.ShowCategories.IsEnabled = true;

            BepInEx.Properties.Profiler.Start();
            BepInEx.Properties.Rules.Replace(new BepInExHeaderRule(BepInEx.Properties.ShowCategories.IsEnabled));

            Exception.Properties.AccessPeriod = SetupPeriod.RWAwake;
            Exception.Properties.AddTag(nameof(Exception));
            Exception.Properties.LogSourceName = nameof(Exception);
            Exception.Properties.AltFilename = new LogFilename(UtilityConsts.LogNames.ExceptionAlt, FileExt.LOG);

            Expedition.Properties.AccessPeriod = SetupPeriod.ModsInit;
            Expedition.Properties.AddTag(nameof(Expedition));
            Expedition.Properties.LogSourceName = nameof(Expedition);
            Expedition.Properties.AltFilename = new LogFilename(UtilityConsts.LogNames.ExpeditionAlt, FileExt.LOG);
            Expedition.Properties.ShowLogsAware = true;

            JollyCoop.Properties.AccessPeriod = SetupPeriod.ModsInit;
            JollyCoop.Properties.AddTag(nameof(JollyCoop));
            JollyCoop.Properties.LogSourceName = nameof(JollyCoop);
            JollyCoop.Properties.AltFilename = new LogFilename(UtilityConsts.LogNames.JollyCoopAlt, FileExt.LOG);
            JollyCoop.Properties.ShowLogsAware = true;

            Version version = Unity.Properties.Version;

            if (version.Major == 0)
            {
                //It is important that all users have ConsoleIDs field updated for this log file
                Unity.Properties.Version = version.Bump(VersionCode.Major);
            }

            Unity.Properties.AccessPeriod = SetupPeriod.RWAwake;
            Unity.Properties.AddTag(nameof(Unity));
            Unity.Properties.ConsoleIDs.Add(ConsoleID.RainWorld);
            Unity.Properties.ConsoleIDs.Add(ConsoleID.BepInEx); //Unity logs to BepInEx by default
            Unity.Properties.LogSourceName = nameof(Unity);
            Unity.Properties.AltFilename = new LogFilename(UtilityConsts.LogNames.UnityAlt, FileExt.LOG);

            NotUsed.Properties.LogSourceName = UtilityLogger.Logger.SourceName;

            //Initializes part of the recursion detection system that cannot be initialized before LogIDs
            foreach (LogID gameID in GameLogger.LogTargets)
                UtilityCore.RequestHandler.GameLogger.ExpectedRequestCounter[gameID] = 0;
        }

        /// <summary>
        /// Converts a filename input into a LogUtils supported filename
        /// </summary>
        /// <exception cref="ArgumentNullException">Filename provided is null</exception>
        internal static string Sanitize(string filename)
        {
            if (filename == null)
                throw new ArgumentNullException(filename);

            return FileExtension.Remove(filename).Trim();
        }

        public RequestType GetRequestType(ILogHandler handler)
        {
            if (Properties == null)
                return RequestType.Invalid;

            if (IsGameControlled)
                return RequestType.Game;

            LogID handlerID = handler.FindEquivalentTarget(this);

            //Check whether LogID should be handled as a local request, or an outgoing (remote) request. Not being recognized by the handler means that
            //the handler is processing a target specifically made through one of the logging method overloads
            return handlerID != null && handlerID.HasLocalAccess ? RequestType.Local : RequestType.Remote;
        }

        static LogID()
        {
            UtilityCore.EnsureInitializedState();
        }

#pragma warning disable CS1591 //Missing XML comment for publicly visible type or member
        //Rain World LogIDs
        public static LogID BepInEx;
        public static LogID Exception;
        public static LogID Expedition;
        public static LogID JollyCoop;
        public static LogID Unity;

        //LogUtils LogIDs
        internal static LogID FileActivity;
        internal static LogID Patcher;

        /// <summary>An unregistered LogID designed to be used as a throwaway parameter</summary>
        public static LogID NotUsed;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        public static CompositeLogTarget operator |(LogID a, ILogTarget b)
        {
            return LogTarget.Combiner.Combine(a, b);
        }
    }

    /// <summary>
    /// Logger permission values - Assign to a LogID provided to a logger to influence how messages are logged, and handled by that logger
    /// </summary>
    public enum LogAccess
    {
        /// <summary>
        /// LogID can be handled by either local, or remote loggers
        /// </summary>
        FullAccess = 0,
        /// <summary>
        /// LogID is only able to be handled as a remote request from one logger to another
        /// </summary>
        RemoteAccessOnly = 1,
        /// <summary>
        /// LogID can only be handled through a local log request
        /// </summary>
        Private = 2
    }
}
