using BepInEx.Logging;
using LogUtils.Helpers;
using LogUtils.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LogUtils
{
    public static class UtilityCore
    {
        public static Version AssemblyVersion = new Version(0, 8, 5);

        /// <summary>
        /// The assembly responsible for loading core resources for the utility
        /// </summary>
        public static bool IsControllingAssembly { get; private set; }

        public static bool IsInitialized { get; private set; }

        private static bool initializingInProgress;

        /// <summary>
        /// The ILogListener managed by the LogManager plugin. Null when LogManager isn't enabled.
        /// </summary>
        public static ILogListener ManagedLogListener;

        public static ManualLogSource BaseLogger { get; private set; }

        public static PropertyDataController PropertyManager;

        /// <summary>
        /// Handles cross-mod data storage for the utility
        /// </summary>
        public static SharedDataHandler DataHandler;

        /// <summary>
        /// Handles log requests between different loggers
        /// </summary>
        public static LogRequestHandler RequestHandler;

        internal static void Initialize()
        {
            if (IsInitialized || initializingInProgress) return; //Initialize may be called several times during the init process

            initializingInProgress = true;

            Debug.unityLogger.filterLogType = (LogType)Math.Max((int)Debug.unityLogger.filterLogType, 1000); //Allow space for custom LogTypes to be defined

            BaseLogger = BepInEx.Logging.Logger.Sources.FirstOrDefault(l => l.SourceName == "LogUtils") as ManualLogSource
                      ?? BepInEx.Logging.Logger.CreateLogSource("LogUtils");

            LoadComponents();

            LogID.InitializeLogIDs(); //This should be called for every assembly that initializes

            if (IsControllingAssembly)
            {
                //Listen for Unity log requests while the log file is unavailable
                if (RWInfo.LatestSetupPeriodReached < LogID.Unity.Properties.AccessPeriod)
                    Application.logMessageReceivedThreaded += HandleUnityLog;

                AppDomain.CurrentDomain.UnhandledException += (o, e) => RequestHandler.DumpRequestsToFile();
                GameHooks.Initialize();
            }

            initializingInProgress = false;
            IsInitialized = true;
        }

        /// <summary>
        /// Creates, or establishes a reference to an existing instance of necessary utility components
        /// </summary>
        internal static void LoadComponents()
        {
            PropertyManager = ComponentUtils.GetOrCreate<PropertyDataController>(UtilityConsts.ComponentTags.PROPERTY_DATA, out bool wasCreated);

            if (wasCreated)
            {
                IsControllingAssembly = true;
                PropertyManager.ReadFromFile();
            }

            DataHandler = ComponentUtils.GetOrCreate<SharedDataHandler>(UtilityConsts.ComponentTags.SHARED_DATA, out _);
            RequestHandler = ComponentUtils.GetOrCreate<LogRequestHandler>(UtilityConsts.ComponentTags.REQUEST_DATA, out _);
        }

        /// <summary>
        /// Searches for an ILogListener that returns a signal sent through ToString().
        /// This listener belongs to the LogManager plugin.
        /// </summary>
        internal static void FindManagedListener()
        {
            IEnumerator<ILogListener> enumerator = BepInEx.Logging.Logger.Listeners.GetEnumerator();

            ILogListener managedListener = null;
            while (enumerator.MoveNext() && managedListener == null)
            {
                if (enumerator.Current.GetSignal() != null)
                    managedListener = enumerator.Current;
            }
        }

        internal static void HandleLogSignal()
        {
            if (ManagedLogListener != null)
                Logger.ProcessLogSignal(ManagedLogListener.GetSignal());
        }

        private static object _loggingLock = new object();
        private static string lastLoggedException;
        private static string lastLoggedStackTrace;

        internal static void HandleUnityLog(string message, string stackTrace, LogType category)
        {
            //This submission wont be able to be logged until Rain World can initialize
            if (RequestHandler.CurrentRequest == null)
            {
                lock (_loggingLock)
                {
                    if (category == LogType.Error || category == LogType.Exception)
                    {
                        //Handle Unity error logging similarly to how the game would handle it
                        if (message != lastLoggedException && stackTrace != lastLoggedStackTrace)
                        {
                            RequestHandler.Submit(new LogRequest(RequestType.Game, new LogEvents.LogMessageEventArgs(LogID.Exception, message, category)), false);
                            RequestHandler.Submit(new LogRequest(RequestType.Game, new LogEvents.LogMessageEventArgs(LogID.Exception, stackTrace, category)), false);

                            lastLoggedException = message;
                            lastLoggedStackTrace = stackTrace;
                        }
                        return;
                    }

                    RequestHandler.Submit(new LogRequest(RequestType.Game, new LogEvents.LogMessageEventArgs(LogID.Unity, message, category)), false);
                }
            }
        }
    }
}
