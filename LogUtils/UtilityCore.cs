using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogUtils
{
    public static class UtilityCore
    {
        public static ManualLogSource BaseLogger { get; private set; }

        public static bool IsInitialized { get; private set; }

        private static bool initializingInProgress;

        internal static void Initialize()
        {
            if (initializingInProgress) return; //Initialize may be called several times during the init process

            initializingInProgress = true;

            BaseLogger = BepInEx.Logging.Logger.Sources.FirstOrDefault(l => l.SourceName == "LogUtils") as ManualLogSource
                      ?? BepInEx.Logging.Logger.CreateLogSource("LogUtils");

            ApplyHooks();
            LoadComponents();
            initializingInProgress = false;

            IsInitialized = true;
        }

        /// <summary>
        /// Apply hooks used by the utility module
        /// </summary>
        internal static void ApplyHooks()
        {
            Logger.ApplyHooks();
        }

        /// <summary>
        /// Releases, and then reapply hooks used by the utility module 
        /// </summary>
        public static void ReloadHooks()
        {
            UnloadHooks();
            ApplyHooks();
        }

        /// <summary>
        /// Releases all hooks used by the utility module
        /// </summary>
        public static void UnloadHooks()
        {
            //TODO: Logger hooks don't have unload logic
        }

        /// <summary>
        /// Creates, or establishes a reference to an existing instance of necessary utility components
        /// </summary>
        internal static void LoadComponents()
        {
            LogProperties.PropertyManager = PropertyDataController.GetOrCreate(out bool wasCreated);

            if (wasCreated)
                LogProperties.PropertyManager.ReadFromFile();
        }
    }
}
