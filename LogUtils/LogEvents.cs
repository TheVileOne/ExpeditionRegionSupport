using BepInEx.Logging;
using JollyCoop;
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

            private LogCategory _category;
            private LogLevel? _categoryBepInEx;
            private LogType? _categoryUnity;

            /// <summary>
            /// The log category associated with the message (equivalent to LogType (Unity), and LogLevel (BepInEx)
            /// </summary>
            public LogCategory Category
            {
                get
                {
                    if (_category != null)
                        return _category;

                    if (ID == LogID.BepInEx || _categoryBepInEx != null)
                        return LogCategory.ToCategory(BepInExCategory);

                    if (ID == LogID.Unity || ID == LogID.Exception || _categoryUnity != null)
                        return LogCategory.ToCategory(UnityCategory);

                    return LogCategory.Default;
                }
                private set => _category = value;
            }

            public LogLevel BepInExCategory
            {
                get
                {
                    if (_categoryBepInEx == null)
                        return _category?.BepInExCategory ?? LogCategory.LOG_LEVEL_DEFAULT;
                    return _categoryBepInEx.Value;
                }
                private set => _categoryBepInEx = value;
            }

            public LogType UnityCategory
            {
                get
                {
                    if (_categoryUnity == null)
                        return _category?.UnityCategory ?? LogCategory.LOG_TYPE_DEFAULT;
                    return _categoryUnity.Value;
                }
                private set => _categoryUnity = value;
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
                Message = processMessage(messageData);
            }

            public LogMessageEventArgs(LogID logID, object messageData, LogLevel category) : base(logID)
            {
                BepInExCategory = category;
                Message = processMessage(messageData);
            }

            public LogMessageEventArgs(LogID logID, object messageData, LogType category) : base(logID)
            {
                UnityCategory = category;
                Message = processMessage(messageData);
            }

            private string processMessage(object data)
            {
                return data?.ToString() ?? string.Empty;
            }

            /// <summary>
            /// Constructs a JollyCoop LogElement struct using stored event data
            /// </summary>
            public LogElement GetElement()
            {
                return new LogElement(Message, LogCategory.IsErrorCategory(Category));
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
