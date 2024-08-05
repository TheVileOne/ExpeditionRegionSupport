using BepInEx.Logging;
using LogUtils.Properties;
using System;

namespace LogUtils
{
    public static class LogEvents
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

        public static LogEventHandler OnPathChanged
        {
            get
            {
                if (onPathChanged == null)
                    onPathChanged = UtilityCore.DataHandler.GetField<LogEventHandler>(nameof(OnPathChanged));
                return onPathChanged.Value;
            }
        }

        public class LogEventArgs : EventArgs
        {
            public LogID ID => Properties.ID;
            public LogProperties Properties { get; }

            public LogEventArgs(LogID logID) : this(logID.Properties)
            {
            }

            public LogEventArgs(LogProperties properties)
            {
                Properties = properties;
            }
        }

        public class LogMessageEventArgs : LogEventArgs
        {
            /// <summary>
            /// Contains source information needed to log through the BepInEx logger
            /// </summary>
            public ManualLogSource LogSource;

            /// <summary>
            /// The log category associated with the message (equivalent to LogType (Unity), and LogLevel (BepInEx)
            /// </summary>
            public LogCategory Category { get; }

            /// <summary>
            /// A message about to be handled by a logger
            /// </summary>
            public string Message { get; }

            public LogMessageEventArgs(LogID logID, string message, LogCategory category = null) : base(logID)
            {
                Category = category ?? LogCategory.Default;
                Message = message;
            }
        }

        public delegate void LogEventHandler(LogEventArgs e);
        public delegate void LogMessageEventHandler(LogMessageEventArgs e);
    }
}
