using LogUtils.Requests;
using System;

namespace LogUtils.Events
{
    public static class UtilityEvents
    {
        //Logging events
        public static LogRequestDataEventHandler OnMessageReceived;
        public static RegistrationChangedEventHandler OnRegistrationChanged;

        //File operation events
        public static LogMovePendingEventHandler OnMovePending;
        public static LogEventHandler OnMoveAborted;
        public static LogEventHandler OnPathChanged;

        //Setup events
        public static Action OnProcessSwitch;
        public static SetupPeriodEventHandler OnSetupPeriodReached;

        //RainWorld Events

        /// <summary>
        /// Gets invoked every frame from the base Update method of the abstract MainLoopProcess class. This EventHandler will be synced to the framesPerSecond defined by 
        /// that process
        /// </summary>
        public static EventHandler<MainLoopProcess, EventArgs> OnNewUpdateSynced;
    }

    public delegate void EventHandler<TSource, TData>(TSource source, TData data);
    public delegate void LogEventHandler(LogEventArgs e);
    public delegate void LogRequestEventHandler(LogRequest request);
    public delegate void LogRequestDataEventHandler(LogRequestEventArgs e);
    public delegate void LogMovePendingEventHandler(LogMovePendingEventArgs e);
    public delegate void LogStreamEventHandler(LogStreamEventArgs e);
    public delegate void RegistrationChangedEventHandler(ILogHandler logger, RegistrationChangedEventArgs registrationStatus);
    public delegate void SetupPeriodEventHandler(SetupPeriodEventArgs e);
}
