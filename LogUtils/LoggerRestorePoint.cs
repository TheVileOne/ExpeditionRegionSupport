using LogUtils.Enums;

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
