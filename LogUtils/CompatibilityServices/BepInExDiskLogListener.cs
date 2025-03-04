using BepInEx.Logging;
using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Helpers;
using LogUtils.Requests;
using LogUtils.Threading;
using System;
using System.Collections.Generic;

namespace LogUtils.CompatibilityServices
{
    public sealed class BepInExDiskLogListener : ILogListener
    {
        /// <summary>
        /// This writer handles all BepInEx log traffic for Rain World
        /// </summary>
        public LogWriter Writer;

        /// <summary>
        /// Stores LogUtils requests until they are able to be handled
        /// </summary>
        private readonly List<LogRequest> utilityRequestsInProcess = new List<LogRequest>();

        public BepInExDiskLogListener(LogWriter writer)
        {
            Writer = writer;
        }

        public void Dispose()
        {
            IDisposable disposable = Writer as IDisposable;

            if (disposable != null)
                disposable.Dispose();
            Writer = null;
        }

        public void LogEvent(object sender, BepInEx.Logging.LogEventArgs eventArgs)
        {
            if (eventArgs.Source is UnityLogSource) return;

            LogRequest request;
            lock (UtilityCore.RequestHandler.RequestProcessLock)
            {
                request = UtilityCore.RequestHandler.CurrentRequest;

                if (request == null || request.Data.ID != LogID.BepInEx)
                {
                    if (eventArgs.Source.SourceName == UtilityConsts.UTILITY_NAME)
                    {
                        request = new LogRequest(RequestType.Game, new LogMessageEventArgs(LogID.BepInEx, eventArgs));
                        logUtilityEvent(request);
                        return;
                    }
                }
            }

            if (RWInfo.LatestSetupPeriodReached >= SetupPeriod.RWAwake)
                ThreadUtils.AssertRunningOnMainThread(LogID.BepInEx);

            lock (UtilityCore.RequestHandler.RequestProcessLock)
            {
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
    }
}
