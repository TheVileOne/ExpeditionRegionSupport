using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace LogUtils.Policy
{
    /// <summary>
    /// A container for utility settings, and user preferences
    /// </summary>
    public sealed class UtilityConfig : ConfigFile
    {
        /// <summary>
        /// Path to the LogUtils core config file
        /// </summary>
        public static readonly string CONFIG_PATH = Path.Combine(Paths.ConfigPath, "LogUtils.cfg");


        internal Dictionary<ConfigDefinition, IConfigEntry> CachedEntries = new Dictionary<ConfigDefinition, IConfigEntry>();

        internal List<ConfigEntryBase> NewEntries = new List<ConfigEntryBase>();

        /// <summary>
        /// Retrieves a cached config entry
        /// </summary>
        public new IConfigEntry this[ConfigDefinition key]
        {
            get
            {
                lock (_ioLock)
                    return CachedEntries[key];
            }
        }

        /// <inheritdoc cref="this[ConfigDefinition]"/>
        public new IConfigEntry this[string section, string key] => this[new ConfigDefinition(section, key)];

        private UtilityConfig() : base(CONFIG_PATH, true)
        {
            SaveOnConfigSet = false; //Saving on set causes too many issues
        }

        internal static void Initialize()
        {
            UtilityCore.Config = new UtilityConfig();
            InitializeEntries();
        }

        internal static void InitializeEntries()
        {
            DebugPolicy.InitializeEntries();
            PatcherPolicy.InitializeEntries();
            TestCasePolicy.InitializeEntries();
            LogRequestPolicy.InitializeEntries();
        }

        /// <summary>
        /// Assigns values stored in the config to their associated policy
        /// </summary>
        public void ReloadValues()
        {
            var entries = CachedEntries.Values;

            lock (_ioLock)
            {
                foreach (IConfigEntry entry in entries)
                    entry.SetValueFromBase();
            }
        }

        public new CachedConfigEntry<T> Bind<T>(ConfigDefinition definition, T defaultValue, ConfigDescription description = null)
        {
            bool hasDefinition = OrphanedEntries.ContainsKey(definition);

            var entry = new CachedConfigEntry<T>(base.Bind(definition, defaultValue, description));

            CachedEntries[definition] = entry;

            if (!hasDefinition)
                NewEntries.Add(entry);
            return entry;
        }

        /// <summary>
        /// Resolves entry data differences between cached config entries and the config file
        /// </summary>
        public void SyncData()
        {
            //TODO: Need a process for handling marked entries for saving on game close
            if (NewEntries.Count == 0) return;

            if (UtilityCore.IsControllingAssembly)
                TrySave();

            NewEntries.Clear(); //Entries will be saved from a different process
        }

        /// <summary>
        /// Process safe method of saving entry values to the config file
        /// </summary>
        public bool TrySave()
        {
            if (TryInvoke(Save))
            {
                UtilityLogger.Log("Config data saved");
                return true;
            }
            UtilityLogger.LogWarning("Unable to save config");
            return false;
        }

        /// <summary>
        /// Process safe method of reading entry values from the config file
        /// </summary>
        public bool TryReload()
        {
            if (TryInvoke(Reload))
            {
                UtilityLogger.Log("Config data read from file");
                return true;
            }
            UtilityLogger.LogWarning("Unable to read config");
            return false;
        }

        internal static bool TryInvoke(Action action)
        {
            /*
             * Read/Write operation may fail due to differing FileShare permissions when this file is accessed from multiple processes.
             * Give a few attempts to retry to improve the chance that save operation will be successful
             */
            int retryCount = 3;
            do
            {
                try
                {
                    action.Invoke();
                    return true;
                }
                catch (IOException)
                {
                    Thread.Sleep(25);
                    retryCount--;
                }
            }
            while (retryCount > 0);
            return false;
        }
    }
}
