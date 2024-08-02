using BepInEx;
using LogUtils.Properties;

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

        public LogID(string filename, string relativePathNoFile = null, bool register = false) : this(filename, relativePathNoFile, LogAccess.RemoteAccessOnly, register)
        {
        }

        public LogID(string filename, LogAccess access, bool register = false) : this(filename, null, access, register)
        {
        }

        internal LogID(string filename, string relativePathNoFile, bool gameControlled, bool register) : this(filename, relativePathNoFile, gameControlled ? LogAccess.FullAccess : LogAccess.RemoteAccessOnly, register)
        {
            IsGameControlled = gameControlled;
        }

        public LogID(string filename, string relativePathNoFile, LogAccess access = LogAccess.RemoteAccessOnly, bool register = false) : base(filename, register)
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
        }

        static LogID()
        {
            //Initialize the utility when this class is accessed
            if (!UtilityCore.IsInitialized)
                UtilityCore.Initialize();

            BepInEx = new LogID("LogOutput", Paths.BepInExRootPath, true, true);
            Exception = new LogID("exceptionLog", "root", true, true);
            Expedition = new LogID("ExpLog", "customroot", true, true);
            JollyCoop = new LogID("jollyLog", "customroot", true, true);
            Unity = new LogID("consoleLog", "root", true, true);

            BepInEx.Properties.AccessPeriod = SetupPeriod.Pregame;
            BepInEx.Properties.AltFilename = "mods";
            BepInEx.Properties.Rules.Add(new ShowCategoryRule(true));

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

        public static readonly LogID BepInEx;
        public static readonly LogID Exception;
        public static readonly LogID Expedition;
        public static readonly LogID JollyCoop;
        public static readonly LogID Unity;
    }

    public enum LogAccess
    {
        FullAccess = 0, //LogID can be handled by either local, or remote loggers
        RemoteAccessOnly = 1, //LogID cannot be handled by the same mod that makes the log request
        Private = 2 //LogID can only be handled by the mod that registers it
    }
}
