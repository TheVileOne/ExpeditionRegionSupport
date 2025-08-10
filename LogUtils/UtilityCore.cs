using LogUtils.Compatibility.BepInEx;
using LogUtils.Compatibility.Unity;
using LogUtils.Console;
using LogUtils.Diagnostics.Tools;
using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Helpers.FileHandling;
using LogUtils.IPC;
using LogUtils.Policy;
using LogUtils.Properties;
using LogUtils.Requests;
using LogUtils.Threading;
using LogUtils.Timers;
using Menu;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInExPath = LogUtils.Helpers.Paths.BepInEx;
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

            initializingInProgress = false;
            IsInitialized = true;
        }

        internal static UtilitySetup.InitializationStep ApplyStep(UtilitySetup.InitializationStep currentStep)
        {
            UtilitySetup.InitializationStep nextStep = currentStep;

            switch (currentStep)
            {
                case UtilitySetup.InitializationStep.SETUP_ENVIRONMENT:
                    {
                        UnityLogger.EnsureLogTypeCapacity(UtilityConsts.CUSTOM_LOGTYPE_LIMIT);
                        UtilityLogger.Initialize();

                        UtilityConfig.Initialize();
                        AnnounceBuild();

                        if (PatcherPolicy.ShouldDeploy)
                            DeployPatcher();
                        else
                            RemovePatcher();

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

                        nextStep = UtilitySetup.InitializationStep.PARSE_FILTER_RULES;
                        break;
                    }
                case UtilitySetup.InitializationStep.PARSE_FILTER_RULES:
                    {
                        LogFilterParser.ParseFile();

                        if (RWInfo.LatestSetupPeriodReached < SetupPeriod.PostMods)
                            LogFilter.ActivateKeyword(UtilityConsts.FilterKeywords.ACTIVATION_PERIOD_STARTUP);

                        nextStep = UtilitySetup.InitializationStep.ADAPT_LOGGING_SYSTEM;
                        break;
                    }
                case UtilitySetup.InitializationStep.ADAPT_LOGGING_SYSTEM:
                    {
                        //This must be run before late initialized log files are handled to allow BepInEx log file to be moved
                        BepInExAdapter.Run();
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

            if (RWInfo.IsRainWorldRunning)
            {
                if (Menu.Remix.OptionalText.engText == null) //This is set in PreModsInIt
                {
                    startupPeriod = SetupPeriod.RWAwake;
                }
                else if (RWInfo.RainWorld.processManager?.currentMainLoop is InitializationScreen)
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
            RWInfo.LatestSetupPeriodReached = startupPeriod;
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
            if (e.CurrentPeriod > e.LastPeriod)
            {
                RWInfo.LatestSetupPeriodReached = e.CurrentPeriod;

                if (RWInfo.LatestSetupPeriodReached == SetupPeriod.RWAwake)
                {
                    //When the game starts, we need to clean up old log files. Any mod that wishes to access these files
                    //must do so in their plugin's OnEnable, or Awake method
                    PropertyManager.CompleteStartupRoutine();
                }
                else
                {
                    //In every other situation the period changes, we process requests that may have gone unhandled since the last setup period
                    RequestHandler.ProcessRequests();
                }
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

        internal static void DeployPatcher()
        {
            UtilityLogger.Log("Checking patcher status");

            byte[] byteStream = (byte[])Resources.ResourceManager.GetObject(UtilityConsts.ResourceNames.PATCHER);

            Version resourceVersion = Assembly.ReflectionOnlyLoad(byteStream).GetName().Version;
            UtilityLogger.Log("Patcher resource version: " + resourceVersion);

            string patcherPath = Path.Combine(BepInExPath.PatcherPath, "LogUtils.VersionLoader.dll");
            string patcherBackupPath = Path.Combine(BepInExPath.BackupPath, "LogUtils.VersionLoader.dll");

            if (File.Exists(patcherPath)) //Already deployed
            {
                Version activeVersion = AssemblyName.GetAssemblyName(patcherPath).Version;

                if (activeVersion == resourceVersion)
                {
                    UtilityLogger.Log("Patcher found");
                }
                else
                {
                    UtilityLogger.Log($"Current patcher version doesn't match resource - (Current {activeVersion})");
                    if (activeVersion < resourceVersion)
                    {
                        //Patcher will be replaced the next time Rain World starts
                        UtilityLogger.Log($"Replacing patcher with new version");
                        RemovePatcher();
                    }
                }
                return;
            }

            UtilityLogger.Log("Deploying patcher");
            try
            {
                FileUtils.SafeDelete(patcherBackupPath); //Patcher should never exist in both patchers, and backup directories at the same time

                File.WriteAllBytes(patcherPath, byteStream);
                UnityDoorstop.AddToWhitelist("LogUtils.VersionLoader.dll");
            }
            catch (FileNotFoundException)
            {
                UtilityLogger.LogWarning("whitelist.txt is unavailable");
            }
            catch (IOException ex)
            {
                UtilityLogger.LogError("Unable to deploy patcher", ex);
            }
        }

        internal static void RemovePatcher()
        {
            UtilityLogger.Log("Checking patcher status");

            string patcherPath = Path.Combine(BepInExPath.PatcherPath, "LogUtils.VersionLoader.dll");

            if (!File.Exists(patcherPath)) //Patcher not available
            {
                UtilityLogger.Log("Patcher not found");
                return;
            }

            UtilityLogger.Log("Removing patcher");
            try
            {
                UnityDoorstop.RemoveFromWhitelist("LogUtils.VersionLoader.dll");
            }
            catch (FileNotFoundException)
            {
                UtilityLogger.LogWarning("whitelist.txt is unavailable");
            }
            catch (IOException ex)
            {
                UtilityLogger.LogError("Unable to remove patcher", ex);
            }
        }

        internal static void OnProcessSwitch()
        {
            if (LogConsole.HasCompatibleWriter())
                LogConsole.SetEnabledState(true);

            Config.ReloadFromProcessSwitch();
            PropertyManager.ReloadFromProcessSwitch();

            //Refresh PropertyFile stream - It is currently has read only permissions
            PropertyManager.PropertyFile.RefreshStream();
        }

        internal static void OnShutdown()
        {
            LogProperties.PropertyManager.SaveToFile();

            Config.TrySave();
            UtilityLogger.Log("Disabling log files");

            //End all active log sessions
            foreach (LogProperties properties in LogProperties.PropertyManager.Properties)
            {
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

                    if (IsControllingAssembly)
                    {
                        //BepInEx log file requires special treatment. This log file cannot be replaced on game start like the other log files
                        //To account for this, replace this log file when the game closes
                        logFile.Properties.CreateTempFile(true);
                        logFile.Properties.RemoveTempFile();
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
