using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LogUtils.Patcher;

public static class Patcher
{
    private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "patcher.log");

    private static readonly string ModsRootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RainWorld_Data", "StreamingAssets", "mods");

    public static IEnumerable<string> TargetDLLs => GetDLLs();

    public static IEnumerable<string> GetDLLs()
    {
        if (File.Exists(LogFilePath)) File.Delete(LogFilePath);
        Log("=== Patcher.GetDLLs() start ===");

        var latest = FindLatestLogUtilsDll(ModsRootPath);
        if (latest != null)
        {
            Log("Loading latest LogUtils DLL: " + latest);
            Assembly.LoadFrom(latest);
            yield return latest;
        }
        else
        {
            Log("No LogUtils plugin found; falling back.");
        }

        yield return "Assembly-CSharp.dll";
    }

    private static string FindLatestLogUtilsDll(string rootPath)
    {
        if (!Directory.Exists(rootPath))
        {
            Log("Mods root not found: " + rootPath);
            return null;
        }

        var dllFiles = Directory
          .EnumerateFiles(rootPath, "LogUtils.dll", SearchOption.AllDirectories)
          .Where(path => path.Split(Path.DirectorySeparatorChar)
          .Any(segment => segment.Equals("plugins", StringComparison.OrdinalIgnoreCase)));

        if (!dllFiles.Any())
        {
            Log("No LogUtils DLLs detected under mods root.");
            return null;
        }

        string latestVersionPath = null;
        var latestVersion = new Version(0, 0, 0, 0);

        foreach (var file in dllFiles)
        {
            try
            {
                var version = AssemblyName.GetAssemblyName(file).Version;
                Log($"Found candidate {file} v{version}");
                if (version > latestVersion)
                {
                    latestVersion = version;
                    latestVersionPath = file;
                }
            }
            catch (Exception ex)
            {
                Log($"Error reading version from {file}: {ex.Message}");
            }
        }

        if (latestVersionPath != null)
            Log($"Selected latest: {latestVersionPath} v{latestVersion}");

        return latestVersionPath;
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

    private static void Log(string message)
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
