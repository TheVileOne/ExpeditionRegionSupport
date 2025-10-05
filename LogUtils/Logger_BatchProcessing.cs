using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using CreateRequestCallback = LogUtils.Requests.LogRequest.Factory.Callback;

namespace LogUtils
{
    public partial class Logger
    {
        protected LogProcessor Processor;

        /// <summary>
        /// Processes and logs a previously initiated batch of log reqeusts
        /// </summary>
        protected void ProcessBatch(LogRequest batchRequest)
        {
            try
            {
                LogRequest lastRequest = null;
                foreach (LogRequest currentRequest in Processor.GenerateRequests(batchRequest.Data))
                {
                    //Avoids the possibility of messages being processed by the console more than once
                    currentRequest.InheritHandledConsoleTargets(lastRequest);

                    LogBase(currentRequest);
                    lastRequest = currentRequest;
                }
            }
            finally
            {
                batchRequest.Complete(); //Batch requests are submitted, but not stored by LogRequestHandler. No cleanup required here.
            }
        }

        protected readonly struct LogProcessor
        {
            /// <summary>
            /// The handler that is responsible for handling processed log requests
            /// </summary>
            private readonly ILogHandler _logHandler;

            public LogProcessor(ILogHandler handler)
            {
                _logHandler = handler;
            }

            public IEnumerable<LogRequest> GenerateRequests(LogRequestEventArgs requestArgs)
            {
                LogProcessorArgs requestProcessorArgs = requestArgs.FindData<LogProcessorArgs>();

                var createRequest = requestProcessorArgs.RequestFactory;

                //LogIDs are processed first followed by ConsoleIDs. Logging is handled through the yielded result
                LogRequest currentRequest, lastRequest = null;
                foreach (LogID target in requestProcessorArgs.EnabledLogIDs)
                {
                    currentRequest = createRequest.Invoke(target.GetRequestType(_logHandler), target, requestArgs.Category, requestArgs.MessageObject, requestArgs.ShouldFilter);

                    if (currentRequest == null)
                        continue;

                    lastRequest = currentRequest;
                    yield return lastRequest;
                }

                IEnumerable<ConsoleID> enabledTargets = requestProcessorArgs.EnabledConsoleIDs;

                if (lastRequest != null) //Possible to be null if all of the requests were rejected
                {
                    var consoleMessageData = lastRequest.Data.GetConsoleData();

                    //Exclude any ConsoleIDs that were already handled when the LogIDs were processed
                    if (consoleMessageData != null)
                        enabledTargets = enabledTargets.Except(consoleMessageData.Handled);
                }

                foreach (ConsoleID target in enabledTargets)
                {
                    currentRequest = createRequest.Invoke(RequestType.Console, target, requestArgs.Category, requestArgs.MessageObject, requestArgs.ShouldFilter);

                    if (currentRequest == null)
                        continue;

                    lastRequest = currentRequest;
                    yield return lastRequest;
                }
            }
        }

        public class LogProcessorArgs : EventArgs
        {
            public readonly LogTargetCollection Targets;

            internal IEnumerable<LogID> EnabledLogIDs => Targets.LogIDs.Where(t => t.IsEnabled);
            internal IEnumerable<ConsoleID> EnabledConsoleIDs => Targets.ConsoleIDs.Where(t => t.IsEnabled);

            public readonly CreateRequestCallback RequestFactory;

            public LogProcessorArgs(LogTargetCollection targets, CreateRequestCallback requestFactory)
            {
                Targets = targets;
                RequestFactory = requestFactory ?? LogRequest.Factory.Create;
            }
        }
    }
}
