﻿namespace LogUtils.Events
{
    public static class UtilityEvents
    {
        private static SharedField<LogMessageEventHandler> onMessageReceived;
        private static SharedField<LogEventHandler> onPathChanged;

        public static LogMessageEventHandler OnMessageReceived
        {
            get
            {
                if (onMessageReceived == null)
                    onMessageReceived = UtilityCore.DataHandler.GetField<LogMessageEventHandler>(nameof(OnMessageReceived));
                return onMessageReceived.Value;
            }
        }

        public static LogMovePendingEventHandler OnMovePending;
        public static LogEventHandler OnMoveAborted;

        public static LogEventHandler OnPathChanged
        {
            get
            {
                if (onPathChanged == null)
                    onPathChanged = UtilityCore.DataHandler.GetField<LogEventHandler>(nameof(OnPathChanged));
                return onPathChanged.Value;
            }
        }

        public static SetupPeriodEventHandler OnSetupPeriodReached;
    }

    public delegate void LogEventHandler(LogEventArgs e);
    public delegate void LogMessageEventHandler(LogMessageEventArgs e);
    public delegate void LogMovePendingEventHandler(LogMovePendingEventArgs e);
    public delegate void LogStreamEventHandler(LogStreamEventArgs e);
    public delegate void SetupPeriodEventHandler(SetupPeriodEventArgs e);
}
