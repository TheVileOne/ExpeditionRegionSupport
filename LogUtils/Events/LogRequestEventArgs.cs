using BepInEx.Logging;
using JollyCoop;
using LogUtils.Console;
using LogUtils.Enums;
using LogUtils.Properties.Formatting;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LogUtils.Events
{
    public class LogRequestEventArgs : LogEventArgs
    {
        /// <summary>
        /// A field for extra arguments - Use in cases when it is inconvenient to replace existing argument data  
        /// </summary>
        public readonly List<EventArgs> ExtraArgs = new List<EventArgs>();

        private ILogSource _logSource;

        /// <summary>
        /// Contains source information needed to log through the BepInEx logger
        /// </summary>
        public ILogSource LogSource
        {
            get => _logSource ?? Properties.LogSource;
            set => _logSource = value;
        }

        private LogCategory _category;
        private LogLevel? _categoryBepInEx;
        private LogType? _categoryUnity;

        /// <summary>
        /// The log category associated with the message (equivalent to LogType (Unity), and LogLevel (BepInEx))
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

        internal bool IsTargetingConsole;

        /// <summary>
        /// An object targeted for logging
        /// </summary>
        public readonly object MessageObject;

        /// <summary>
        /// A message targeted for logging
        /// </summary>
        public string Message => MessageObject.ToString();

        /// <summary>
        /// The message format rules associated with this message data
        /// </summary>
        public LogRuleCollection Rules
        {
            get
            {
                if (IsTargetingConsole)
                {
                    //Console data does not use the same field, which is reserved for log files only
                    var consoleMessageData = GetConsoleData();
                    return consoleMessageData.Rules;
                }
                return Properties.Rules;
            }
        }

        private uint? _totalMessageCache;

        public uint TotalMessagesLogged
        {
            get
            {
                if (IsTargetingConsole)
                {
                    //Console data does not use the same field, which is reserved for log files only
                    var consoleMessageData = GetConsoleData();
                    return consoleMessageData.TotalMessagesLogged;
                }
                return _totalMessageCache ?? Properties.MessagesHandledThisSession;
            }
        }

        /// <summary>
        ///  Assigns a value to TotalMessagesLogged that will not change when new messages are logged 
        /// </summary>
        public void CacheMessageTotal()
        {
            _totalMessageCache = Properties.MessagesHandledThisSession;
        }

        /// <summary>
        /// An enumerable containing ConsoleIDs that have yet to handle the message data
        /// </summary>
        public IEnumerable<ConsoleID> PendingConsoleIDs
        {
            get
            {
                var consoleRequestData = GetConsoleData();

                if (consoleRequestData != null)
                {
                    List<ConsoleID> sentToConsole = consoleRequestData.Handled;
                    List<ConsoleID> waitingToBeHandled = consoleRequestData.Pending;

                    return waitingToBeHandled.Union(Properties.ConsoleIDs.Except(sentToConsole));
                }

                return Properties.ConsoleIDs;
            }
        }

        /// <summary>
        /// Whether this request's message should be filtered after request is successfully handled
        /// </summary>
        public bool ShouldFilter;

        public FilterDuration FilterDuration;

        #region Constructors
        /// <summary>
        /// Creates a new data storage instance for a LogRequest event
        /// </summary>
        /// <param name="logID">The targeted log file ID</param>
        /// <param name="messageData">The message to log</param>
        /// <param name="category">The category associated with the message</param>
        /// <exception cref="NullReferenceException">The LogID, or its properties field is null</exception>
        public LogRequestEventArgs(LogID logID, object messageData, LogCategory category = null) : base(logID)
        {
            Category = category ?? LogCategory.Default;
            MessageObject = messageData ?? string.Empty;
        }

        /// <inheritdoc cref="LogRequestEventArgs(LogID, object, LogCategory)"/>
        public LogRequestEventArgs(LogID logID, object messageData, LogLevel category) : base(logID)
        {
            BepInExCategory = category;
            MessageObject = messageData ?? string.Empty;
        }

        /// <inheritdoc cref="LogRequestEventArgs(LogID, object, LogCategory)"/>
        public LogRequestEventArgs(LogID logID, object messageData, LogType category) : base(logID)
        {
            UnityCategory = category;
            MessageObject = messageData ?? string.Empty;
        }

        internal LogRequestEventArgs(LogID logID, BepInEx.Logging.LogEventArgs args) : this(logID, args.Data, args.Level)
        {
            LogSource = args.Source;
        }

        /// <summary>
        /// Creates a new data storage instance for a LogRequest event
        /// </summary>
        /// <param name="consoleID">The targeted console ID</param>
        /// <param name="messageData">The message to log</param>
        /// <param name="category">The category associated with the message</param>
        public LogRequestEventArgs(ConsoleID consoleID, object messageData, LogCategory category = null) : this(LogID.NotUsed, messageData, category)
        {
            ExtraArgs.Add(new ConsoleRequestEventArgs(consoleID));
        }

        /// <inheritdoc cref="LogRequestEventArgs(ConsoleID, object, LogCategory)"/>
        public LogRequestEventArgs(ConsoleID consoleID, object messageData, LogLevel category) : this(LogID.NotUsed, messageData, category)
        {
            ExtraArgs.Add(new ConsoleRequestEventArgs(consoleID));
        }

        /// <inheritdoc cref="LogRequestEventArgs(ConsoleID, object, LogCategory)"/>
        public LogRequestEventArgs(ConsoleID consoleID, object messageData, LogType category) : this(LogID.NotUsed, messageData, category)
        {
            ExtraArgs.Add(new ConsoleRequestEventArgs(consoleID));
        }
        #endregion

        /// <summary>
        /// Finds the first match of a given type, or returns null
        /// </summary>
        public TData FindData<TData>() where TData : class
        {
            return this as TData ?? ExtraArgs.OfType<TData>().FirstOrDefault();
        }

        public ConsoleRequestEventArgs GetConsoleData() => FindData<ConsoleRequestEventArgs>();

        public ConsoleLogWriter GetConsoleWriter()
        {
            var consoleMessageData = GetConsoleData();

            return consoleMessageData?.Writer;
        }

        public void SetConsoleWriter(ConsoleLogWriter writer)
        {
            var consoleMessageData = GetConsoleData();

            if (consoleMessageData == null)
            {
                //We only want to create console data when we have a writer to set
                if (writer == null)
                    return;

                ExtraArgs.Add(consoleMessageData = new ConsoleRequestEventArgs(writer.ID));
            }
            consoleMessageData.Writer = writer;
        }

        /// <summary>
        /// Constructs a JollyCoop LogElement struct using stored event data
        /// </summary>
        public LogElement GetElement()
        {
            return new LogElement(Message, LogCategory.IsErrorCategory(Category));
        }
    }
}
