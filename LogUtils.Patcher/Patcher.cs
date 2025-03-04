using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using System.Collections.Generic;
using System;

namespace LogUtils.Patcher
{
    public static class Patcher
    {
        private static readonly string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "patcher.log");

        private static readonly string pluginRootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");

        public static IEnumerable<string> TargetDLLs => GetDLLs();

        public static IEnumerable<string> GetDLLs()
        {
            File.Delete("patcher.log");

            foreach (var dll in GetLogUtilsDLLsFromPlugins(pluginRootPath))
            {
                yield return dll;
            }

            yield return "Assembly-CSharp.dll";
        }

        public static IEnumerable<string> GetLogUtilsDLLsFromPlugins(string rootDirectory)
        {
            if (!Directory.Exists(rootDirectory))
            {
                Log("Plugin root directory not found: " + rootDirectory);
                yield break;
            }

            var dllFiles = Directory.EnumerateFiles(rootDirectory, "LogUtils*.dll", SearchOption.AllDirectories);
            if (!dllFiles.Any())
            {
                Log("No LogUtils DLLs found in plugin folders under " + rootDirectory);
            }

            string latestDll = null;
            Version latestVersion = new Version(0, 0, 0, 0);

            foreach (var file in dllFiles)
            {
                try
                {
                    Version version = GetDllVersion(file);
                    if (version > latestVersion)
                    {
                        latestVersion = version;
                        latestDll = file;
                    }
                }
                catch (Exception ex)
                {
                    Log($"Error reading {file}: {ex}");
                }
            }

            if (!string.IsNullOrEmpty(latestDll))
            {
                Log("Latest LogUtils DLL determined: " + latestDll);
                yield return latestDll;
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

        public static string GetLatestFrameworkDll(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Log("Directory not found: " + directoryPath);
                return null;
            }

            var files = Directory.GetFiles(directoryPath, "LogUtils*.dll");
            if (files.Length == 0)
            {
                Log("No framework DLLs found in " + directoryPath);
                return null;
            }

            string latestDll = null;
            Version latestVersion = new Version(0, 0, 0, 0);

            foreach (var file in files)
            {
                try
                {
                    Version version = GetDllVersion(file);
                    if (version > latestVersion)
                    {
                        latestVersion = version;
                        latestDll = file;
                    }
                }
                catch (Exception ex)
                {
                    Log($"Error reading {file}: {ex}");
                    throw new Exception($"Error reading {file}: {ex}");
                }
            }

            Log("Latest framework DLL determined: " + latestDll);
            return latestDll;
        }

        private static Version GetDllVersion(string dllPath)
        {
            var assemblyName = AssemblyName.GetAssemblyName(dllPath);
            return assemblyName.Version;
        }

        private static void Log(string message)
        {
            try
            {
                string logMessage = $"{System.DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{System.Environment.NewLine}";
                File.AppendAllText(logFilePath, logMessage);
            }
            catch (Exception ex)
            {
                throw new Exception("Error writing to log file. " + ex);
            }
        }
    }
}