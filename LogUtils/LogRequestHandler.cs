using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LogUtils
{
    public class LogRequestHandler : UtilityComponent
    {
        public override string Tag => UtilityConsts.ComponentTags.REQUEST_DATA;

        private List<BetaLogger> availableLoggers = new List<BetaLogger>();

        /// <summary>
        /// A list of loggers available to handle remote log requests
        /// </summary>
        public IEnumerable<BetaLogger> AvailableLoggers => availableLoggers;

        public BufferedLinkedList<LogRequest> UnhandledRequests;

        public LogRequest CurrentRequest
        {
            get
            {
                if (UnhandledRequests.Count == 0) //No requests to handle
                    return null;

                LogRequest request = UnhandledRequests.Last.Value;

                //Only pending requests should be considered current
                if (request.Status == RequestStatus.Pending)
                    return request;

                return null;
            }

            set
            {
                if (value == null)
                    throw new ArgumentNullException("Log Request queue does not accept null values");

                LogRequest lastUnhandledRequest = CurrentRequest;

                //Ensure that only one pending request is handled by design. This shouldn't be the case normally, and handling it this way will consume the unhandled request
                if (lastUnhandledRequest != null && (lastUnhandledRequest.Status == RequestStatus.Complete || lastUnhandledRequest.Status == RequestStatus.Pending))
                    UnhandledRequests.RemoveLast();

                UnhandledRequests.AddLast(value);
            }
        }

        public LogRequestHandler()
        {
            enabled = true;
            UnhandledRequests = new BufferedLinkedList<LogRequest>(20);
        }

        public IEnumerable<LogRequest> GetActiveRequests(LogID logFile)
        {
            return UnhandledRequests.Where(log => log.Equals(logFile));
        }

        /// <summary>
        /// Registers a logger for remote logging
        /// </summary>
        public void Register(BetaLogger logger)
        {
            UtilityCore.BaseLogger.LogInfo("Registering logger");
            UtilityCore.BaseLogger.LogInfo("Log targets: " + string.Join(" ,", logger.LogTargets));

            availableLoggers.Add(logger);

            //Regardless of whether this logger 
            foreach (LogID logFile in logger.LogTargets.Where(log => !log.IsGameControlled && log.Access != LogAccess.RemoteAccessOnly)) //Game controlled logids cannot be handled here
            {
                logger.HandleRequests(GetActiveRequests(logFile));
            }
        }

        /// <summary>
        /// Unregisters a logger for remote logging
        /// </summary>
        public void Unregister(BetaLogger logger)
        {
            UtilityCore.BaseLogger.LogInfo("Unregistering logger");
            UtilityCore.BaseLogger.LogInfo("Log targets: " + string.Join(" ,", logger.LogTargets));
            availableLoggers.Remove(logger);
        }

        public void Update()
        {
            if (UnhandledRequests.Count == 0) return;

            LogRequest target;
            LinkedListNode<LogRequest> targetNode;

            target = UnhandledRequests.First.Value;
            targetNode = UnhandledRequests.First;

            bool processRequests = true;
            while (UnhandledRequests.Count > 0 && processRequests)
            {
                if (target.Status == RequestStatus.Complete)
                {
                    UnhandledRequests.Remove(targetNode);
                }
                else if (target.Status == RequestStatus.Rejected)
                {
                    //TODO: Check rejection reason
                }
                else //Pending
                {
                    processRequests = false;
                }
            }
        }
        }
    }
}
