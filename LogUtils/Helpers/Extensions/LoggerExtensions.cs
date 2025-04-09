using LogUtils.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LogUtils.Helpers.Extensions
{
    public static class LoggerExtensions
    {
        /// <summary>
        /// Attempts to write content from the message buffer to file
        /// </summary>
        /// <param name="logFile">The file that contains the message buffer</param>
        /// <param name="respectBufferState">When true no content will be written to file if MessageBuffer.IsBuffering property is set to true</param>
        public static void WriteFromBuffer(this ILogWriter writer, LogID logFile, bool respectBufferState = true)
        {
            writer.WriteFromBuffer(logFile, TimeSpan.Zero, respectBufferState);
        }

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
