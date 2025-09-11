using BepInEx.Logging;
using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Requests;
using LogUtils.Threading;
using System;
using System.Collections.Generic;
using LogEventArgs = BepInEx.Logging.LogEventArgs;

namespace LogUtils.Compatibility.BepInEx.Listeners
{
    public sealed class DiskLogListener : ILogListener
    {
        /// <summary>
        /// This writer handles all BepInEx log traffic for Rain World
        /// </summary>
        public ILogWriter Writer;

        /// <summary>
        /// Stores LogUtils requests until they are able to be handled
        /// </summary>
        private readonly List<LogRequest> utilityRequestsInProcess = new List<LogRequest>();

        public DiskLogListener(ILogWriter writer)
        {
            Writer = writer;
        }

        public void LogEvent(object sender, LogEventArgs eventArgs)
        {
            if (eventArgs.Source is UnityLogSource) return;

            if (IsDisposed)
            {
                if (Writer == null)
                    Writer = LogWriter.Writer;
                UtilityLogger.DebugLog("LogListener has been disposed");
            }

            using (UtilityCore.RequestHandler.BeginCriticalSection())
            {
                LogRequest request;
                //Utility log events are easy to trigger stack overflows through recursive access. For this reason we handle utility sourced requests
                //differently than other requests.
                if (eventArgs.Source is UtilityLogSource)
                {
                    request = new LogRequest(RequestType.Game, new LogRequestEventArgs(LogID.BepInEx, eventArgs));
                    logUtilityEvent(request);
                    return;
                }

                //No situation should require sanitization here. This is a safeguard protection used for stability purposes.
                UtilityCore.RequestHandler.SanitizeCurrentRequest();

                //BepInEx requests are handled through an external API. We make sure the recursive counter is updated to reflect that we receives a request.
                request = UtilityCore.RequestHandler.GetRequestFromAPI(LogID.BepInEx);

                //if (RWInfo.LatestSetupPeriodReached >= SetupPeriod.RWAwake)
                //    ThreadUtils.AssertRunningOnMainThread(LogID.BepInEx);

                if (request == null)
                {
                    try
                    {
                        UtilityCore.RequestHandler.RecursionCheckCounter++;
                        request = UtilityCore.RequestHandler.Submit(new LogRequest(RequestType.Game, new LogRequestEventArgs(LogID.BepInEx, eventArgs)), false);

                        if (request.Status == RequestStatus.Rejected)
                            return;
                    }
                    finally
                    {
                        UtilityCore.RequestHandler.RecursionCheckCounter--;
                    }
                }

                request.Data.LogSource = eventArgs.Source;
                Writer.WriteFrom(request);
            }
        }

        private void logUtilityEvent(LogRequest request)
        {
            const int retry_request_time = 5; //milliseconds

            if (utilityRequestsInProcess.Count > 0)
            {
                Task utilityRequestTask = new Task(() =>
                {
                    logUtilityEvent(request);
                }, retry_request_time);

                utilityRequestTask.Name = UtilityConsts.UTILITY_NAME;

                LogTasker.Schedule(utilityRequestTask);
                return;
            }

            //TODO: Check thread safety
            utilityRequestsInProcess.Add(request);

            try
            {
                //LogUtils is given logging priority and must not be handled through the request submission process like other log requests. This makes
                //it possible for LogUtils to write to process without interfering with requests in process 
                Writer.WriteFrom(request);
            }
            catch (Exception ex)
            {
                UtilityLogger.DebugLog("Could not handle LogUtils request");
                UtilityLogger.DebugLog(ex);
            }
            finally
            {
                utilityRequestsInProcess.Remove(request);
            }
        }

        #region Dispose pattern

        internal bool IsDisposed;

        /// <summary>
        /// Performs tasks for disposing a <see cref="DiskLogListener"/>
        /// </summary>
        /// <param name="disposing">Whether or not the dispose request is invoked by the application (true), or invoked by the destructor (false)</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Dispose pattern")]
        internal void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            LogWriter.TryDispose(Writer);

            Writer = null;
            IsDisposed = true;
        }

        /// <inheritdoc cref="Dispose(bool)"/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary/>
        ~DiskLogListener()
        {
            Dispose(disposing: false);
        }
        #endregion
    }
}
