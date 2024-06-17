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
                //Make sure properties are read from file before any ExtEnums are registered
                LogProperties.LoadProperties();

                values.AddEntry(value);
                index = values.Count - 1;
            }

            if (LogProperties.PropertyManager != null)
            {
                Properties = LogProperties.PropertyManager.GetProperties(this, relativePathNoFile);

                if (register && Properties == null)
                {
                    //Register a new LogProperties instance for this LogID
                    Properties = LogProperties.PropertyManager.SetProperties(this, relativePathNoFile);
                }
            }

            //At this point, a null means there isn't an intention to register properties with the manager, but properties should still be created
            //in case LogID is registered in the future
            if (Properties == null)
                Properties = new LogProperties(this, relativePathNoFile);
        }

        //TODO: Need to add altfilename info
        public static readonly LogID BepInEx = new LogID(null, "LogOutput", Paths.BepInExRootPath, true);
        public static readonly LogID Exception = new LogID(null, "exceptionLog", "root", true);
        public static readonly LogID Expedition = new LogID(null, "ExpLog", "customroot", true);
        public static readonly LogID JollyCoop = new LogID(null, "jollyLog", "customroot", true);
        public static readonly LogID Unity = new LogID(null, "consoleLog", "root", true);
    }
}
