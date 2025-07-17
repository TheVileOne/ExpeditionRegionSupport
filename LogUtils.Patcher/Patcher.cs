using BepInEx.MultiFolderLoader;
using Mono.Cecil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AssemblyCandidate = (System.Version Version, string Path);

namespace LogUtils.Patcher;

public static class Patcher
{
    private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "patcher.log");

    public static IEnumerable<string> TargetDLLs => GetDLLs();

    /// <summary>
    /// The latest version of LogUtils will be stored in this field
    /// </summary>
    internal static AssemblyCandidate Target;

    public static IEnumerable<string> GetDLLs()
    {
        if (File.Exists(LogFilePath)) File.Delete(LogFilePath);
        Log("=== Patcher.GetDLLs() start ===");

        applyHooks();
        yield return "Assembly-CSharp.dll";
    }

    private static void applyHooks()
    {
        new Hook(typeof(ModManager).GetMethod("InitInternal", BindingFlags.Static | BindingFlags.NonPublic), onModsCollected).Apply();
        new Hook(typeof(ModManager).GetMethod("AddMod", BindingFlags.Static | BindingFlags.NonPublic), onEnabledMod).Apply();
    }

    private static void onEnabledMod(Action<string> orig, string path)
    {
        int lastModCount = ModManager.Mods.Count;

        orig(path);

        if (lastModCount != ModManager.Mods.Count) //Has orig added a new mod entry
        {
            Mod currentMod = ModManager.Mods[ModManager.Mods.Count - 1];

            string targetPath = null;
            try
            {
                targetPath = AssemblyUtils.FindAssembly(currentMod.PluginsPath, "LogUtils.dll");

                if (targetPath != null)
                {
                    var version = AssemblyName.GetAssemblyName(targetPath).Version;
                    Log($"Found candidate {targetPath} v{version}");

                    if (Target.Path == null || Target.Version < version)
                        Target = new AssemblyCandidate(version, targetPath);
                }
            }
            catch (IOException ex)
            {
                Log($"Error trying to access {currentMod.PluginsPath}: {ex.Message}");
            }
            catch (Exception ex)
            {
                Log($"Error reading version from {targetPath}: {ex.Message}");
            }
        }
    }

    private static void onModsCollected(Action orig)
    {
        orig();

        if (Target.Path == null)
        {
            Log("No LogUtils DLLs associated with enabled mods detected.");
            return;
        }

        if (Target.Path != null)
        {
            Log("Loading latest LogUtils DLL: " + Target.Path);
            Assembly.LoadFrom(Target.Path);
        }
        else
        {
            Log("No LogUtils assembly found.");
        }
    }

    public static void Patch(AssemblyDefinition assembly)
    {
        Log("Starting patch process for assembly: " + assembly.Name.Name);
        try
        {
            Log("Patching completed successfully.");
        }
        catch (Exception ex)
        {
            Log("Error during patching: " + ex);
            throw;
        }
    }

    internal static void Log(string message)
    {
        try
        {
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{Environment.NewLine}";
            File.AppendAllText(LogFilePath, line);
        }
        catch (Exception ex)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            File.AppendAllText(LogFilePath, $"{timestamp} - !!!Exception: {ex}{Environment.NewLine}");
        }
    }
}
