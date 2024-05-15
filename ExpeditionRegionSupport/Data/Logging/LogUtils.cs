using Expedition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Data.Logging.Utils
{
    public static class LogUtils
    {
        /// <summary>
        /// Logs to both mod-specific logger, and ExpLog
        /// </summary>
        /// <param name="entry"></param>
        public static void LogBoth(string entry)
        {
            ExpLog.Log(entry);
            Plugin.Logger.LogInfo(entry);
        }
    }
}
