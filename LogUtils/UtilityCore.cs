using BepInEx.Logging;
using LogUtils.Compatibility.BepInEx;
using LogUtils.Compatibility.Unity;
using LogUtils.Console;
using LogUtils.Diagnostics.Tools;
using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.IPC;
using LogUtils.Policy;
using LogUtils.Properties;
using LogUtils.Requests;
using LogUtils.Threading;
using LogUtils.Timers;
using Menu;
using System;
using System.Linq;
using System.Reflection;
using Debug = LogUtils.Diagnostics.Debug;

namespace LogUtils
{
    public static class UtilityCore
    {
        public static Assembly Assembly { get; }

        public static Version AssemblyVersion { get; }

        /// <summary>
        /// The active build environment for the assembly
        /// </summary>
        internal static UtilitySetup.Build Build;

        /// <summary>
        /// The assembly responsible for loading core resources for the utility
        /// </summary>
        public static bool IsControllingAssembly => ProcessMonitor.IsConnected;

        /// <summary>
        /// The initialized state for the assembly. This does NOT indicate that another version of the assembly has initialized,
        /// and every assembly must go through the init process
        /// </summary>
        public static bool IsInitialized { get; private set; }

        /// <summary>
        /// The initialized state encountered a problem during initialization
        /// </summary>
        public static bool InitializedWithErrors { get; private set; }

        /// <summary>
        /// The initialization process is in progress for the current assembly
        /// </summary>
        private static bool initializingInProgress;

        /// <inheritdoc cref="UtilityConfig"/>
        public static UtilityConfig Config;

        /// <summary>
        /// Handles cross-mod data storage for the utility
        /// </summary>
        public static SharedDataHandler DataHandler;

        public static PersistenceManager PersistenceManager;

        public static PropertyDataController PropertyManager;

        /// <summary>
        /// Handles log requests between different loggers
        /// </summary>
        public static LogRequestHandler RequestHandler;

        public static EventScheduler Scheduler;

        /// <summary>
        /// Ensures that core functionality is in a proper and useable state by ensuring the initialization procedure has run
        /// </summary>
        public static void EnsureInitializedState()
        {
            //This approach does not guarantee an initialized state when called during the initialization process itself
            Initialize();
        }

        static UtilityCore()
        {
            Assembly = Assembly.GetExecutingAssembly();
            AssemblyVersion = Assembly.GetName().Version;

            Build =
#if DEBUG
                UtilitySetup.Build.DEVELOPMENT;
#else
                UtilitySetup.Build.RELEASE;
#endif
        }

        internal static void Initialize()
        {
            if (IsInitialized || initializingInProgress) return; //Initialize may be called several times during the init process

            initializingInProgress = true;

            UtilitySetup.CurrentStep = UtilitySetup.InitializationStep.NOT_STARTED;

            try
            {
                UtilitySetup.CurrentStep = UtilitySetup.InitializationStep.SETUP_ENVIRONMENT;
                while (UtilitySetup.CurrentStep != UtilitySetup.InitializationStep.COMPLETE)
                {
                    UtilitySetup.CurrentStep = ApplyStep(UtilitySetup.CurrentStep);
                }
            }
            catch (Exception ex)
            {
                InitializedWithErrors = true;

                //TODO: An exception during utility initialization is most likely unrecoverable. Utility must try to restore original logging functionality here
                UtilityLogger.LogFatal("A fatal error has occurred during setup process. Utility will no longer function as expected");
                UtilityLogger.LogFatal($"FAILED STEP: {UtilitySetup.CurrentStep}");
                UtilityLogger.LogFatal(ex);
            }

            if (IsControllingAssembly && Build == UtilitySetup.Build.DEVELOPMENT)
            {
                Debug.InitializeTestSuite();
                Debug.RunTests();
            }

            //Patcher log processing cleanup
            ILogListener eventListener = PatcherLogEventProcessor.GetEventListener();

            if (eventListener != null)
            {
                PatcherLogEventProcessor.Results.Clear(); //Results have no purpose beyond this point - clear to free up memory
                eventListener.Dispose(); //This event listener removes itself from the Listeners collection when disposed
            }

            initializingInProgress = false;
            IsInitialized = true;

            LogGroupID myGroup = new LogGroupID("Slugg log group", true);
            LogID myLogIDConflict = new LogID("Slugg log group", LogAccess.FullAccess, true);
            LogID myOtherLogIDConflict = new LogID("Slugg log group", "root", LogAccess.FullAccess, true);

            myGroup.Properties.FolderPath = "Test";

            LogCategory category = new LogCategory("LogPath", true);
            UtilityLogger.LogWarning("Category: " + category.Value);
            Logger logger = new Logger(LogID.BepInEx);
            //UtilityLogger.DebugLog("Path 1 " + myLogIDConflict.Properties.OriginalFolderPath);
            //UtilityLogger.DebugLog("Path 2 " + myOtherLogIDConflict.Properties.OriginalFolderPath);
            logger.Log(category, "Path 1 " + myLogIDConflict.Properties.OriginalFolderPath ?? "NULL");
            logger.Log(category, "Path 2 " + myOtherLogIDConflict.Properties.OriginalFolderPath ?? "NULL");
        }

        internal static UtilitySetup.InitializationStep ApplyStep(UtilitySetup.InitializationStep currentStep)
        {
            UtilitySetup.InitializationStep nextStep = currentStep;

            switch (currentStep)
            {
                case UtilitySetup.InitializationStep.SETUP_ENVIRONMENT:
                    {
                        //Utility logger cannot be used before it is initialized, and debug log cannot be used before config is read
                        UnityLogger.EnsureLogTypeCapacity(UtilityConsts.CUSTOM_LOGTYPE_LIMIT);
                        UtilityLogger.Initialize();

                        UtilityConfig.Initialize();
                        AnnounceBuild();

                        if (Build == UtilitySetup.Build.DEVELOPMENT)
                            DeadlockTester.Run();

                        nextStep = UtilitySetup.InitializationStep.START_SCHEDULER;
                        break;
                    }
                case UtilitySetup.InitializationStep.START_SCHEDULER:
                    {
                        LogTasker.Start();

                        nextStep = UtilitySetup.InitializationStep.ESTABLISH_MONITOR_CONNECTION;
                        break;
                    }
                case UtilitySetup.InitializationStep.ESTABLISH_MONITOR_CONNECTION:
                    {
                        UtilityEvents.OnProcessSwitch += OnProcessSwitch;
                        ProcessMonitor.Connect();

                        nextStep = UtilitySetup.InitializationStep.ESTABLISH_SETUP_PERIOD;
                        break;
                    }
                case UtilitySetup.InitializationStep.ESTABLISH_SETUP_PERIOD:
                    {
                        InitializeSetupPeriod();

                        nextStep = UtilitySetup.InitializationStep.INITIALIZE_COMPONENTS;
                        break;
                    }
                case UtilitySetup.InitializationStep.INITIALIZE_COMPONENTS:
                    {
                        ProcessMonitor.WaitOnConnectionStatus();
                        UtilityLogger.Log("IsControllingAssembly: " + IsControllingAssembly);

                        LoadComponents();

                        nextStep = UtilitySetup.InitializationStep.INITIALIZE_PATCHER;
                        break;
                    }
                case UtilitySetup.InitializationStep.INITIALIZE_PATCHER:
                    {
                        PatcherLogEventProcessor.ProcessLogEvents();
                        PatcherController.Initialize();

                        nextStep = UtilitySetup.InitializationStep.INITIALIZE_ENUMS;
                        break;
                    }
                case UtilitySetup.InitializationStep.INITIALIZE_ENUMS:
                    {
                        LogsFolder.Initialize();

                        //These are initialized after components, because they internally depend on SharedDataHandler
                        ConsoleID.InitializeEnums();
                        LogCategory.InitializeEnums();
                        LogID.InitializeEnums();

                        //These are regular ExtEnums, and do not depend on any component
                        //TODO: BufferContext.InitializeEnums();
                        DebugContext.InitializeEnums();

                        nextStep = UtilitySetup.InitializationStep.PARSE_FILTER_RULES;
                        break;
                    }
                case UtilitySetup.InitializationStep.PARSE_FILTER_RULES:
                    {
                        LogFilterParser.ParseFile();

                        if (RainWorldInfo.LatestSetupPeriodReached < SetupPeriod.PostMods)
                            LogFilter.ActivateKeyword(UtilityConsts.FilterKeywords.ACTIVATION_PERIOD_STARTUP);

                        nextStep = UtilitySetup.InitializationStep.ADAPT_LOGGING_SYSTEM;
                        break;
                    }
                case UtilitySetup.InitializationStep.ADAPT_LOGGING_SYSTEM:
                    {
                        //This must be run before late initialized log files are handled to allow BepInEx log file to be moved
                        BepInExAdapter.Run();
                        UnityAdapter.Run();
                        LogConsole.Initialize();

                        if (!IsControllingAssembly)
                        {
                            //Disable console states activated from other Rain World processes
                            LogConsole.SetEnabledState(false);
                        }

                        nextStep = UtilitySetup.InitializationStep.POST_LOGID_PROCESSING;
                        break;
                    }
                case UtilitySetup.InitializationStep.POST_LOGID_PROCESSING:
                    {
                        PropertyManager.ProcessLogFiles();

                        if (PatcherPolicy.ShowPatcherLog)
                            PatcherLogEventProcessor.LogResults();

                        //Listen for Unity log requests while the log file is unavailable
                        if (!LogID.Unity.Properties.CanBeAccessed)
                            UnityLogger.ReceiveUnityLogEvents = true;

                        nextStep = UtilitySetup.InitializationStep.APPLY_HOOKS;
                        break;
                    }
                case UtilitySetup.InitializationStep.APPLY_HOOKS:
                    {
                        GameHooks.Initialize();
                        break;
                    }
            }

            if (nextStep != currentStep)
                return nextStep;

            return nextStep = UtilitySetup.InitializationStep.COMPLETE;
        }

        internal static void InitializeSetupPeriod()
        {
            //This is before hooks are established. It is highly likely that the utility will load very early, and any mod could force it. Since we cannot control
            //this factor, we have to infer using specific game fields to tell which part of the initialization period we are in
            SetupPeriod startupPeriod = SetupPeriod.Pregame;

            if (RainWorldInfo.IsRainWorldRunning)
            {
                if (Menu.Remix.OptionalText.engText == null) //This is set in PreModsInIt
                {
                    startupPeriod = SetupPeriod.RWAwake;
                }
                else if (RainWorldInfo.RainWorld.processManager?.currentMainLoop is InitializationScreen)
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

            UtilityEvents.OnSetupPeriodReached += onSetupPeriodReached;

            //We should invoke at least one setup period event. Use a special case enum value to reflect this special situation.
            if (startupPeriod == SetupPeriod.PostMods)
                RainWorldInfo.NotifyOnPeriodReached(SetupPeriod.LatePostMods);

            RainWorldInfo.LatestSetupPeriodReached = startupPeriod;
        }

        /// <summary>
        /// Creates, or establishes a reference to an existing instance of necessary utility components
        /// </summary>
        internal static void LoadComponents()
        {
            Scheduler = UtilityComponent.Create<EventScheduler>();
            PersistenceManager = UtilityComponent.Create<PersistenceManager>();
            DataHandler = UtilityComponent.Create<SharedDataHandler>();
            RequestHandler = UtilityComponent.Create<LogRequestHandler>();

            PropertyManager = UtilityComponent.Create<PropertyDataController>();
            PropertyManager.SetPropertiesFromFile();
        }

        private static void onSetupPeriodReached(SetupPeriodEventArgs e)
        {
            if (e.CurrentPeriod > e.LastPeriod) //Init methods may be called more than once
            {
                RainWorldInfo.LatestSetupPeriodReached = getPeriod(e.CurrentPeriod);

                if (RainWorldInfo.LatestSetupPeriodReached == SetupPeriod.PostMods)
                    LogsFolder.AddGroupsToFolder();

                if (PropertyManager.StartupRoutineActive)
                {
                    //When the game starts, we need to clean up old log files. Any mod that wishes to access these files must do so in
                    //their plugin's OnEnable, or Awake method
                    PropertyManager.CompleteStartupRoutine();
                }
                else
                {
                    //In every other situation the period changes, we process requests that may have gone unhandled since the last setup period
                    RequestHandler.ProcessRequests();
                }
            }

            static SetupPeriod getPeriod(SetupPeriod currentPeriod)
            {
                if (currentPeriod <= SetupPeriod.PostMods)
                    return currentPeriod;

                if (currentPeriod == SetupPeriod.LatePostMods)
                {
                    //LogUtils operates best when it is initialized early - this is too late to recover all log files
                    UtilityLogger.LogWarning("Extra late initialization");
                    return SetupPeriod.PostMods;
                }

                //Custom setup periods should be okay if a mod wishes to define additional setup states
                UtilityLogger.Log("Unrecognized setup period");
                return currentPeriod;
            }
        }

        internal static void AnnounceBuild()
        {
            UtilityLogger.Logger.LogMessage($"{UtilityConsts.UTILITY_NAME} {AssemblyVersion} {Build} BUILD started");
        }

        internal static void OnProcessConnected()
        {
            //A process switch occurs when a process gives up control to another process - this flag defines when it it considered too early to detect a process switch
            bool isProcessSwitch = UtilitySetup.CurrentStep > UtilitySetup.InitializationStep.INITIALIZE_COMPONENTS;

            if (!isProcessSwitch)
            {
                UtilityLogger.DeleteInternalLogs();
                return;
            }
            UtilityEvents.OnProcessSwitch.Invoke();
        }

        internal static void OnProcessSwitch()
        {
            if (LogConsole.HasCompatibleWriter())
                LogConsole.SetEnabledState(true);

            Config.ReloadFromProcessSwitch();
            PropertyManager.ReloadFromProcessSwitch();

            //Refresh PropertyFile stream - It currently has read only permissions
            PropertyManager.PropertyFile.RefreshStream();
        }

        internal static void OnShutdown()
        {
            LogProperties.PropertyManager.SaveToFile();

            Config.TrySave();
            UtilityLogger.Log("Disabling log files");

            //End all active log sessions
            foreach (LogProperties properties in LogProperties.PropertyManager.AllProperties)
            {
                if (properties is LogGroupProperties groupProperties)
                {
                    foreach (LogID member in groupProperties.Members.Where(logID => !logID.Registered))
                    {
                        using (member.Properties.FileLock.Acquire())
                        {
                            member.Properties.EndLogSession();
                            member.Properties.AllowLogging = false; //No new logs should happen beyond this point
                        }
                    }
                    properties.AllowLogging = false;
                    continue;
                }

                if (properties.ID.Equals(LogID.BepInEx))
                    continue;

                using (properties.FileLock.Acquire())
                {
                    properties.EndLogSession();
                    properties.AllowLogging = false; //No new logs should happen beyond this point
                }
            }

            LogID logFile = LogID.BepInEx;

            if (logFile.Properties.FileExists)
            {
                using (logFile.Properties.FileLock.Acquire())
                {
                    //End the log session later than the others to ensure that session state is reported to file
                    logFile.Properties.EndLogSession();
                    logFile.Properties.AllowLogging = false;

                    if (logFile.Properties.ShouldOverwrite)
                    {
                        //BepInEx log file requires special treatment. This log file cannot be replaced on game start like the other log files
                        //To account for this, replace this log file when the game closes
                        logFile.Properties.CreateTempFile(true);
                    }
                }
            }

            //Stop listening for log events
            var disposeTask = System.Threading.Tasks.Task.Run(BepInExAdapter.DisposeListeners);

            //Disposing the listeners sometimes locks up the main thread for some reason. Running on a background thread avoids potential lock ups
            disposeTask.Wait();

            LogTasker.Close();
        }
    }
}
