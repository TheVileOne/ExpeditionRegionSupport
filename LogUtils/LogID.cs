using LogUtils.Helpers;
using LogUtils.Properties;
using System;
using System.IO;

namespace LogUtils
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
        /// Checks whether file, and path combination matches the file and path information of an existing registered LogProperties object
        /// </summary>
        /// <param name="filename">The filename to search for</param>
        /// <param name="relativePathNoFile">The filepath to search for. When set to null, any filename match will be returned with custom root being prioritized</param>
        public static bool IsRegistered(string filename, string relativePathNoFile = null)
        {
            string logName = Path.GetFileNameWithoutExtension(filename);
            string logPath = LogProperties.GetContainingPath(relativePathNoFile);

            bool searchForAnyPath = LogProperties.IsPathWildcard(relativePathNoFile);

            var stringComparer = EqualityComparer.StringComparerIgnoreCase;

            bool results = false;
            foreach (LogProperties properties in LogProperties.PropertyManager.Properties)
            {
                bool namesAreEqual = stringComparer.Equals(logName, properties.Filename)
                                  || stringComparer.Equals(logName, properties.CurrentFilename);

                if (namesAreEqual &&
                   (searchForAnyPath
                  || PathUtils.PathsAreEqual(logPath, properties.OriginalFolderPath)
                  || PathUtils.PathsAreEqual(logPath, properties.CurrentFolderPath)))
                {
                    results = true;
                    break;
                }
            }
            return results;
        }

        internal static void InitializeLogIDs()
        {
            BepInEx = new LogID(null, UtilityConsts.LogNames.BepInEx, Paths.BepInExRootPath, true);
            Exception = new LogID(null, UtilityConsts.LogNames.Exception, UtilityConsts.PathKeywords.ROOT, true);
            Expedition = new LogID(null, UtilityConsts.LogNames.Expedition, UtilityConsts.PathKeywords.STREAMING_ASSETS, true);
            JollyCoop = new LogID(null, UtilityConsts.LogNames.JollyCoop, UtilityConsts.PathKeywords.STREAMING_ASSETS, true);
            Unity = new LogID(null, UtilityConsts.LogNames.Unity, UtilityConsts.PathKeywords.ROOT, true);

            BepInEx.Properties.AccessPeriod = SetupPeriod.Pregame;
            BepInEx.Properties.AltFilename = UtilityConsts.LogNames.BepInExAlt;
            BepInEx.Properties.IsWriteRestricted = true;
            BepInEx.Properties.LogSessionActive = true; //BepInEx log is active before the utility can initialize
            BepInEx.Properties.PreferredFileExt = FileExt.LOG;
            BepInEx.Properties.ShowCategories.IsEnabled = true;

            Exception.Properties.AccessPeriod = SetupPeriod.RWAwake;
            Exception.Properties.AltFilename = UtilityConsts.LogNames.ExceptionAlt;
            Exception.Properties.PreferredFileExt = FileExt.TEXT;

            Expedition.Properties.AccessPeriod = SetupPeriod.ModsInit;
            Expedition.Properties.AltFilename = UtilityConsts.LogNames.ExpeditionAlt;
            Expedition.Properties.PreferredFileExt = FileExt.TEXT;
            Expedition.Properties.ShowLogsAware = true;

            JollyCoop.Properties.AccessPeriod = SetupPeriod.ModsInit;
            JollyCoop.Properties.AltFilename = UtilityConsts.LogNames.JollyCoopAlt;
            JollyCoop.Properties.PreferredFileExt = FileExt.TEXT;
            JollyCoop.Properties.ShowLogsAware = true;

            Unity.Properties.AccessPeriod = SetupPeriod.RWAwake;
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
    }

    public enum LogAccess
    {
        FullAccess = 0, //LogID can be handled by either local, or remote loggers
        RemoteAccessOnly = 1, //LogID cannot be handled by the same mod that makes the log request
        Private = 2 //LogID can only be handled by the mod that registers it
    }
}
