using BepInEx.Logging;
using LogUtils.Properties;
using System;
using System.IO;
using UnityEngine;

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

            private LogLevel? _bepInExCategory;
            private LogType? _unityCategory;
            public LogLevel BepInExCategory
            {
                get => _bepInExCategory ?? Category.BepInExCategory;
                private set => _bepInExCategory = value;
            }

            public LogType UnityCategory
            {
                get => _unityCategory ?? Category.UnityCategory;
                private set => _unityCategory = value;
            }

            /// <summary>
            /// A message about to be handled by a logger
            /// </summary>
            public string Message { get; }

            /// <summary>
            /// Whether this request's message should be filtered after request is successfully handled
            /// </summary>
            public bool ShouldFilter;

            public FilterDuration FilterDuration;

            public LogMessageEventArgs(LogID logID, object messageData, LogCategory category = null) : base(logID)
            {
                Category = category ?? LogCategory.Default;
                Message = messageData?.ToString();
            }

            public LogMessageEventArgs(LogID logID, object messageData, LogLevel category) : base(logID)
            {
                BepInExCategory = category;
                Message = messageData?.ToString();
            }

            public LogMessageEventArgs(LogID logID, object messageData, LogType category) : base(logID)
            {
                UnityCategory = category;
                Message = messageData?.ToString();
            }
        }

        public class LogStreamEventArgs : LogEventArgs
        {
            public StreamWriter Writer { get; }

            public LogStreamEventArgs(LogID logID, StreamWriter writer) : base(logID)
            {
                Writer = writer;
            }
        }

        public delegate void LogEventHandler(LogEventArgs e);
        public delegate void LogMessageEventHandler(LogMessageEventArgs e);
        public delegate void LogStreamEventHandler(LogStreamEventArgs e);
    }
}
