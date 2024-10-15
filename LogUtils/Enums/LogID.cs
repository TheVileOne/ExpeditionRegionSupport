using LogUtils.Helpers;
using LogUtils.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LogUtils.Enums
{
    public class LogID : SharedExtEnum<LogID>
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

        public static IEnumerable<LogID> RegisteredIDs => LogProperties.PropertyManager.Properties.Select(p => p.ID);

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

        internal LogID(string filename) : base(Path.GetFileNameWithoutExtension(filename), false) //Used by ComparisonLogID to bypass LogProperties creation
        {
            IsGameControlled = UtilityConsts.LogNames.NameMatch(filename);
        }

        internal LogID(PathWrapper pathData, LogAccess access, bool register) : this(pathData.Filename, pathData.Path, access, register)
        {
        }

        internal LogID(LogProperties properties, string filename, string relativePathNoFile, bool register) : base(Path.GetFileNameWithoutExtension(filename), register)
        {
            //TODO: Check if ManagedReference can be useful for LogIDs
            IsGameControlled = UtilityConsts.LogNames.NameMatch(filename);
            Access = LogAccess.RemoteAccessOnly;

            if (IsGameControlled)
            {
                IsEnabled = true;
                Access = LogAccess.FullAccess;
            }

            Properties = properties;

            if (Properties == null)
            {
                InitializeProperties(relativePathNoFile);

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
            IsGameControlled = UtilityConsts.LogNames.NameMatch(filename);
            Access = access;

            if (IsGameControlled)
            {
                IsEnabled = true;
                Access = LogAccess.FullAccess;
            }

            InitializeProperties(relativePathNoFile);

            string fileExt = Path.GetExtension(filename);

            if (fileExt != string.Empty)
                Properties.PreferredFileExt = fileExt;
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

        public static LogID CreateTemporaryID(string filename, string relativePathNoFile)
        {
            if (IsRegistered(filename, relativePathNoFile))
                throw new InvalidOperationException("Temporary log ID could not be created; a registered log ID already exists.");

            return new LogID(filename, relativePathNoFile, LogAccess.Private);
        }

        /// <summary>
        /// Finds a registered LogID with the given filename, and path
        /// </summary>
        /// <param name="filename">The filename to search for</param>
        /// <param name="relativePathNoFile">The filepath to search for. When set to null, any filename match will be returned with custom root being prioritized</param>
        public static LogID Find(string filename, string relativePathNoFile)
        {
            return FindAll(filename, relativePathNoFile).FirstOrDefault();
        }

        /// <summary>
        /// Finds all registered LogID with the given filename, and path
        /// </summary>
        /// <param name="filename">The filename to search for</param>
        /// <param name="relativePathNoFile">The filepath to search for. When set to null, any filename match will be returned with custom root being prioritized</param>
        public static IEnumerable<LogID> FindAll(string filename, string relativePathNoFile)
        {
            //Convert string data into something that can be compared to stored file data
            string logName = Path.GetFileNameWithoutExtension(filename);
            string logPath = LogProperties.GetContainingPath(relativePathNoFile);

            bool searchForAnyPath = LogProperties.IsPathWildcard(relativePathNoFile);

            foreach (LogProperties properties in LogProperties.PropertyManager.Properties)
            {
                bool filenameMatch = logName.MatchAny(EqualityComparer.StringComparerIgnoreCase, properties.Filename, properties.CurrentFilename);

                if (filenameMatch && (searchForAnyPath || hasPathMatch()))
                    yield return properties.ID;

                bool hasPathMatch()
                {
                    return PathUtils.PathsAreEqual(logPath, properties.OriginalFolderPath)
                        || PathUtils.PathsAreEqual(logPath, properties.CurrentFolderPath);
                }
            }
        }

        /// <summary>
        /// Finds all registered LogIDs with the given filename, and path
        /// </summary>
        public static LogID[] FindByTag(params string[] tags)
        {
            List<LogID> found = new List<LogID>(LogProperties.PropertyManager.Properties.Count);

            foreach (var properties in LogProperties.PropertyManager.Properties)
            {
                if (properties.Tags.Any(tag => tags.Contains(tag, EqualityComparer.StringComparerIgnoreCase)))
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

        internal static void InitializeLogIDs()
        {
            BepInEx = new LogID(null, UtilityConsts.LogNames.BepInEx, Paths.BepInExRootPath, true);
            Exception = new LogID(null, UtilityConsts.LogNames.Exception, UtilityConsts.PathKeywords.ROOT, true);
            Expedition = new LogID(null, UtilityConsts.LogNames.Expedition, UtilityConsts.PathKeywords.STREAMING_ASSETS, true);
            JollyCoop = new LogID(null, UtilityConsts.LogNames.JollyCoop, UtilityConsts.PathKeywords.STREAMING_ASSETS, true);
            Unity = new LogID(null, UtilityConsts.LogNames.Unity, UtilityConsts.PathKeywords.ROOT, true);

            //Fallback LogID
            Unknown = new ComparisonLogID(UtilityConsts.LogNames.Unknown);

            BepInEx.Properties.AccessPeriod = SetupPeriod.Pregame;
            BepInEx.Properties.AddTag(nameof(BepInEx));
            BepInEx.Properties.LogSourceName = nameof(BepInEx);
            BepInEx.Properties.AltFilename = UtilityConsts.LogNames.BepInExAlt;
            BepInEx.Properties.IsWriteRestricted = true;
            BepInEx.Properties.LogSessionActive = true; //BepInEx log is active before the utility can initialize
            BepInEx.Properties.PreferredFileExt = FileExt.LOG;
            BepInEx.Properties.ShowCategories.IsEnabled = true;

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
            //Initialize the utility when this class is accessed
            if (!UtilityCore.IsInitialized)
                UtilityCore.Initialize();
        }

        public static LogID BepInEx;
        public static LogID Exception;
        public static LogID Expedition;
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
