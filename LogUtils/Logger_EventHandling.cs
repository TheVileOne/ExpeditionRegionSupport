using LogUtils.Events;
using LogUtils.Requests;
using System;

namespace LogUtils
{
    public partial class Logger
    {
        private LogRequestEventHandler newRequestHandler;
        private RegistrationChangedEventHandler registrationChangedHandler;

        internal void SetEvents()
        {
            WeakReference<Logger> weakRef = new WeakReference<Logger>(this);

            registrationChangedHandler = new RegistrationChangedEventHandler((target, registrationStatus) =>
            {
                //Event trigger conditions: only applies to the target instance
                if (weakRef.TryGetTarget(out Logger captured) && captured == target)
                {
                    //Invoke event
                    if (registrationStatus.Current)
                        captured.OnRegister();
                    else
                        captured.OnUnregister();
                }
            });

            newRequestHandler = new LogRequestEventHandler((request) =>
            {
                //Event trigger conditions: only applies to the target instance
                if (weakRef.TryGetTarget(out Logger captured) && captured == request.Sender)
                {
                    //Invoke event
                    captured.OnNewRequest(request);
                }
            });

            UtilityEvents.OnRegistrationChanged += registrationChangedHandler;
            LogRequestEvents.OnSubmit += newRequestHandler;
        }

        /// <summary>
        /// Event is invoked when the logger is registered
        /// </summary>
        protected virtual void OnRegister()
        {
        }

        /// <summary>
        /// Event is invoked when the logger becomes unregistered
        /// </summary>
        protected virtual void OnUnregister()
        {
        }

        /// <summary>
        /// Event is invoked each time a LogRequest is submitted by the current logger
        /// </summary>
        protected virtual void OnNewRequest(LogRequest request)
        {
            if (request.Type == RequestType.Batch)
            {
                try
                {
                    LogRequest lastRequest = null;
                    foreach (LogRequest currentRequest in Processor.GenerateRequests(request.Data))
                    {
                        //Avoids the possibility of messages being processed by the console more than once
                        currentRequest.InheritHandledConsoleTargets(lastRequest);

                        LogBase(request);
                        lastRequest = currentRequest;
                    }
                }
                finally
                {
                    request.Complete(); //Batch requests are submitted, but not stored by LogRequestHandler. No cleanup required here.
                }
            }
        }
    }
}
