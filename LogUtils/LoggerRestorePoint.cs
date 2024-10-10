using BepInEx.Logging;
using LogUtils.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogUtils
{
    public struct LoggerRestorePoint
    {
        public bool AllowLogging;
        public bool AllowRemoteLogging;
        public LogID[] LogTargets;

        public LoggerRestorePoint(Logger logger)
        {
            AllowLogging = logger.AllowLogging;
            AllowRemoteLogging = logger.AllowRemoteLogging;
            LogTargets = logger.LogTargets.ToArray();
        }
    }
}
