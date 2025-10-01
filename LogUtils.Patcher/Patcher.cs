using BepInEx;
using BepInEx.Logging;
using BepInEx.MultiFolderLoader;
using Mono.Cecil;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using AssemblyCandidate = (System.Version Version, string Path);

namespace LogUtils.VersionLoader;

public static class Patcher
{
    /// <summary>
    /// Primary logger for patcher service
    /// </summary>
    internal static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("LogUtils.VersionLoader");

    public static IEnumerable<string> TargetDLLs => GetDLLs();

    public static IEnumerable<string> GetDLLs()
    {
        yield return "BepInEx.MultiFolderLoader.dll"; //Our patcher code needs to run after this patcher runs

        string patcherPath = Path.Combine(Paths.PatcherPluginPath, "LogUtils.VersionLoader.dll");

        if (File.Exists(patcherPath)) //The file may have been moved by the MultiFolderLoader
        {
            LogEventCache eventCache = new LogEventCache();

            BepInEx.Logging.Logger.Listeners.Add(eventCache);
            Logger.LogMessage("=== Patcher.GetDLLs() start ===");

            AssemblyCandidate target = AssemblyUtils.FindLatestAssembly(getSearchPaths(), "LogUtils.dll");

            if (target.Path != null)
            {
                Logger.LogMessage("Loading latest LogUtils DLL: " + target.Path);
                Assembly.LoadFrom(target.Path);
            }
            else
            {
                Logger.LogInfo("No LogUtils assembly found.");
            }
            eventCache.IsEnabled = false; //For some reason events will be handled twice if we keep event listening enabled
        }
    }

    private static IEnumerable<string> getSearchPaths()
    {
        foreach (Mod mod in ModManager.Mods)
        {
            yield return mod.PluginsPath;

            //Check the mod's root directory only if we did not find results in the current plugin directory
            if (!AssemblyUtils.LastFoundAssembly.HasPath(mod.PluginsPath))
                yield return mod.ModDir;
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Method required for patcher detection")]
    public static void Patch(AssemblyDefinition assembly)
    {
    }
}
