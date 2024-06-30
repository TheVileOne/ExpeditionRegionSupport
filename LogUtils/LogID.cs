using BepInEx;

namespace LogUtils
{
    public class LogID : ExtEnum<LogID>
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
                    Properties = new LogProperties(filename, relativePathNoFile);
            }
        }

        static LogID()
        {
            //Initialize the utility when this class is accessed
            if (!UtilityCore.IsInitialized)
                UtilityCore.Initialize();

            BepInEx = new LogID("LogOutput", Paths.BepInExRootPath, true);
            Exception = new LogID("exceptionLog", "root", true);
            Expedition = new LogID("ExpLog", "customroot", true);
            JollyCoop = new LogID("jollyLog", "customroot", true);
            Unity = new LogID("consoleLog", "root", true);

            BepInEx.Properties.AltFilename = "mods";
            BepInEx.Properties.Rules.Add(new ShowCategoryRule(true));
            Exception.Properties.AltFilename = "exception";
            Expedition.Properties.AltFilename = "expedition";
            JollyCoop.Properties.AltFilename = "jolly";
            Unity.Properties.AltFilename = "console";
        }

        public static readonly LogID BepInEx;
        public static readonly LogID Exception;
        public static readonly LogID Expedition;
        public static readonly LogID JollyCoop;
        public static readonly LogID Unity;
    }
}
