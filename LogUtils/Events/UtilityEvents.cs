namespace LogUtils.Events
{
    public static class UtilityEvents
    {
        //Logging events
        public static LogMessageEventHandler OnMessageReceived;

        //File operation events
        public static LogMovePendingEventHandler OnMovePending;
        public static LogEventHandler OnMoveAborted;
        public static LogEventHandler OnPathChanged;

        //Setup events
        public static SetupPeriodEventHandler OnSetupPeriodReached;
    }

    public delegate void LogEventHandler(LogEventArgs e);
    public delegate void LogMessageEventHandler(LogMessageEventArgs e);
    public delegate void LogMovePendingEventHandler(LogMovePendingEventArgs e);
    public delegate void LogStreamEventHandler(LogStreamEventArgs e);
    public delegate void SetupPeriodEventHandler(SetupPeriodEventArgs e);
}
