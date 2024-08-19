using BepInEx;
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
        public LogProperties Properties { get; }

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

        public LogID(string filename, string relativePathNoFile, bool register) : this(filename, relativePathNoFile, LogAccess.RemoteAccessOnly, register)
        {
        }

        public LogID(string filename, LogAccess access = LogAccess.RemoteAccessOnly, bool register = false) : this(filename, null, access, register)
        {
        }

        internal LogID(string filename, string relativePathNoFile, bool gameControlled, bool register) : this(filename, relativePathNoFile, gameControlled ? LogAccess.FullAccess : LogAccess.RemoteAccessOnly, register)
        {
            IsGameControlled = gameControlled;
        }

        public LogID(string filename, string relativePathNoFile, LogAccess access = LogAccess.RemoteAccessOnly, bool register = false) : base(Path.GetFileNameWithoutExtension(filename), register)
        {
            Access = access;
            Properties = LogProperties.PropertyManager.GetProperties(this, relativePathNoFile);

            if (Properties == null)
            {
                if (register)
                    Properties = LogProperties.PropertyManager.SetProperties(this, relativePathNoFile); //Register a new LogProperties instance for this LogID
                else
                    Properties = new LogProperties(value, relativePathNoFile);
            }

            string fileExt = Path.GetExtension(filename);

            if (fileExt != string.Empty)
                Properties.PreferredFileExt = fileExt;
        }

        public static LogID FromPath(string logPath, LogAccess access, bool register)
        {
            string logName = Path.GetFileNameWithoutExtension(logPath);
            logPath = Path.GetDirectoryName(logPath);

            return new LogID(logName, logPath, access, register);
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

            bool results = false;
            foreach (LogProperties properties in LogProperties.PropertyManager.Properties)
            {
                if ((LogProperties.CompareNames(logName, properties.Filename)
                  || LogProperties.CompareNames(logName, properties.CurrentFilename))
                  &&
                    (searchForAnyPath
                  || PathUtils.ComparePaths(logPath, properties.OriginalFolderPath)
                  || PathUtils.ComparePaths(logPath, properties.CurrentFolderPath)))
                {
                    results = true;
                    break;
                }
            }
            return results;
        }

        internal static void InitializeLogIDs()
        {
            BepInEx = new LogID("LogOutput", Paths.BepInExRootPath, true, true);
            Exception = new LogID("exceptionLog", "root", true, true);
            Expedition = new LogID("ExpLog", "customroot", true, true);
            JollyCoop = new LogID("jollyLog", "customroot", true, true);
            Unity = new LogID("consoleLog", "root", true, true);

            BepInEx.Properties.AccessPeriod = SetupPeriod.Pregame;
            BepInEx.Properties.AltFilename = "mods";
            BepInEx.Properties.Rules.Add(new ShowCategoryRule(true));
            BepInEx.Properties.LogSessionActive = true; //BepInEx log is active before the utility can initialize

            Exception.Properties.AccessPeriod = SetupPeriod.RWAwake;
            Exception.Properties.AltFilename = "exception";

            Expedition.Properties.AccessPeriod = SetupPeriod.ModsInit;
            Expedition.Properties.AltFilename = "expedition";
            Expedition.Properties.ShowLogsAware = true;

            JollyCoop.Properties.AccessPeriod = SetupPeriod.ModsInit;
            JollyCoop.Properties.AltFilename = "jolly";
            JollyCoop.Properties.ShowLogsAware = true;

            Unity.Properties.AccessPeriod = SetupPeriod.RWAwake;
            Unity.Properties.AltFilename = "console";
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
