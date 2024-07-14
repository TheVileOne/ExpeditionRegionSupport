using BepInEx.Logging;
using LogUtils.Helpers;
using LogUtils.Properties;
using System;
using System.Collections.Generic;
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
            PropertyManager = ComponentUtils.GetOrCreate<PropertyDataController>(UtilityConsts.ComponentTags.PROPERTY_DATA, out bool wasCreated);

            if (wasCreated)
            {
                IsControllingAssembly = true;
                PropertyManager.ReadFromFile();
            }

            DataHandler = ComponentUtils.GetOrCreate<SharedDataHandler>(UtilityConsts.ComponentTags.SHARED_DATA, out _);
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
    }
}
