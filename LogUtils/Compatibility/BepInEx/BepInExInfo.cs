using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using DiskLogListener = LogUtils.Compatibility.BepInEx.Listeners.DiskLogListener;
using Logging = BepInEx.Logging.Logger;

namespace LogUtils.Compatibility.BepInEx
{
    /// <summary>
    /// Contains references to state controlled by BepInEx
    /// </summary>
    public static class BepInExInfo
    {
        public static ICollection<ILogListener> Listeners => Logging.Listeners;

        internal static DiskLogListener LogListener { get; private set; }

        /// <summary>
        /// The GameObject that contains the game, and all modded assemblies
        /// </summary>
        public static GameObject ManagerObject => Chainloader.ManagerObject;

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
            PluginInfos = Chainloader.PluginInfos.Values.OrderBy(p => p.Location).ToList();
        }

        /// <summary>
        /// Gets the plugin instance associated with the provided assembly
        /// </summary>
        public static BaseUnityPlugin GetPlugin(Assembly assembly)
        {
            return GetPluginInfo(assembly)?.Instance;
        }

        /// <summary>
        /// Gets the PluginInfo instance associated with the provided assembly
        /// </summary>
        public static PluginInfo GetPluginInfo(Assembly assembly)
        {
            if (assembly == null)
                return null;

            return PluginInfos.Find(p => p.Location == assembly.Location);
        }

        /// <summary>
        /// Gets the plugin metadata associated with the provided assembly
        /// </summary>
        public static BepInPlugin GetPluginMetadata(Assembly assembly)
        {
            return GetPluginInfo(assembly)?.Metadata;
        }
    }
}
