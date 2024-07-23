using System;
using System.Collections.Generic;
using System.IO;

namespace LogUtils
{
    public class LogRequestHandler : UtilityComponent
    {
        public override string Tag => UtilityConsts.ComponentTags.REQUEST_DATA;

        /// <summary>
        /// A list of loggers available to handle log requests. (Not all loggers may accept remote requests)
        /// </summary>
        public List<BetaLogger> AvailableLoggers = new List<BetaLogger>();

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
