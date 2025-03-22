using LogUtils.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LogUtils.Helpers.Extensions
{
    public static class LoggerExtensions
    {
        internal static IEnumerable<PersistentLogFileHandle> GetUnusedHandles(this ILogger logger, IEnumerable<PersistentLogFileHandle> handlePool)
        {
            var localTargets = Array.FindAll(logger.AvailableTargets, canLogBeHandledLocally);

            return handlePool.Where(handle => !localTargets.Contains(handle.FileID));
        }

        private static bool canLogBeHandledLocally(LogID logID)
        {
            //No game-controlled, or remote targets
            return !logID.IsGameControlled && (logID.Access == LogAccess.FullAccess || logID.Access == LogAccess.Private);
        }
    }
}
