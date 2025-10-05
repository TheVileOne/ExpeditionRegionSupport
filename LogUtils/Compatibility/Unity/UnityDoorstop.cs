using BepInEx.MultiFolderLoader;
using System;
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
            bool hasAllowList = false;

            if (doorstopSections.TryGetValue("MultiFolderLoader", out GhettoIni.Section section)
              && section.Entries.TryGetValue("whiteListPath", out allowListPath))
            {
                allowListPath = Path.Combine(allowListPath, "whitelist.txt");
                hasAllowList = File.Exists(allowListPath);
            }

            if (!hasAllowList)
                throw new FileNotFoundException(); //Expected to be handled by the caller

            return allowListPath;
        }

        /// <summary>
        /// Adds a filename entry to whitelist.txt
        /// </summary>
        /// <exception cref="FileNotFoundException">whitelist.txt does not exist</exception>
        public static void AddToWhitelist(string entry)
        {
            UtilityLogger.Log("Updating whitelist.txt");
            UtilityLogger.Log("Target: " + entry);

            string allowListPath = getWhitelistPath();
            string[] lines = File.ReadAllLines(allowListPath);

            if (lines.Contains(entry, StringComparer.OrdinalIgnoreCase))
            {
                UtilityLogger.Log("Entry found");
                return;
            }

            UtilityLogger.Log("Adding patcher entry");
            using (StreamWriter writer = File.AppendText(allowListPath))
            {
                writer.WriteLine(entry.ToLower()); //Lowercase to be consistent with other entries in this txt file
            }
        }

        /// <summary>
        /// Removes a filename entry from whitelist.txt
        /// </summary>
        /// <remarks>Input is case insensitive</remarks>
        /// <exception cref="FileNotFoundException">whitelist.txt does not exist</exception>
        public static void RemoveFromWhitelist(string entry)
        {
            UtilityLogger.Log("Updating whitelist.txt");
            UtilityLogger.Log("Target: " + entry);

            string allowListPath = getWhitelistPath();
            string[] lines = File.ReadAllLines(allowListPath);

            using (StreamWriter writer = File.CreateText(allowListPath))
            {
                int entryCount = 0;
                foreach (string line in lines)
                {
                    if (string.Equals(line, entry, StringComparison.OrdinalIgnoreCase))
                    {
                        UtilityLogger.Log("Entry found");
                        continue;
                    }

                    //Write all lines except the line that identifies the patcher
                    entryCount++;
                    writer.WriteLine(line);
                }

                if (entryCount != lines.Length)
                    UtilityLogger.Log("Patcher entry removed");
            }
        }
    }
}
