using BepInEx.Logging;
using LogUtils.Helpers;
using LogUtils.Properties;
using Menu;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
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

        /// <summary>
        /// The initialized state for the assembly. This does NOT indicate that another version of the assembly has initialized,
        /// and every assembly must go through the init process
        /// </summary>
        public static bool IsInitialized { get; private set; }

        /// <summary>
        /// The initialization process is in progress for the current assembly
        /// </summary>
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

            //This is before hooks are established. It is highly likely that the utility will load very early, and any mod could force it. Since we cannot control
            //this factor, we have to infer using specific game fields to tell which part of the initialization period we are in
            SetupPeriod startupPeriod = SetupPeriod.Pregame;

            if (Custom.rainWorld != null)
            {
                if (Menu.Remix.OptionalText.engText == null) //This is set in PreModsInIt
                {
                    startupPeriod = SetupPeriod.RWAwake;
                }
                else if (Custom.rainWorld.processManager?.currentMainLoop is InitializationScreen)
                {
                    //All ExtEnumTypes are forcefully updated as part of the OnModsInit run routine. Look for initialized types
                    if (ExtEnumBase.valueDictionary.Count() < 50) //Somewhere between PreModsInIt and OnModsInit, we don't know where exactly
                        startupPeriod = SetupPeriod.PreMods;
                    else
                        startupPeriod = SetupPeriod.PostMods;
                }
                else //It shouldn't be possible to be another period
                {
                    startupPeriod = SetupPeriod.PostMods;
                }
            }

            RWInfo.LatestSetupPeriodReached = startupPeriod;

            PropertyManager.IsEditGracePeriod = RWInfo.LatestSetupPeriodReached < SetupPeriod.PostMods;

            LogID.InitializeLogIDs(); //This should be called for every assembly that initializes

            if (IsControllingAssembly)
            {
                bool shouldRunStartupRoutine = RWInfo.LatestSetupPeriodReached < RWInfo.STARTUP_CUTOFF_PERIOD;

                //It is important for normal function of the utility for it to initialize before the game does. The following code handles the situation when
                //the utility is initialized too late, and the game has been allowed to intialize the log files without the necessary utility hooks active
                if (RWInfo.LatestSetupPeriodReached > SetupPeriod.Pregame)
                {
                    if (shouldRunStartupRoutine)
                        PropertyManager.StartupRoutineActive = true; //Notify that startup process might be happening early

                    ProcessLateInitializedLogFile(LogID.Unity);
                    ProcessLateInitializedLogFile(LogID.Exception);

                    if (RWInfo.LatestSetupPeriodReached >= SetupPeriod.ModsInit) //Expedition, and JollyCoop
                    {
                        ProcessLateInitializedLogFile(LogID.Expedition);
                        ProcessLateInitializedLogFile(LogID.JollyCoop);
                    }
                }

                if (shouldRunStartupRoutine) //Sanity check in case we are initializing extra late
                    PropertyManager.BeginStartupRoutine();

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

        internal static void ProcessLateInitializedLogFile(LogID logFile)
        {
            LogProperties properties = logFile.Properties;

            //The original filename is stored without its original file extension in the value field of the LogID
            string originalFilePath = Helpers.LogUtils.FindLogPathWithoutFileExtension(properties.OriginalFolderPath, logFile.value);

            if (originalFilePath != null) //This shouldn't be null under typical circumstances
            {
                bool moveFileFromOriginalPath = !PathUtils.PathsAreEqual(originalFilePath, properties.CurrentFilePath);
                bool lastKnownFileOverwritten = PathUtils.PathsAreEqual(originalFilePath, properties.LastKnownFilePath);

                if (moveFileFromOriginalPath)
                {
                    //Set up the temp file for this log file if it isn't too late to do so
                    if (!lastKnownFileOverwritten && PropertyManager.StartupRoutineActive)
                    {
                        properties.CreateTempFile();
                        properties.SkipStartupRoutine = true;
                    }

                    //Move the file, and if it fails, change the path. Either way, log file exists
                    if (Helpers.LogUtils.MoveLog(originalFilePath, properties.CurrentFilePath) == FileStatus.MoveComplete)
                        properties.ChangePath(properties.CurrentFilePath);
                    else
                        properties.ChangePath(originalFilePath);

                    properties.FileExists = true;
                    properties.LogSessionActive = true;
                }

                properties.SkipStartupRoutine |= lastKnownFileOverwritten;
            }
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
                    if (LogCategory.IsUnityErrorCategory(category))
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
