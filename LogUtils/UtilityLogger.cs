using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogUtils
{
    public static class UtilityLogger
    {
        /*
        private static ManualLogSource logger;

        internal static void Initialize()
        {
            logger = BepInEx.Logging.Logger.Sources.FirstOrDefault(l => l.SourceName == "LogUtils") as ManualLogSource
                  ?? BepInEx.Logging.Logger.CreateLogSource("LogUtils");
        }

        public void LogFatal(object data)
        {
            Log(LogLevel.Fatal, data);
        }

        public void LogError(object data)
        {
            Log(LogLevel.Error, data);
        }

        public void LogWarning(object data)
        {
            Log(LogLevel.Warning, data);
        }

        public void LogMessage(object data)
        {
            Log(LogLevel.Message, data);
        }

        public void LogInfo(object data)
        {
            Log(LogLevel.Info, data);
        }

        public void LogDebug(object data)
        {
            Log(LogLevel.Debug, data);
        }
        */
    }
}
