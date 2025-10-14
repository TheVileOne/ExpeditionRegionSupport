using BepInEx.Logging;
using LogUtils.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using PatcherLogEvent = (BepInEx.Logging.LogEventArgs EventData, System.DateTime Timestamp);

namespace LogUtils
{
    /// <summary>
    /// Static class responsible for processing log event data stored by VersionLoader, and logging it to file
    /// </summary>
    internal static class PatcherLogEventProcessor
    {
        public static List<PatcherLogEvent> Results = new List<PatcherLogEvent>();

        public static bool HasResults => Results.Count > 0;

        public static void ProcessLogEvents()
        {
            ILogListener eventListener = GetEventListener();

            if (eventListener == null) //Patcher is unavailable
                return;

            var eventCollection = GetEvents(eventListener);

            if (eventCollection != null)
            {
                //Collect the results
                Results.AddRange(eventCollection);
                return;
            }

            //This shouldn't ever happen
            UtilityLogger.LogWarning("Patcher event data could not be accessed through reflection");
        }

        public static void LogResults()
        {
            if (LogID.Patcher == null)
            {
                UtilityLogger.LogWarning("VersionLoader LogID not initialized");
                return;
            }

            Logger logger = new DiscreteLogger(LogID.Patcher);

            using (logger)
            {
                foreach (PatcherLogEvent logEvent in Results)
                {
                    LogLevel category = logEvent.EventData.Level;
                    object messageObj = logEvent.EventData.Data;

                    if (messageObj is Exception)
                        messageObj = "!!!Exception: " + messageObj;

                    //TODO: This needs to be handled through a log rule instead
                    DateTimeFormat format = LogID.Patcher.Properties.DateTimeFormat;

                    messageObj = $"{logEvent.Timestamp.ToString(format.FormatString, format.FormatProvider)} - {messageObj}";
                    logger.Log(category, messageObj);
                }
            }
        }

        /// <summary>
        /// Accesses and returns the event listener containing unhandled patcher log events
        /// </summary>
        /// <returns>Returns a found listener, otherwise null</returns>
        public static ILogListener GetEventListener()
        {
            var listeners = BepInEx.Logging.Logger.Listeners;
            return listeners.FirstOrDefault(IsEventListener);
        }

        /// <summary>
        /// Extracts log event data from event listener object
        /// </summary>
        internal static ICollection<PatcherLogEvent> GetEvents(ILogListener eventListener)
        {
            //This type belongs to the VersionLoader module - we have to get the values out using reflection
            Type eventListenerType = eventListener.GetType();
            FieldInfo logEventsField = eventListenerType.GetField("Cache", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            return logEventsField.GetValue(eventListener) as ICollection<PatcherLogEvent>;
        }

        internal static bool IsEventListener(ILogListener target)
        {
            Type type = target.GetType();
            return type.Namespace.StartsWith("LogUtils") && type.Name == "LogEventCache";
        }
    }
}
