using BepInEx;

namespace ExpeditionRegionSupport.Data.Logging
{
    public class LogID : ExtEnum<LogProperties>
    {
        /// <summary>
        /// Contains path information, and other settings that affect logging behavior 
        /// </summary>
        public LogProperties Properties { get; }

        public LogID(string modID, string name, string relativePathNoFile = null, bool register = false) : base(name, false)
        {
            if (register)
            {
                values.AddEntry(value);
                index = values.Count - 1;
            }

            Properties = LogProperties.PropertyManager.GetProperties(this, relativePathNoFile);

            if (register && Properties == null)
            {
                //Register a new LogProperties instance for this LogID
                Properties = LogProperties.PropertyManager.SetProperties(this, relativePathNoFile);
            }

            //At this point, a null means there isn't an intention to register properties with the manager, but properties should still be created
            //in case LogID is registered in the future
            if (Properties == null)
                Properties = new LogProperties(this, relativePathNoFile);
        }

        static LogID()
        {
            //Initialize the utility when this class is accessed
            if (!UtilityCore.IsInitialized)
                UtilityCore.Initialize();

            //TODO: Need to add altfilename info
            BepInEx = new LogID(null, "LogOutput", Paths.BepInExRootPath, true);
            Exception = new LogID(null, "exceptionLog", "root", true);
            Expedition = new LogID(null, "ExpLog", "customroot", true);
            JollyCoop = new LogID(null, "jollyLog", "customroot", true);
            Unity = new LogID(null, "consoleLog", "root", true);
        }

        public static readonly LogID BepInEx;
        public static readonly LogID Exception;
        public static readonly LogID Expedition;
        public static readonly LogID JollyCoop;
        public static readonly LogID Unity;
    }
}
