using BepInEx.Logging;
using JollyCoop;
using LogUtils.Enums;
using LogUtils.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LogUtils.Events
{
    public class LogEventArgs : EventArgs, ICloneable
    {
        public LogID ID { get; protected set; }
        public LogProperties Properties => ID.Properties;

        public LogEventArgs(LogID logID) : this(logID.Properties)
        {
        }

        public LogEventArgs(LogProperties properties)
        {
            ID = properties?.ID;
        }

        public virtual LogEventArgs Clone(LogID newID)
        {
            LogEventArgs copy = (LogEventArgs)Clone();

            copy.ID = newID;
            return copy;
        }

        public virtual object Clone()
        {
            return MemberwiseClone();
        }
    }

    public class LogMessageEventArgs : LogEventArgs
    {
        /// <summary>
        /// A field for extra arguments - Use in cases when it is inconvenient to replace existing argument data  
        /// </summary>
        public readonly List<EventArgs> ExtraArgs = new List<EventArgs>();

        /// <summary>
        /// Contains source information needed to log through the BepInEx logger
        /// </summary>
        public ILogSource LogSource;

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

        /// <summary>
        /// A message to be handled by a logger
        /// </summary>
        public string Message { get; }

        private uint? _totalMessageCache;
        
        public uint TotalMessagesLogged => _totalMessageCache ?? Properties.MessagesHandledThisSession;

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
                var consoleRequestData = FindData<ConsoleRequestEventArgs>();

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

        internal LogMessageEventArgs(LogID logID, BepInEx.Logging.LogEventArgs args) : this(logID, args.Data, args.Level)
        {
            LogSource = args.Source;
        }

        private string processMessage(object data)
        {
            return data?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Finds the first match of a given type, or returns null
        /// </summary>
        public TData FindData<TData>() where TData : EventArgs
        {
            return this as TData ?? ExtraArgs.OfType<TData>().FirstOrDefault();
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
}
