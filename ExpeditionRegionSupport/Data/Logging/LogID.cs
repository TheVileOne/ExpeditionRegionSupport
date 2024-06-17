using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
