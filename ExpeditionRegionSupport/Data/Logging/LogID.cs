using BepInEx;

namespace ExpeditionRegionSupport.Data.Logging
{
    public class LogID : ExtEnum<LogProperties>
    {
        /// <summary>
        /// Contains path information, and other settings that affect logging behavior 
        /// </summary>
        public LogProperties Properties { get; }

        public LogID(string filename, string relativePathNoFile = null, bool register = false) : base(filename, false)
        {
            if (register)
            {
                values.AddEntry(value);
                index = values.Count - 1;
            }

            Properties = LogProperties.PropertyManager.GetProperties(this, relativePathNoFile);

            if (Properties == null)
            {
                if (register)
                    Properties = LogProperties.PropertyManager.SetProperties(this, relativePathNoFile); //Register a new LogProperties instance for this LogID
                else
                    Properties = new LogProperties(this, relativePathNoFile);
            }
        }

        static LogID()
        {
            //Initialize the utility when this class is accessed
            if (!UtilityCore.IsInitialized)
                UtilityCore.Initialize();

            //TODO: Need to add altfilename info
            BepInEx = new LogID("LogOutput", Paths.BepInExRootPath, true);
            Exception = new LogID("exceptionLog", "root", true);
            Expedition = new LogID("ExpLog", "customroot", true);
            JollyCoop = new LogID("jollyLog", "customroot", true);
            Unity = new LogID("consoleLog", "root", true);
        }

        public static readonly LogID BepInEx;
        public static readonly LogID Exception;
        public static readonly LogID Expedition;
        public static readonly LogID JollyCoop;
        public static readonly LogID Unity;
    }
}
