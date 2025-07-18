using BepInEx;
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
    internal static Logger Logger = new Logger();

    public static IEnumerable<string> TargetDLLs => GetDLLs();

    public static IEnumerable<string> GetDLLs()
    {
        yield return "BepInEx.MultiFolderLoader.dll"; //Our patcher code needs to run after this patcher runs

        string patcherPath = Path.Combine(Paths.PatcherPluginPath, "LogUtils.VersionLoader.dll");

        if (File.Exists(patcherPath)) //The file may have been moved by the MultiFolderLoader
        {
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
