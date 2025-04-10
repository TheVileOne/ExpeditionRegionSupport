using LogUtils.Enums;
using LogUtils.Helpers.Comparers;
using LogUtils.Requests;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LogUtils.Helpers.Extensions
{
    public static class LoggerExtensions
    {
        /// <summary>
        /// Finds a list of all logger instances that accepts log requests for a specified LogID
        /// </summary>
        /// <param name="logFile">LogID to check</param>
        /// <param name="requestType">The request type expected</param>
        /// <param name="doPathCheck">Should the log file's containing folder bear significance when finding a logger match</param>
        public static IEnumerable<ILogHandler> CompatibleWith(this IEnumerable<ILogHandler> handlers, LogID logFile, RequestType requestType, bool doPathCheck)
        {
            return handlers.Where(handler => handler.CanHandle(logFile, requestType, doPathCheck));
        }

        internal static void FindCompatible(this IEnumerable<ILogHandler> handlers, LogID logFile, out ILogHandler localLogger, out ILogHandler remoteLogger)
        {
            localLogger = remoteLogger = null;

            var remoteHandlers = handlers.CompatibleWith(logFile, RequestType.Remote, doPathCheck: true);
            var localHandlers = handlers.CompatibleWith(logFile, RequestType.Local, doPathCheck: true);

            foreach (Logger logger in remoteHandlers)
            {
                //Most situations wont make it past the first assignment
                if (localLogger == null)
                {
                    localLogger = remoteLogger = logger;
                    continue;
                }

                //Choose the first logger match that allows logging
                if (!localLogger.AllowLogging)
                {
                    localLogger = logger;

                    //Align the remote logger reference with the local logger when remote logging is still unavailable
                    if (!remoteLogger.AllowRemoteLogging)
                        remoteLogger = localLogger;
                    continue;
                }

                //The local logger is the perfect match for the remote logger
                if (localLogger.AllowRemoteLogging)
                {
                    remoteLogger = localLogger;
                    break;
                }

                int results = RemoteLoggerComparer.DefaultComparer.Compare(remoteLogger, logger);

                if (results > 0)
                {
                    remoteLogger = logger;

                    if (results == RemoteLoggerComparer.MAX_SCORE)
                        break;
                }
            }

            //Check specifically for a logger instance that handles local requests in the unusual case that no logger instances can handle remote requests
            if (localLogger == null)
            {
                remoteLogger = null;

                foreach (ILogHandler logger in localHandlers)
                {
                    if (localLogger == null || logger.AllowLogging)
                    {
                        localLogger = logger;

                        if (logger.AllowLogging)
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Find a logger instance that accepts log requests for a specified LogID
        /// </summary>
        /// <param name="logFile">LogID to check</param>
        /// <param name="requestType">The request type expected</param>
        /// <param name="doPathCheck">Should the log file's containing folder bear significance when finding a logger match</param>
        internal static ILogHandler FindCompatible(this IEnumerable<ILogHandler> handlers, LogID logFile, RequestType requestType, bool doPathCheck)
        {
            if (requestType == RequestType.Game)
                return null;

            ILogHandler[] candidates = handlers.CompatibleWith(logFile, requestType, doPathCheck).ToArray();

            if (candidates.Length == 0)
                return null;

            if (candidates.Length == 1)
                return candidates[0];

            ILogHandler bestCandidate;
            if (requestType == RequestType.Local)
            {
                bestCandidate = Array.Find(candidates, logger => logger.AllowLogging) ?? candidates[0];
            }
            else
            {
                bestCandidate = candidates[0];

                if (bestCandidate.AllowLogging && bestCandidate.AllowRemoteLogging)
                    return bestCandidate;

                foreach (ILogHandler logger in candidates)
                {
                    int results = RemoteLoggerComparer.DefaultComparer.Compare(bestCandidate, logger);

                    if (results > 0)
                    {
                        bestCandidate = logger;

                        if (results == RemoteLoggerComparer.MAX_SCORE)
                            break;
                    }
                }
            }
            return bestCandidate;
        }

        public static IEnumerable<ILogWriter> GetWriters(this IEnumerable<ILogHandler> handlers)
        {
            foreach (var writeProvider in handlers.OfType<ILogWriterProvider>())
            {
                ILogWriter writer = writeProvider.GetWriter();

                if (writer != null)
                    yield return writer;
            }
            yield break;
        }

        public static IEnumerable<ILogWriter> GetWriters(this IEnumerable<ILogHandler> handlers, LogID logFile)
        {
            foreach (var writeProvider in handlers.OfType<ILogWriterProvider>())
            {
                ILogWriter writer = writeProvider.GetWriter(logFile);

                if (writer != null)
                    yield return writer;
            }
            yield break;
        }

        /// <summary>
        /// Attempts to write content from the message buffer to file
        /// </summary>
        /// <param name="logFile">The file that contains the message buffer</param>
        /// <param name="respectBufferState">When true no content will be written to file if MessageBuffer.IsBuffering property is set to true</param>
        public static void WriteFromBuffer(this ILogWriter writer, LogID logFile, bool respectBufferState = true)
        {
            writer.WriteFromBuffer(logFile, TimeSpan.Zero, respectBufferState);
        }

        internal static IEnumerable<PersistentLogFileHandle> GetUnusedHandles(this ILogHandler logger, IEnumerable<PersistentLogFileHandle> handlePool)
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
