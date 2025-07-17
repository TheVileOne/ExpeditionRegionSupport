using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
