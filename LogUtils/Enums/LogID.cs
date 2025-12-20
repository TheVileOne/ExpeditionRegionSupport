using LogUtils.Helpers.Comparers;
using LogUtils.Helpers.FileHandling;
using LogUtils.Policy;
using LogUtils.Properties;
using LogUtils.Properties.Formatting;
using LogUtils.Requests;
using LogUtils.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using BepInExPath = LogUtils.Helpers.Paths.BepInEx;

namespace LogUtils.Enums
{
    /// <summary>
    /// An <see cref="ExtEnum{LogID}"/> type representing a log file.
    /// </summary>
    /// <remarks>
    /// Log file properties may be accessed, and changed through the <see cref="Properties"/> field.<br/>
    /// Implements <see cref="ILogTarget"/> interface.<br/>
    /// Note: This type serves as the base class for <see cref="LogGroupID"/>, which is designed for inheritance of log properties, not as a logging target.
    /// </remarks>
    public partial class LogID : SharedExtEnum<LogID>, ILogTarget, IEquatable<LogID>, ILockable
    {
        /// <summary>
        /// Registration may be handled through the <see cref="SharedExtEnum{T}"/> constructor only when no other existing reference to this <see cref="LogID"/> value is present.  
        /// </summary>
        protected override RegistrationStatus RegistrationStage
        {
            get
            {
                RegistrationStatus stage = base.RegistrationStage;

                //Inherit the status when registration is already completed
                if (stage == RegistrationStatus.Completed)
                    return RegistrationStatus.Completed;

                //When we know this instance is the managed reference, signal to complete the registration process
                if (ReferenceEquals(ManagedReference, this) || Properties != null)
                    return RegistrationStatus.Ready;

                //When it is not the same reference, and we know registration is not yet completed, we need to wait
                return RegistrationStatus.WaitingOnSignal;
            }
        }

        private LogProperties _properties;
        /// <summary>
        /// Contains path information, and other settings that affect logging behavior 
        /// </summary>
        public LogProperties Properties
        {
            get => _properties;
            protected set
            {
                _properties = value;
                CompleteRegistration(); //LogID is a class that defers registration in certain situations until properties are assigned
            }
        }

        /// <inheritdoc/>
        public override string Tag
        {
            get
            {
                if (RegistrationStage == RegistrationStatus.Completed && !ReferenceEquals(ManagedReference, this))
                    return ManagedReference.Tag;

                if (Properties != null)
                    return Path.Combine(Properties.OriginalFolderPath, Value);

                return Value;
            }
        }

        /// <summary>
        /// Acts as a permission flag that affects the behavior of loggers, and the handling of logging requests targeting this <see cref="LogID"/> instance
        /// </summary>
        public LogAccess Access;

        /// <summary>
        /// Checks that <see cref="LogID"/> will be handled with a local context when passed to a logger
        /// </summary>
        internal bool HasLocalAccess => !IsGameControlled && (Access == LogAccess.FullAccess || Access == LogAccess.Private);

        /// <summary>
        /// Controls whether messages targetting this log file can be handled by a logger
        /// </summary>
        public bool IsEnabled => UtilityCore.IsControllingAssembly && IsInstanceEnabled && (Properties == null || Properties.AllowLogging);

        /// <summary>
        /// A flag that controls whether logging should be permitted for this <see cref="LogID"/> instance
        /// </summary>
        public bool IsInstanceEnabled = true;

        /// <summary>
        /// A flag that indicates that this represents an existing game-controlled log file
        /// </summary>
        public bool IsGameControlled;

        /// <summary>
        /// Creates a new <see cref="LogID"/> instance.
        /// </summary>
        /// <param name="filename">The filename, and optional path to target and use for logging.<br/>
        /// The ExtEnum value will be equivalent to the filename portion of this parameter without the file extension.<br/>
        /// A filename without a path will default to StreamingAssets directory as a path unless an existing <see cref="LogID"/> with the specified filename is already registered.
        /// </param>
        /// <param name="access">Modifier that affects who may access and use the log file.<br/>
        /// To access a log file you control, use <b>Private/FullAccess</b>. To access a log file you do not control, use <b>RemoteAccessOnly</b>.
        /// </param>
        /// <param name="register">Sets registration state for the ExtEnum.<br/>
        /// <para>Registration affects whether a <see cref="LogID"/> gets its own properties that write to file on game close. An unregistered <see cref="LogID"/> will still get its own properties,<br/>
        /// but those properties, and changes to those properties wont be saved to file.</para>
        /// <para>Avoid registering a <see cref="LogID"/> that is temporary, and your mod is designated for public release.</para>
        /// </param>
        /// <exception cref="ArgumentNullException">Filename provided is null</exception>
        public LogID(string filename, LogAccess access, bool register = false) : this(new PathWrapper(filename), access, register)
        {
        }

        /// <summary>
        /// Creates a new <see cref="LogID"/> instance without attempting to create properties for it.
        /// </summary>
        protected private LogID(string filename) : base(filename) //Used by ComparisonLogID to bypass LogProperties creation
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

        internal LogID(LogProperties properties, bool register) : base(properties.GetRawID(), register)
        {
            Properties = properties; //Wont be null

            if (Registered && !LogProperties.PropertyManager.Exists(properties))
                LogProperties.PropertyManager.SetProperties(properties);

            InitializeAccess(LogAccess.RemoteAccessOnly); //Log access is presumed to be unavailable to loggers
        }

        /// <summary>
        /// Creates a new <see cref="LogID"/> instance.
        /// </summary>
        /// <param name="filename">The filename to target, and use for logging.<br/>
        /// The ExtEnum value will be equivalent to the filename without the file extension.
        /// </param>
        /// <param name="relativePathNoFile">The path to the log file.<br/>
        /// Setting to null will default to the StreamingAssets directory as a path unless an existing <see cref="LogID"/> with the specified filename is already registered.
        /// </param>
        /// <param name="access">Modifier that affects who may access and use the log file.<br/>
        /// To access a log file you control, use <b>Private/FullAccess</b>. To access a log file you do not control, use <b>RemoteAccessOnly</b>.
        /// </param>
        /// <param name="register">Sets registration state for the ExtEnum.<br/>
        /// <para>Registration affects whether a <see cref="LogID"/> gets its own properties that write to file on game close. An unregistered <see cref="LogID"/> will still get its own properties,<br/>
        /// but those properties, and changes to those properties wont be saved to file.</para>
        /// <para>Avoid registering a <see cref="LogID"/> that is temporary, and your mod is designated for public release.</para>
        /// </param>
        public LogID(string filename, string relativePathNoFile, LogAccess access, bool register = false) : base(Sanitize(filename), register)
        {
            InitializeProperties(filename, relativePathNoFile);
            InitializeAccess(access);
        }

        /// <summary>
        /// Creates a new unregistered <see cref="LogID"/> instance associated with a group identifier.
        /// </summary>
        /// <remarks>Log properties will be inherited by the group id <i>unless</i> the specified logging path is already associated with a registered <see cref="LogID"/> instance.</remarks>
        /// <param name="groupID">The group to associate this instance with.</param>
        /// <param name="filename">The filename, and optional path to target and use for logging.<br/>
        /// The ExtEnum value will be equivalent to the filename portion of this parameter without the file extension.<br/>
        /// A filename without a path will default to StreamingAssets directory as a path unless an existing <see cref="LogID"/> with the specified filename is already registered.
        /// </param>
        /// <param name="access">Modifier that affects who may access and use the log file.<br/>
        /// To access a log file you control, use <b>Private/FullAccess</b>. To access a log file you do not control, use <b>RemoteAccessOnly</b>.
        /// </param>
        public LogID(LogGroupID groupID, string filename, LogAccess access) : this(groupID, new PathWrapper(filename), access)
        {
        }

        private LogID(LogGroupID groupID, PathWrapper pathData, LogAccess access) : this(groupID, pathData.Filename, pathData.Path, access)
        {
            //Intermediary constructor overload
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
        /// Setting to null will default to the StreamingAssets directory as a path unless an existing <see cref="LogID"/> with the specified filename is already registered.
        /// </param>
        /// <param name="access">Modifier that affects who may access and use the log file.<br/>
        /// To access a log file you control, use <b>Private/FullAccess</b>. To access a log file you do not control, use <b>RemoteAccessOnly</b>.
        /// </param>
        public LogID(LogGroupID groupID, string filename, string relativePathNoFile, LogAccess access) : base(Sanitize(filename), false)
        {
            //Initialize properties
            var groupProperties = groupID.Properties;

            if (!groupProperties.IsFolderGroup)
            {
                //When there is no group path, the provided path will be used as the log path, and property initialization will be handled like a typical LogID initialization
                InitializeProperties(filename, relativePathNoFile);
            }
            else
            {
                //When there is a group path, the group path will be combined with the provided path as long as the two paths are compatible
                string assignedFolderPath = LogProperties.ApplyGroupPath(groupID, relativePathNoFile);

                //Check registered entries and members of registered log groups
                IEnumerable<LogProperties> searchEntries =
                    LogProperties.PropertyManager.Properties.Union(LogProperties.PropertyManager.GroupProperties.GetMemberProperties());

                var existingProperties = LogProperties.PropertyManager.GetProperties(this, assignedFolderPath, searchEntries);

                if (existingProperties != null)
                    Properties = existingProperties;
                else
                    Properties = groupProperties.Clone(filename, assignedFolderPath);
            }

            //Assign to group
            UtilityLogger.Log("Assigning entry to group");
            if (Properties.Group == null)
            {
                groupID.Assign(this);
            }
            else if (!Properties.Group.Equals(groupID))
            {
                UtilityLogger.LogWarning("Entry already assigned");
            }

            //Initialize log access
            InitializeAccess(access);
        }

        protected void InitializeAccess(LogAccess initialAccess)
        {
            IsGameControlled = ManagedReference == this ? UtilityConsts.LogNames.NameMatch(Value) : ManagedReference.IsGameControlled;
            Access = IsGameControlled ? LogAccess.FullAccess : initialAccess;
        }

        protected void InitializeProperties(string filename, string logPath)
        {
            var properties = LogProperties.PropertyManager.GetProperties(this, logPath);

            if (properties != null)
            {
                Properties = properties;
                return;
            }

            Properties = new LogProperties(filename, logPath);

            if (Registered)
                LogProperties.PropertyManager.SetProperties(Properties);
        }

        /// <inheritdoc/>
        protected override void CompleteRegistration()
        {
            if (RegistrationStage == RegistrationStatus.Completed)
                return;

            if (!ReferenceEquals(ManagedReference, this)) //This reference cannot be trusted to be accurate unless we try to assign with path information available
                ManagedReference = (LogID)UtilityCore.DataHandler.GetOrAssign(this);
            base.CompleteRegistration();
        }

        /// <inheritdoc/>
        public override bool CheckTag(string tag)
        {
            //Adding a file extension is required by the helper, and also protects file information that contains a period in the filename
            string path = PathUtils.PathWithoutFilename(tag + ".txt", out string value);

            bool hasPath = false;
            if (!PathUtils.IsEmpty(path))
            {
                hasPath = true;
                tag = FileExtension.Remove(value);
            }

            bool hasValue = ComparerUtils.StringComparerIgnoreCase.Equals(Value, tag);
            bool checkPath = hasPath && Properties != null;

            //Intentionally not checking Tag property here - it will be more efficient to check the metadata this way and use the Tag value exclusively for lookups
            return hasValue && (!checkPath || PathUtils.PathsAreEqual(path, Properties.OriginalFolderPath));
        }

        /// <inheritdoc cref="Equals(LogID, bool)"/>
        /// <remarks>The log path is included as part of the equality check</remarks>
        public new bool Equals(LogID idOther)
        {
            //This should return an expected result even for implementations that do not include the path
            return Equals(idOther, doPathCheck: true);
        }

        /// <summary>
        /// Determines whether the specified <see cref="LogID"/> instance is equal to the current instance
        /// </summary>
        /// <param name="idOther">The <see cref="LogID"/> instance to compare with the current instance</param>
        /// <param name="doPathCheck">Whether the folder path should also be considered in the equality check</param>
        public virtual bool Equals(LogID idOther, bool doPathCheck)
        {
            if (idOther == null)
                return false;

            //Comparing through Equals will check the IDHash, which involves the path. BaseEquals compares the value part only.
            //Properties being null is considered a wildcard check, and should always return true if there is a value match.
            if (!doPathCheck || Properties == null || idOther.Properties == null)
                return BaseEquals(idOther);

            return base.Equals(idOther);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            if (Properties != null)
                return Properties.GetHashCode();
            return base.GetHashCode();
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

        /// <inheritdoc/>
        public override void Register()
        {
            bool lastKnownState = Registered;
            base.Register();

            if (lastKnownState != Registered)
                OnRegistrationChanged(!lastKnownState);
        }

        /// <inheritdoc/>
        public override void Unregister()
        {
            bool lastKnownState = Registered;
            base.Unregister();

            if (lastKnownState != Registered)
                OnRegistrationChanged(!lastKnownState);
        }

        internal void OnRegistrationChanged(bool registered)
        {
            /*
             * Conditions for secondary registration changes
             * I  - Must be the managed reference - this is to avoid this code applying more than once (through the calling instance, or the managed reference).
             * It makes the most sense to give this responsibility to the managed reference. Inheritors of shared state are aware of their reference state instance,
             * but the opposite is not true. In addition to that reason, the managed reference state will be updated before any shared inheritors would. 
             * II - Must not be run during primary registration - this is indicated by the registration stage set as Completed.
             */
            if (RegistrationStage != RegistrationStatus.Completed || !ReferenceEquals(ManagedReference, this))
                return;

            LogProperties properties = Properties; //Better null safety

            if (properties == null) //Null properties cannot be registered
                return;

            if (registered)
            {
                //This addresses a quirk with how properties are handled when reading from file. They are added to the properties collection despite it being too early
                //to properly establish a registration state.
                if (Properties.InitializedFromFile && LogProperties.PropertyManager.Exists(Properties))
                    return;

                LogProperties.PropertyManager.SetProperties(Properties);
            }
            else
                LogProperties.PropertyManager.RemoveProperties(Properties);
        }

        /// <summary>
        /// Determine if <see cref="LogID"/> reference represents a file, or a log group
        /// </summary>
        public static bool IsGroupType(LogID logID)
        {
            return logID is LogGroupID || (logID is ComparisonLogID comparisonID && comparisonID.RepresentedType == LogIDType.Group);
        }

        internal static string CreateIDValue(string valueBase, LogIDType idType)
        {
            if (idType == LogIDType.Group)
                return LogGroupID.ID_PREFIX + valueBase;
            return Sanitize(valueBase);
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

        /// <inheritdoc/>
        public Lock GetLock()
        {
            if (Properties == null)
            {
                UtilityLogger.LogWarning("Lock was accessed on a LogID without a Properties instance");
                return new Lock();
            }
            return Properties.GetLock();
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

        /// <summary>An unregistered <see cref="LogID"/> designed to be used as a throwaway parameter</summary>
        public static LogID NotUsed;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        public static CompositeLogTarget operator |(LogID a, ILogTarget b)
        {
            return LogTarget.Combiner.Combine(a, b);
        }
    }

    /// <summary>
    /// Logger permission values - Assign to a <see cref="LogID"/> provided to a logger to influence how messages are logged, and handled by that logger
    /// </summary>
    public enum LogAccess
    {
        /// <summary>
        /// The <see cref="LogID"/> can be handled by either local, or remote loggers
        /// </summary>
        FullAccess = 0,
        /// <summary>
        /// The <see cref="LogID"/> is only able to be handled as a remote request from one logger to another
        /// </summary>
        RemoteAccessOnly = 1,
        /// <summary>
        /// The <see cref="LogID"/> can only be handled through a local log request
        /// </summary>
        Private = 2
    }

    /// <summary>
    /// A context identifier that describes the purpose of a <see cref="LogID"/>
    /// </summary>
    public enum LogIDType
    {
        /// <summary>The context represents a log file</summary>
        File,
        /// <summary>The context represents a log group</summary>
        Group
    }
}
