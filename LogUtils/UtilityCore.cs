using BepInEx.Logging;
using System;
using System.Linq;

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

        public static ManualLogSource BaseLogger { get; private set; }

        public static PropertyDataController PropertyManager;

        /// <summary>
        /// Handles cross-mod data storage for the utility
        /// </summary>
        public static SharedDataHandler DataHandler;

        internal static void Initialize()
        {
            if (IsInitialized || initializingInProgress) return; //Initialize may be called several times during the init process

            initializingInProgress = true;

            BaseLogger = BepInEx.Logging.Logger.Sources.FirstOrDefault(l => l.SourceName == "LogUtils") as ManualLogSource
                      ?? BepInEx.Logging.Logger.CreateLogSource("LogUtils");

            LoadComponents();
            GameHooks.Initialize();
            initializingInProgress = false;

            IsInitialized = true;
        }

        /// <summary>
        /// Creates, or establishes a reference to an existing instance of necessary utility components
        /// </summary>
        internal static void LoadComponents()
        {
            PropertyManager = ComponentUtils.GetOrCreate<PropertyDataController>("Log Properties", out bool wasCreated);

            if (wasCreated)
            {
                IsControllingAssembly = true;
                PropertyManager.ReadFromFile();
            }

            DataHandler = ComponentUtils.GetOrCreate<SharedDataHandler>("Shared Data", out _);
        }
    }
}
