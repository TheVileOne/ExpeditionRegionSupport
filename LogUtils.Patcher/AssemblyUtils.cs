using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AssemblyCandidate = (System.Version Version, string Path);

namespace LogUtils.Patcher;

internal static class AssemblyUtils
{
    public static AssemblyCandidate LastFoundAssembly;

    /// <summary>
    /// Searches the specified directory path (and any subdirectories) for an assembly, and returns the first match
    /// </summary>
    public static string FindAssembly(string searchPath, string assemblyName)
    {
        return Directory.EnumerateFiles(searchPath, assemblyName, SearchOption.TopDirectoryOnly).FirstOrDefault();
    }

    public static AssemblyCandidate FindLatestAssembly(IEnumerable<string> searchTargets, string assemblyName)
    {
        AssemblyCandidate target = default;
        foreach (string searchPath in searchTargets)
        {
            try
            {
                string targetPath = FindAssembly(searchPath, assemblyName);

                if (targetPath != null)
                {
                    var version = AssemblyName.GetAssemblyName(targetPath).Version;
                    Patcher.Logger.LogInfo($"Found candidate {targetPath} v{version}");

                    LastFoundAssembly = new AssemblyCandidate(version, targetPath);

                    if (target.Path == null || target.Version < version)
                        target = LastFoundAssembly;
                }
            }
            catch (IOException ex)
            {
                Patcher.Logger.LogError($"Error trying to access {searchPath}: {ex.Message}");
            }
            catch (Exception ex)
            {
                Patcher.Logger.LogError($"Error reading version from {searchPath}: {ex.Message}");
            }
        }
        return target;
    }

    public static bool HasPath(this AssemblyCandidate candidate, string path)
    {
        if (candidate.Path == null)
            return false;
        return candidate.Path.StartsWith(path);
    }
}
