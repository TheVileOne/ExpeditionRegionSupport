using LogUtils.Events;
using LogUtils.Requests;
using System;
using System.Threading;

namespace LogUtils
{
    public partial class Logger
    {
        /// <summary>
        /// Contains event data pertaining to Unity context objects (if applicable)
        /// </summary>
        private ThreadLocal<EventArgs> unityDataCache;

        private LogRequestEventHandler newRequestHandler;
        private RegistrationChangedEventHandler registrationChangedHandler;

        protected virtual void ClearEventData()
        {
            if (unityDataCache?.IsValueCreated == true)
                unityDataCache.Value = null;
        }

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

        protected virtual void OnUnregister()
        {
            LogRequestEvents.OnSubmit -= newRequestHandler;
        }

        protected virtual void OnNewRequest(LogRequest request)
        {
            //Unity exclusive data
            if (unityDataCache != null)
            {
                var data = unityDataCache.Value;

                if (data != null)
                    request.Data.ExtraArgs.Add(data);
            }
        }
    }
}
