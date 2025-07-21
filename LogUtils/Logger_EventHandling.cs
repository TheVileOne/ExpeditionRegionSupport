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
            UtilityEvents.OnRegistrationChanged += registrationChangedHandler;
        }

        /// <summary>
        /// Event is invoked when the logger is registered
        /// </summary>
        protected virtual void OnRegister()
        {
            if (newRequestHandler == null)
            {
                WeakReference<Logger> weakRef = new WeakReference<Logger>(this);

                newRequestHandler = new LogRequestEventHandler((request) =>
                {
                    //Event trigger conditions: only applies to the target instance
                    if (weakRef.TryGetTarget(out Logger captured) && captured == request.Sender)
                    {
                        //Invoke event
                        captured.OnNewRequest(request);
                    }
                });
            }
            LogRequestEvents.OnSubmit += newRequestHandler;
        }

        /// <summary>
        /// Event is invoked when the logger becomes unregistered
        /// </summary>
        protected virtual void OnUnregister()
        {
            LogRequestEvents.OnSubmit -= newRequestHandler;
        }

        /// <summary>
        /// Event is invoked each time a LogRequest is submitted by the current logger
        /// </summary>
        protected virtual void OnNewRequest(LogRequest request)
        {
        }
    }
}
