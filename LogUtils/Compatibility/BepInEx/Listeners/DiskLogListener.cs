﻿using BepInEx.Logging;
using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Helpers;
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
                UtilityCore.RequestHandler.SanitizeCurrentRequest();

                LogRequest request = UtilityCore.RequestHandler.CurrentRequest;

                if ((request == null || request.Data.ID != LogID.BepInEx) && eventArgs.Source.SourceName == UtilityConsts.UTILITY_NAME)
                {
                    request = new LogRequest(RequestType.Game, new LogMessageEventArgs(LogID.BepInEx, eventArgs));
                    logUtilityEvent(request);
                    return;
                }

                if (RWInfo.LatestSetupPeriodReached >= SetupPeriod.RWAwake)
                    ThreadUtils.AssertRunningOnMainThread(LogID.BepInEx);

                request = UtilityCore.RequestHandler.CurrentRequest;

                if (request == null)
                {
                    request = UtilityCore.RequestHandler.Submit(new LogRequest(RequestType.Game, new LogMessageEventArgs(LogID.BepInEx, eventArgs)), false);

                    if (request.Status == RequestStatus.Rejected)
                        return;
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

        internal void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            LogWriter.TryDispose(Writer);

            Writer = null;
            IsDisposed = true;
        }

        public void Dispose()
        {
            //Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        ~DiskLogListener()
        {
            Dispose(disposing: false);
        }

        #endregion
    }
}
