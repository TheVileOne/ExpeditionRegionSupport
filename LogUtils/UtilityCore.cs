using LogUtils.CompatibilityServices;
using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Helpers;
using LogUtils.Properties;
using LogUtils.Threading;
using Menu;
using System;
using System.Linq;
using System.Reflection;

namespace LogUtils
{
    public static class UtilityCore
    {
        public static Assembly ExecutingAssembly = Assembly.GetExecutingAssembly();

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
        /// The initialized state encountered a problem during initialization
        /// </summary>
        public static bool InitializedWithErrors { get; private set; }

        /// <summary>
        /// The initialization process is in progress for the current assembly
        /// </summary>
        private static bool initializingInProgress;

        public static PersistenceManager PersistenceManager;

        public static PropertyDataController PropertyManager;

        /// <summary>
        /// Handles cross-mod data storage for the utility
        /// </summary>
        public static SharedDataHandler DataHandler;

        /// <summary>
        /// Handles log requests between different loggers
        /// </summary>
        public static LogRequestHandler RequestHandler;

        public static FrameTimer Scheduler;

        public static int ThreadID;

        internal static void Initialize()
        {
            if (IsInitialized || initializingInProgress) return; //Initialize may be called several times during the init process

            initializingInProgress = true;

            UtilitySetup.InitializationStep currentStep = UtilitySetup.InitializationStep.NOT_STARTED;
            try
            {
                currentStep = UtilitySetup.InitializationStep.INITALIZE_CORE_LOGGER;
                while (currentStep != UtilitySetup.InitializationStep.COMPLETE)
                {
                    currentStep = ApplyStep(currentStep);
                }
            }
            catch (Exception ex)
            {
                InitializedWithErrors = true;

                //TODO: An exception during utility initialization is most likely unrecoverable. Utility must try to restore original logging functionality here
                UtilityLogger.LogFatal("A fatal error has occurred during setup process. Utility will no longer function as expected");
                UtilityLogger.LogFatal($"FAILED STEP: {currentStep}");
                UtilityLogger.LogFatal(ex);
            }

            initializingInProgress = false;
            IsInitialized = true;
        }

        internal static UtilitySetup.InitializationStep ApplyStep(UtilitySetup.InitializationStep currentStep)
        {
            UtilitySetup.InitializationStep nextStep = currentStep;
            switch (currentStep)
            {
                case UtilitySetup.InitializationStep.INITALIZE_CORE_LOGGER:
                    {
                        UtilityLogger.EnsureLogTypeCapacity(UtilityConsts.CUSTOM_LOGTYPE_LIMIT);
                        UtilityLogger.Initialize();

                        nextStep = UtilitySetup.InitializationStep.START_SCHEDULER;
                        break;
                    }
                case UtilitySetup.InitializationStep.START_SCHEDULER:
                    {
                        LogTasker.Start();

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
                        LoadComponents();

                        nextStep = UtilitySetup.InitializationStep.INITIALIZE_LOGIDS;
                        break;
                    }
                case UtilitySetup.InitializationStep.INITIALIZE_LOGIDS:
                    {
                        LogID.InitializeLogIDs(); //This should be called for every assembly that initializes

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
            }

            //The steps after this point should only be run by a single assembly
            if (nextStep != currentStep)
                return nextStep;

            if (IsControllingAssembly)
            {
                switch (currentStep)
                {
                    case UtilitySetup.InitializationStep.ADAPT_LOGGING_SYSTEM:
                        {
                            //This must be run before late initialized log files are handled to allow BepInEx log file to be moved
                            BepInExAdapter.Run();

                            nextStep = UtilitySetup.InitializationStep.POST_LOGID_PROCESSING;
                            break;
                        }
                    case UtilitySetup.InitializationStep.POST_LOGID_PROCESSING:
                        {
                            PropertyManager.ProcessLogFiles();

                            //Listen for Unity log requests while the log file is unavailable
                            if (!LogID.Unity.Properties.CanBeAccessed)
                                UtilityLogger.ReceiveUnityLogEvents = true;

                            nextStep = UtilitySetup.InitializationStep.APPLY_HOOKS;
                            break;
                        }
                    case UtilitySetup.InitializationStep.APPLY_HOOKS:
                        {
                            AppDomain.CurrentDomain.UnhandledException += (o, e) => RequestHandler.DumpRequestsToFile();
                            GameHooks.Initialize();
                            break;
                        }
                }

                if (nextStep != currentStep)
                    return nextStep;
            }
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
            Scheduler = ComponentUtils.GetOrCreate<FrameTimer>(UtilityConsts.ComponentTags.SCHEDULER, out _);
            PersistenceManager = ComponentUtils.GetOrCreate<PersistenceManager>(UtilityConsts.ComponentTags.PERSISTENCE_MANAGER, out _);
            DataHandler = ComponentUtils.GetOrCreate<SharedDataHandler>(UtilityConsts.ComponentTags.SHARED_DATA, out _);
            RequestHandler = ComponentUtils.GetOrCreate<LogRequestHandler>(UtilityConsts.ComponentTags.REQUEST_DATA, out _);

            PropertyManager = ComponentUtils.GetOrCreate<PropertyDataController>(UtilityConsts.ComponentTags.PROPERTY_DATA, out bool wasCreated);

            if (wasCreated)
            {
                IsControllingAssembly = true;
                PropertyManager.SetPropertiesFromFile();
            }
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
    }
}
