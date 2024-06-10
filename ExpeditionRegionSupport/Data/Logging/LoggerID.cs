using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Data.Logging
{
    public class LoggerID : ExtEnum<LoggerID>
    {
        /// <summary>
        /// The equivalent LoggerID instance to this LoggerID at the master source. It may not be the same 'LoggerID' that the class mod controls
        /// We cannot store any more specifically than this
        /// </summary>
        protected ExtEnumType MasterID;

        public bool IsMaster;

        public LoggerID(string name, bool register) : base(name, register)
        {
            /*
            foreach (var kvp in valueDictionary)
            {
                Plugin.Logger.LogInfo($"Key: {kvp.Key} Value: {kvp.Value}");
            }
            */
        }
    }
}
