using BepInEx.Logging;
using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Helpers;
using System;

namespace LogUtils.CompatibilityServices
{
    public sealed class BepInExDiskLogListener : ILogListener
    {
        /// <summary>
        /// This writer handles all BepInEx log traffic for Rain World
        /// </summary>
        public LogWriter Writer;

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

            bool isUtilityLogger = eventArgs.Source.SourceName == UtilityConsts.UTILITY_NAME;

            if (!isUtilityLogger) //Utility must be allowed to log without disturbing utility functions
            {
                if (RWInfo.LatestSetupPeriodReached >= SetupPeriod.RWAwake)
                    ThreadUtils.AssertRunningOnMainThread(LogID.BepInEx);

                lock (UtilityCore.RequestHandler.RequestProcessLock)
                {
                    LogRequest request = UtilityCore.RequestHandler.CurrentRequest;

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
        }
    }
}
