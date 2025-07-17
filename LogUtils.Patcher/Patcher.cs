using BepInEx.MultiFolderLoader;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AssemblyCandidate = (System.Version Version, string Path);

namespace LogUtils.Patcher;

public static class Patcher
{
    private static readonly string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "patcher.log");

    public static IEnumerable<string> TargetDLLs => GetDLLs();

    public static IEnumerable<string> GetDLLs()
    {
        if (File.Exists(logFilePath))
            File.Delete(logFilePath);
        Log("=== Patcher.GetDLLs() start ===");

        yield return "BepInEx.MultiFolderLoader.dll";

        AssemblyCandidate target = AssemblyUtils.FindLatestAssembly(getSearchPaths(), "LogUtils.dll");

        if (target.Path != null)
        {
            Log("Loading latest LogUtils DLL: " + target.Path);
            Assembly.LoadFrom(target.Path);
        }
        else
        {
            Log("No LogUtils assembly found.");
        }
    }

    private static IEnumerable<string> getSearchPaths()
    {
        return ModManager.Mods.Select(mod => mod.PluginsPath);
    }

    public static void Patch(AssemblyDefinition assembly)
    {
        //Log("Starting patch process for assembly: " + assembly.Name.Name);
        //try
        //{
        //    Log("Patching completed successfully.");
        //}
        //catch (Exception ex)
        //{
        //    Log("Error during patching: " + ex);
        //    throw;
        //}
    }

    internal static void Log(string message)
    {
        try
        {
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{Environment.NewLine}";
            File.AppendAllText(logFilePath, line);
        }
        catch (Exception ex)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            File.AppendAllText(logFilePath, $"{timestamp} - !!!Exception: {ex}{Environment.NewLine}");
        }
    }
}
