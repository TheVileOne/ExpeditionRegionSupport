using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using LogUtils.Helpers.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DiskLogListener = LogUtils.Compatibility.Listeners.DiskLogListener;

namespace LogUtils.Compatibility
{
    public static class BepInExInfo
    {
        public static ConfigFile Config;

        public static ICollection<ILogListener> Listeners => BepInEx.Logging.Logger.Listeners;

        internal static DiskLogListener LogListener { get; private set; }

        public static List<PluginInfo> PluginInfos;

        /// <summary>
        /// Removes listener from BepInEx Listeners collection, and disposes the listener instance
        /// </summary>
        internal static void Close(ILogListener listener)
        {
            if (Listeners.Remove(listener))
                listener.TryDispose();
        }

        /// <summary>
        /// Finds the first ILogListener of a given type, or null if not found
        /// </summary>
        internal static T Find<T>() where T : ILogListener
        {
            return Listeners.OfType<T>().FirstOrDefault();
        }

        internal static void InitializeListener()
        {
            Listeners.Add(LogListener = new DiskLogListener(new TimedLogWriter()));
        }

        internal static void BuildInfoCache()
        {
            Config = GetConfigFile();
            PluginInfos = Chainloader.PluginInfos.Values.OrderBy(p => p.Location).ToList();
        }

        /// <summary>
        /// Gets the BepInEx core config file through reflection
        /// </summary>
        internal static ConfigFile GetConfigFile()
        {
            Type type = typeof(ConfigFile);
            PropertyInfo configProperty = type.GetProperty("CoreConfig", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            return configProperty.GetValue(null, null) as ConfigFile;
        }

        public static BaseUnityPlugin GetPlugin(Assembly assembly)
        {
            return GetPluginInfo(assembly)?.Instance;
        }

        public static PluginInfo GetPluginInfo(Assembly assembly)
        {
            if (assembly == null)
                return null;

            return PluginInfos.Find(p => p.Location == assembly.Location);
        }

        public static BepInPlugin GetPluginMetadata(Assembly assembly)
        {
            return GetPluginInfo(assembly)?.Metadata;
        }
    }
}
