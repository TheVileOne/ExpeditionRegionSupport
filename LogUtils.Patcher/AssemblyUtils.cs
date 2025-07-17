using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AssemblyCandidate = (System.Version Version, string Path);

namespace LogUtils.Patcher
{
    internal static class AssemblyUtils
    {
        /// <summary>
        /// Searches the specified directory path (and any subdirectories) for an assembly, and returns the first match
        /// </summary>
        public static string FindAssembly(string searchPath, string assemblyName)
        {
            return Directory.EnumerateFiles(searchPath, assemblyName, SearchOption.AllDirectories).FirstOrDefault();
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
                        Patcher.Log($"Found candidate {targetPath} v{version}");

                        if (target.Path == null || target.Version < version)
                            target = new AssemblyCandidate(version, targetPath);
                    }
                }
                catch (IOException ex)
                {
                    Patcher.Log($"Error trying to access {searchPath}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Patcher.Log($"Error reading version from {searchPath}: {ex.Message}");
                }
            }
            return target;
        }
    }
}
