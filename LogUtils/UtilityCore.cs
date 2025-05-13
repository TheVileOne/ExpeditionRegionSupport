using LogUtils.Compatibility;
using LogUtils.Console;
using LogUtils.Diagnostics.Tools;
using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Helpers;
using LogUtils.IPC;
using LogUtils.Properties;
using LogUtils.Requests;
using LogUtils.Threading;
using LogUtils.Timers;
using Menu;
using System;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Text;
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
        internal static UtilitySetup.Build Build
        {
            get
            {
#if DEBUG
                return UtilitySetup.Build.DEVELOPMENT;
#else
                return UtilitySetup.Build.RELEASE;
#endif
            }
        }

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

        /// <summary>
        /// Indicates whether the utility initialization declaration message has been reported - this should only happen once 
        /// </summary>
        private static bool hasAnnouncedBuild;

        /// <summary>
        /// Handles cross-mod data storage for the utility
        /// </summary>
        public static SharedDataHandler DataHandler;

        public static PersistenceManager PersistenceManager;

        /// <summary>
        /// An IPC client representing the Rain World process that LogUtils operates on
        /// </summary>
        public static PipeServerNew.Client ProcessClient;

        /// <summary>
        /// An IPC server allowing basic communication between other Rain World processes
        /// </summary>
        public static PipeServerNew.Server ProcessServer;

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
        }

        internal static void Initialize()
        {
            if (IsInitialized || initializingInProgress) return; //Initialize may be called several times during the init process

            initializingInProgress = true;

            UtilitySetup.CurrentStep = UtilitySetup.InitializationStep.NOT_STARTED;

#if DEBUG
            //Used for debugging purposes only - not meant for production builds
            SetupDebugEnvironment();
#endif

            try
            {
                UtilitySetup.CurrentStep = UtilitySetup.InitializationStep.INITALIZE_CORE_LOGGER;
                while (UtilitySetup.CurrentStep != UtilitySetup.InitializationStep.COMPLETE)
                {
                    UtilityLogger.DebugLog("Applying " + UtilitySetup.CurrentStep);
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

            if (Build == UtilitySetup.Build.DEVELOPMENT)
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
                case UtilitySetup.InitializationStep.INITALIZE_CORE_LOGGER:
                    {
                        UnityLogger.EnsureLogTypeCapacity(UtilityConsts.CUSTOM_LOGTYPE_LIMIT);
                        UtilityLogger.Initialize();

                        if (!hasAnnouncedBuild)
                            AnnounceBuild();

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

                        nextStep = UtilitySetup.InitializationStep.INITIALIZE_ENUMS;
                        break;
                    }
                case UtilitySetup.InitializationStep.INITIALIZE_ENUMS:
                    {
                        //These are initialized after components, because they internally depend on SharedDataHandler
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
                            LogConsole.Initialize();

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
            Scheduler = ComponentUtils.GetOrCreate<EventScheduler>(UtilityConsts.ComponentTags.SCHEDULER, out _);
            PersistenceManager = ComponentUtils.GetOrCreate<PersistenceManager>(UtilityConsts.ComponentTags.PERSISTENCE_MANAGER, out _);
            DataHandler = ComponentUtils.GetOrCreate<SharedDataHandler>(UtilityConsts.ComponentTags.SHARED_DATA, out _);
            RequestHandler = ComponentUtils.GetOrCreate<LogRequestHandler>(UtilityConsts.ComponentTags.REQUEST_DATA, out _);

            ProcessServer = ComponentUtils.GetOrCreate<PipeServerNew.Server>(UtilityConsts.ComponentTags.IPC_SERVER, out _);
            ProcessClient = ComponentUtils.GetOrCreate<PipeServerNew.Client>(UtilityConsts.ComponentTags.IPC_CLIENT, out _);

            PropertyManager = ComponentUtils.GetOrCreate<PropertyDataController>(UtilityConsts.ComponentTags.PROPERTY_DATA, out bool wasCreated);

            if (wasCreated)
            {
                IsControllingAssembly = true;
                PropertyManager.SetPropertiesFromFile();
            }
        }

        private static void test()
        {
            UtilityLogger.DebugLog("Running tasks");
            System.Threading.Tasks.Task.Run(SendByteAndReceiveResponseContinuous);
            System.Threading.Tasks.Task.Run(ReceiveByteAndRespondContinuous);
        }

        static NamedPipeServerStream namedPipeServer;

        private static void SendByteAndReceiveResponseContinuous()
        {
            try
            {
                namedPipeServer = new NamedPipeServerStream("test-pipe");
            }
            catch (Exception ex)
                {
                UtilityLogger.LogError(ex);
            }

            string[] testValues = ["test", "yes", "x"];

            var testEnumerator = testValues.GetEnumerator();

            //using (NamedPipeServerStream namedPipeServer = new NamedPipeServerStream("test-pipe"))
            {
                UtilityLogger.Log("Server waiting for a connection...");
                namedPipeServer.WaitForConnection();
                UtilityLogger.Log("A client has connected, send a byte from the server: ");

                testEnumerator.MoveNext();
                string b = (string)testEnumerator.Current;

                UtilityLogger.Log(string.Format("About to send byte {0} to client.", b));
                namedPipeServer.WriteByte(Encoding.UTF8.GetBytes(b).First());
                UtilityLogger.Log("Byte sent, waiting for response from client...");
                int byteFromClient = namedPipeServer.ReadByte();
                UtilityLogger.Log(string.Format("Received byte response from client: {0}", byteFromClient));
                while (byteFromClient != 120)
                {
                    UtilityLogger.Log("Send a byte response: ");

                    testEnumerator.MoveNext();
                    b = (string)testEnumerator.Current;

                    UtilityLogger.Log(string.Format("About to send byte {0} to client.", b));
                    namedPipeServer.WriteByte(Encoding.UTF8.GetBytes(b).First());
                    UtilityLogger.Log("Byte sent, waiting for response from client...");
                    byteFromClient = namedPipeServer.ReadByte();
                    UtilityLogger.Log(string.Format("Received byte response from client: {0}", byteFromClient));
                }
                UtilityLogger.Log("Server exiting, client sent an 'x'...");
            }
        }

        private static void ReceiveByteAndRespondContinuous()
        {
            string[] testValues = ["no", "fear", "x"];

            var testEnumerator = testValues.GetEnumerator();

            using (NamedPipeClientStream namedPipeClient = new NamedPipeClientStream("test-pipe"))
            {
                try
                {
                    namedPipeClient.Connect();
                }
                catch (Exception ex)
                {
                    UtilityLogger.LogError(ex);
                }
                UtilityLogger.Log("Client connected to the named pipe server. Waiting for server to send first byte...");
                UtilityLogger.Log(string.Format("The server sent a single byte to the client: {0}", namedPipeClient.ReadByte()));
                UtilityLogger.Log("Provide a byte response from client: ");

                testEnumerator.MoveNext();
                string b = (string)testEnumerator.Current;

                UtilityLogger.Log(string.Format("About to send byte {0} to server.", b));
                namedPipeClient.WriteByte(Encoding.UTF8.GetBytes(b).First());
                while (b != "x")
                {
                    UtilityLogger.Log(string.Format("The server sent a single byte to the client: {0}", namedPipeClient.ReadByte()));
                    UtilityLogger.Log("Provide a byte response from client: ");

                    testEnumerator.MoveNext();
                    b = (string)testEnumerator.Current;

                    UtilityLogger.Log(string.Format("About to send byte {0} to server.", b));
                    namedPipeClient.WriteByte(Encoding.UTF8.GetBytes(b).First());
                }

                UtilityLogger.Log("Client chose to disconnect...");
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

        internal static void AnnounceBuild()
        {
            UtilityLogger.Logger.LogMessage($"{UtilityConsts.UTILITY_NAME} {AssemblyVersion} {Build} BUILD started");
            hasAnnouncedBuild = true;
        }

        internal static void SetupDebugEnvironment()
        {
            //Ensure that Logger is always initialized before the debug environment is setup
            if (UtilityLogger.Logger == null)
            {
                UtilityLogger.Initialize();
                AnnounceBuild();
            }
            DeadlockTester.Run();
        }

        internal static void OnShutdown()
        {
            LogProperties.PropertyManager.SaveToFile();

            //End all active log sessions
            LogProperties.PropertyManager.Properties.ForEach(properties =>
            {
                if (properties.ID.Equals(LogID.BepInEx))
                    return;

                using (properties.FileLock.Acquire())
                {
                    properties.EndLogSession();
                    properties.AllowLogging = false; //No new logs should happen beyond this point
                }
            });

            LogID logFile = LogID.BepInEx;

            if (logFile.Properties.FileExists)
            {
                using (logFile.Properties.FileLock.Acquire())
                {
                    //End the log session later than the others to ensure that session state is reported to file
                    logFile.Properties.EndLogSession();
                    logFile.Properties.AllowLogging = false;

                    //BepInEx log file requires special treatment. This log file cannot be replaced on game start like the other log files
                    //To account for this, replace this log file when the game closes
                    logFile.Properties.CreateTempFile(true);
                }
            }

            //Stop listening for log events
            BepInExAdapter.DisposeListeners();
            LogTasker.Close();
        }
    }
}
