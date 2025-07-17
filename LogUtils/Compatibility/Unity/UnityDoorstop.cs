using BepInEx.MultiFolderLoader;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityPath = LogUtils.Helpers.Paths.Unity;

namespace LogUtils.Compatibility.Unity
{
    internal static class UnityDoorstop
    {
        public static Dictionary<string, GhettoIni.Section> ReadIni()
        {
            return GhettoIni.Read(UnityPath.DoorstopIniPath);
        }

        private static string getWhitelistPath()
        {
            Dictionary<string, GhettoIni.Section> doorstopSections = ReadIni();

            string allowListPath = null;
            bool hasAllowList = doorstopSections.TryGetValue("MultiFolderLoader", out GhettoIni.Section section)
                              && section.Entries.TryGetValue("whiteListPath", out allowListPath)
                              && File.Exists(allowListPath);

            if (!hasAllowList)
                throw new FileNotFoundException(); //Expected to be handled by the caller

            return allowListPath;
        }

        /// <summary>
        /// Adds a filename entry to whitelist.txt
        /// </summary>
        /// <exception cref="FileNotFoundException"></exception>
        public static void AddToWhitelist(string filename)
        {
            string allowListPath = getWhitelistPath();
            string allowListEntry = filename.ToLower(); //Lowercase to be consistent with other entries in this txt file

            string[] lines = File.ReadAllLines(allowListPath);

            if (lines.Contains(allowListEntry))
                return;

            using (StreamWriter writer = File.AppendText(allowListPath))
            {
                writer.WriteLine(allowListEntry);
            }
        }

        /// <summary>
        /// Removes a filename entry to whitelist.txt
        /// </summary>
        /// <exception cref="FileNotFoundException"></exception>
        public static void RemoveFromWhitelist(string filename)
        {
            string allowListPath = getWhitelistPath();
            string allowListEntry = filename.ToLower();

            string[] lines = File.ReadAllLines(allowListPath);

            using (StreamWriter writer = File.CreateText(allowListPath))
            {
                foreach (string line in lines)
                {
                    if (line != allowListEntry) //Write all lines except the line that identifies the patcher
                        writer.WriteLine(line);
                }
            }
        }
    }
}
