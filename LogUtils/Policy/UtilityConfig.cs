using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace LogUtils.Policy
{
    /// <summary>
    /// A container for utility settings, and user preferences
    /// </summary>
    public sealed class UtilityConfig
    {
        /// <summary>
        /// Path to the LogUtils core config file
        /// </summary>
        public static readonly string CONFIG_PATH = Path.Combine(Paths.ConfigPath, "LogUtils.cfg");

        internal ConfigFile ConfigFile;

        internal object ConfigLock;

        internal Dictionary<ConfigDefinition, IConfigEntry> CachedEntries = new Dictionary<ConfigDefinition, IConfigEntry>();

        /// <summary>
        /// Retrieves a cached config entry
        /// </summary>
        public IConfigEntry this[ConfigDefinition key]
        {
            get
            {
                lock (ConfigLock)
                    return CachedEntries[key];
            }
        }

        /// <inheritdoc cref="this[ConfigDefinition]"/>
        public IConfigEntry this[string section, string key] => this[new ConfigDefinition(section, key)];

        private UtilityConfig()
        {
            ConfigFile = new ConfigFile(CONFIG_PATH, true)
            {
                SaveOnConfigSet = false //Saving on set causes too many issues
            };
            ConfigLock = ConfigFile._ioLock;
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

        internal void ReloadFromProcessSwitch()
        {
            lock (ConfigLock)
            {
                ConfigFile.SettingChanged += detectChanges;

                bool hasChanges = false;
                if (TryReload() && hasChanges)
                    ReloadCache(); //Any entry changes made by this process will be overwritten when this is called
                ConfigFile.SettingChanged -= detectChanges;

                //We want to know if at least one setting has changed during the reload
                void detectChanges(object config, SettingChangedEventArgs ignored)
                {
                    hasChanges = true;
                }
            }
        }

        /// <summary>
        /// Assigns values stored in the config to their associated policy
        /// </summary>
        public void ReloadCache()
        {
            var entries = CachedEntries.Values;

            lock (ConfigLock)
            {
                foreach (IConfigEntry entry in entries)
                    entry.SetValueFromBase();
            }
        }

        public CachedConfigEntry<T> Bind<T>(ConfigDefinition definition, T defaultValue, ConfigDescription description = null)
        {
            bool hasDefinition = ConfigFile.OrphanedEntries.ContainsKey(definition);

            var entry = new CachedConfigEntry<T>(ConfigFile.Bind(definition, defaultValue, description));

            if (!hasDefinition)
                entry.Mark();

            CachedEntries[definition] = entry;
            return entry;
        }

        /// <summary>
        /// Assigns the default value for all config entries
        /// </summary>
        public void ResetToDefaults(SaveOption saveOption = SaveOption.DontSave)
        {
            bool saveAfterProcessing = saveOption == SaveOption.SaveImmediately;

            if (saveAfterProcessing)
                saveOption = SaveOption.SaveLater; //Mark entries instead to avoid extra file operations

            foreach (var entry in CachedEntries.Values)
                entry.ResetToDefault(saveOption);

            if (saveAfterProcessing)
                TrySave();
        }

        /// <summary>
        /// Process safe method of saving entry values to the config file
        /// </summary>
        public bool TrySave()
        {
            var markedEntries = CachedEntries.Values.Where(e => e.IsMarked).ToArray();

            //Entries that are updated here will get overwritten on a process switch. Behavior may be changed at a later point which allows
            //marked entries to be handled on a case by case basis
            foreach (IConfigEntry entry in markedEntries)
            {
                entry.UpdateBaseEntry();
                entry.Unmark();
            }

            if (!UtilityCore.IsControllingAssembly) //Avoid possible unwanted overwrites from alternate processes
                return false;

            bool configSaved = TryInvoke(ConfigFile.Save);

            if (configSaved)
                UtilityLogger.Log("Config data saved");
            else
                UtilityLogger.LogWarning("Unable to save config");
            return configSaved;
        }

        /// <summary>
        /// Process safe method of reading entry values from the config file
        /// </summary>
        /// <remarks>This method does not affect the value cache. To assign values to cache, also invoke <see cref="ReloadCache"/>.</remarks>
        public bool TryReload()
        {
            bool configRead = TryInvoke(ConfigFile.Reload);

            if (configRead)
                UtilityLogger.Log("Config data read from file");
            else
                UtilityLogger.LogWarning("Unable to read config");
            return configRead;
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

    /// <summary>
    /// Represents options for saving config entries to file
    /// </summary>
    public enum SaveOption
    {
        DontSave,
        SaveImmediately,
        SaveLater
    }
}
