using LogUtils.Helpers;
using LogUtils.Helpers.Comparers;
using LogUtils.Helpers.FileHandling;
using LogUtils.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LogUtils.Enums
{
    public class LogID : SharedExtEnum<LogID>, IEquatable<LogID>
    {
        /// <summary>
        /// Contains path information, and other settings that affect logging behavior 
        /// </summary>
        public LogProperties Properties { get; protected set; }

        /// <summary>
        /// Controls the handle limitations of this LogID for the local mod
        /// </summary>
        public LogAccess Access;

        /// <summary>
        /// A flag that controls whether logging should be permitted for this LogID instance
        /// </summary>
        public bool IsEnabled = true;

        /// <summary>
        /// A flag that indicates that this represents a log file managed by the game
        /// </summary>
        public bool IsGameControlled;

        public static LogID[] RegisteredEntries => LogProperties.PropertyManager.Properties.Select(p => p.ID).ToArray();

        /// <summary>
        /// Creates a new LogID instance
        /// </summary>
        /// <param name="filename">The filename, and optional path to target and use for logging
        /// The ExtEnum value will be equivalent to the filename portion of this parameter without the file extension
        /// A filename without a path will default to StreamingAssets directory as a path unless an existing LogID with the specified filename is already registered
        /// </param>
        /// <param name="access">Modifier that affects who may access and use the log file
        /// Set to LogAccess.RemoteAccessOnly UNLESS your mod intends to handle LogRequests for this LogID
        /// </param>
        /// <param name="register">Whether or not this LogID is registered as an ExtEnum
        /// Registration affects whether a LogID gets its own properties that write to file on game close
        /// An unregistered LogID will still get its own properties, those properties, and changes to those properties wont be saved to file
        /// DO NOT register a LogID that is temporary, and your mod is designated for public release
        /// </param>
        public LogID(string filename, LogAccess access, bool register = false) : this(new PathWrapper(filename), access, register)
        {
        }

        /// <summary>
        /// Creates a new LogID instance without attempting to create a LogProperties instance
        /// </summary>
        internal LogID(string filename) : base(Path.GetFileNameWithoutExtension(filename), false) //Used by ComparisonLogID to bypass LogProperties creation
        {
            InitializeFields();
        }

        /// <summary>
        /// Creates a new LogID instance using a filename, and assuming a default/preexisting registered path
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Exists to satisfy Activator parameters for SharedExtEnum")]
        internal LogID(string filename, bool register) : this(filename, null, LogAccess.RemoteAccessOnly)
        {
        }

        internal LogID(PathWrapper pathData, LogAccess access, bool register) : this(pathData.Filename, pathData.Path, access, register)
        {
        }

        internal LogID(LogProperties properties, string filename, string relativePathNoFile, bool register) : base(Path.GetFileNameWithoutExtension(filename), register)
        {
            Access = LogAccess.RemoteAccessOnly;
            InitializeFields();

            Properties = properties;

            if (Properties == null)
            {
                InitializeProperties(relativePathNoFile);

                //This extension will overwrite an existing one with unknown side effects
                string fileExt = Path.GetExtension(filename);

                if (fileExt != string.Empty)
                    Properties.PreferredFileExt = fileExt;
            }
        }

        /// <summary>
        /// Creates a new LogID instance
        /// </summary>
        /// <param name="filename">The filename to target, and use for logging
        /// The ExtEnum value will be equivalent to the filename without the file extension
        /// </param>
        /// <param name="relativePathNoFile">The path to the log file
        /// Setting to null will default to the StreamingAssets directory as a path unless an existing LogID with the specified filename is already registered</param>
        /// <param name="access">Modifier that affects who may access and use the log file
        /// Set to LogAccess.RemoteAccessOnly UNLESS your mod intends to handle LogRequests for this LogID
        /// </param>
        /// <param name="register">Whether or not this LogID is registered as an ExtEnum
        /// Registration affects whether a LogID gets its own properties that write to file on game close
        /// An unregistered LogID will still get its own properties, those properties, and changes to those properties wont be saved to file
        /// DO NOT register a LogID that is temporary, and your mod is designated for public release
        /// </param>
        public LogID(string filename, string relativePathNoFile, LogAccess access, bool register = false) : base(Path.GetFileNameWithoutExtension(filename), register)
        {
            Access = access;

            InitializeFields();
            InitializeProperties(relativePathNoFile);

            //This extension will overwrite an existing one with unknown side effects
            string fileExt = Path.GetExtension(filename);

            if (fileExt != string.Empty)
                Properties.PreferredFileExt = fileExt;
        }

        protected void InitializeFields()
        {
            IsGameControlled = ManagedReference == this ? UtilityConsts.LogNames.NameMatch(value) : ManagedReference.IsGameControlled;

            if (IsGameControlled)
            {
                IsEnabled = true;
                Access = LogAccess.FullAccess;
            }
        }

        protected void InitializeProperties(string logPath)
        {
            Properties = LogProperties.PropertyManager.GetProperties(this, logPath);

            if (Properties == null)
            {
                if (Registered)
                    Properties = LogProperties.PropertyManager.SetProperties(this, logPath); //Register a new LogProperties instance for this LogID
                else
                    Properties = new LogProperties(value, logPath);
            }
        }

        /// <summary>
        /// Determines whether the specified LogID is equal to the current LogID
        /// </summary>
        /// <param name="idOther">The LogID to compare with the current LogID</param>
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
        /// Finds a registered LogID with the given filename, and path
        /// <br>
        /// Compares ID, Filename, and CurrentFilename fields
        /// </br>
        /// </summary>
        /// <param name="filename">The filename to search for</param>
        /// <param name="relativePathNoFile">The filepath to search for. When set to null, any filename match will be returned with custom root being prioritized</param>
        public static LogID Find(string filename, string relativePathNoFile = null)
        {
            IEnumerable<LogID> results = FindAll(filename, CompareOptions.Basic);

            if (!results.Any())
                return null;

            bool searchForAnyPath = LogProperties.IsPathWildcard(relativePathNoFile);
            string searchPath = LogProperties.GetContainingPath(relativePathNoFile);

            LogID bestCandidate = null;
            foreach (LogID logID in results)
            {
                if (logID.Properties.HasFolderPath(searchPath))
                {
                    bestCandidate = logID;
                    break; //Best match has been found
                }

                if (searchForAnyPath && bestCandidate == null) //First match is prioritized over any other match when all paths are valid
                    bestCandidate = logID;
            }
            return bestCandidate;
        }

        /// <summary>
        /// Finds all registered LogID with the given filename
        /// <br>
        /// Compares ID, Filename, and CurrentFilename fields
        /// </br>
        /// </summary>
        /// <param name="filename">The filename to search for</param>
        public static IEnumerable<LogID> FindAll(string filename)
        {
            return FindAll(filename, CompareOptions.Basic);
        }

        /// <summary>
        /// Finds all registered LogID with the given filename
        /// </summary>
        /// <param name="filename">The filename to search for</param>
        /// <param name="compareOptions">Represents options that determine which fields to check against</param>
        public static IEnumerable<LogID> FindAll(string filename, CompareOptions compareOptions)
        {
            return LogProperties.PropertyManager.Properties.Where(p => p.HasFilename(filename, compareOptions)).Select(p => p.ID);
        }

        public static IEnumerable<LogID> FindAll(Func<LogProperties, bool> predicate)
        {
            return LogProperties.PropertyManager.Properties.Where(predicate).Select(p => p.ID);
        }

        /// <summary>
        /// Finds all registered LogIDs with the given tags
        /// </summary>
        public static LogID[] FindByTag(params string[] tags)
        {
            List<LogID> found = new List<LogID>(LogProperties.PropertyManager.Properties.Count);

            foreach (var properties in LogProperties.PropertyManager.Properties)
            {
                if (properties.Tags.Any(tag => tags.Contains(tag, ComparerUtils.StringComparerIgnoreCase)))
                    found.Add(properties.ID);
            }
            return found.ToArray();
        }

        /// <summary>
        /// Checks whether file, and path combination matches the file and path information of an existing registered LogID
        /// </summary>
        /// <param name="filename">The filename to search for</param>
        /// <param name="relativePathNoFile">The filepath to search for. When set to null, any filename match will be returned with custom root being prioritized</param>
        public static bool IsRegistered(string filename, string relativePathNoFile = null)
        {
            return Find(filename, relativePathNoFile) != null;
        }

        internal static void InitializeEnums()
        {
            //Game-defined LogIDs
            BepInEx = new LogID(null, UtilityConsts.LogNames.BepInEx, Paths.BepInExRootPath, true);
            Exception = new LogID(null, UtilityConsts.LogNames.Exception, UtilityConsts.PathKeywords.ROOT, true);
            Expedition = new LogID(null, UtilityConsts.LogNames.Expedition, UtilityConsts.PathKeywords.STREAMING_ASSETS, true);
            JollyCoop = new LogID(null, UtilityConsts.LogNames.JollyCoop, UtilityConsts.PathKeywords.STREAMING_ASSETS, true);
            Unity = new LogID(null, UtilityConsts.LogNames.Unity, UtilityConsts.PathKeywords.ROOT, true);

            //File activity monitoring LogID
            FileActivity = new LogID("LogActivity", UtilityConsts.PathKeywords.ROOT, LogAccess.Private, false);

            //Fallback LogID
            Unknown = new ComparisonLogID(UtilityConsts.LogNames.Unknown);

            BepInEx.Properties.AccessPeriod = SetupPeriod.Pregame;
            BepInEx.Properties.AddTag(nameof(BepInEx));
            BepInEx.Properties.LogSourceName = nameof(BepInEx);
            BepInEx.Properties.AltFilename = UtilityConsts.LogNames.BepInExAlt;
            BepInEx.Properties.IsWriteRestricted = true;
            BepInEx.Properties.LogSessionActive = true; //BepInEx log is active before the utility can initialize
            BepInEx.Properties.PreferredFileExt = FileExt.LOG;

            BepInEx.Properties.Rules.Replace(new BepInExHeaderRule(BepInEx.Properties.ShowCategories.IsEnabled));

            Exception.Properties.AccessPeriod = SetupPeriod.RWAwake;
            Exception.Properties.AddTag(nameof(Exception));
            Exception.Properties.LogSourceName = nameof(Exception);
            Exception.Properties.AltFilename = UtilityConsts.LogNames.ExceptionAlt;
            Exception.Properties.PreferredFileExt = FileExt.TEXT;

            Expedition.Properties.AccessPeriod = SetupPeriod.ModsInit;
            Expedition.Properties.AddTag(nameof(Expedition));
            Expedition.Properties.LogSourceName = nameof(Expedition);
            Expedition.Properties.AltFilename = UtilityConsts.LogNames.ExpeditionAlt;
            Expedition.Properties.PreferredFileExt = FileExt.TEXT;
            Expedition.Properties.ShowLogsAware = true;

            JollyCoop.Properties.AccessPeriod = SetupPeriod.ModsInit;
            JollyCoop.Properties.AddTag(nameof(JollyCoop));
            JollyCoop.Properties.LogSourceName = nameof(JollyCoop);
            JollyCoop.Properties.AltFilename = UtilityConsts.LogNames.JollyCoopAlt;
            JollyCoop.Properties.PreferredFileExt = FileExt.TEXT;
            JollyCoop.Properties.ShowLogsAware = true;

            Unity.Properties.AccessPeriod = SetupPeriod.RWAwake;
            Unity.Properties.AddTag(nameof(Unity));
            Unity.Properties.LogSourceName = nameof(Unity);
            Unity.Properties.AltFilename = UtilityConsts.LogNames.UnityAlt;
            Unity.Properties.PreferredFileExt = FileExt.TEXT;
        }

        static LogID()
        {
            UtilityCore.EnsureInitializedState();
        }

        public static LogID BepInEx;
        public static LogID Exception;
        public static LogID Expedition;
        internal static LogID FileActivity;
        public static LogID JollyCoop;
        public static LogID Unity;
        internal static LogID Unknown;
    }

    public enum LogAccess
    {
        FullAccess = 0, //LogID can be handled by either local, or remote loggers
        RemoteAccessOnly = 1, //LogID cannot be handled by the same mod that makes the log request
        Private = 2 //LogID can only be handled by the mod that registers it
    }
}
