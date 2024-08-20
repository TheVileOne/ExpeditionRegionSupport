using BepInEx.Logging;
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

        public LoggerRestorePoint(BetaLogger logger)
        {
            AllowLogging = logger.AllowLogging;
            AllowRemoteLogging = logger.AllowRemoteLogging;
            LogTargets = logger.LogTargets.ToArray();
        }
    }
}
