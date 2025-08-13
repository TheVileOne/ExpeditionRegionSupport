using BepInEx;
using BepInEx.Logging;
using BepInEx.MultiFolderLoader;
using Mono.Cecil;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AssemblyCandidate = (System.Version Version, string Path);

namespace LogUtils.Patcher;

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
            BepInEx.Logging.Logger.Listeners.Add(new LogEventCache());
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
        }
    }

    private static IEnumerable<string> getSearchPaths()
    {
        return ModManager.Mods.Select(mod => mod.PluginsPath);
    }

    public static void Patch(AssemblyDefinition assembly)
    {
    }
}
