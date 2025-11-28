using LogUtils.Helpers.FileHandling;
using System.IO;
using UnityEngine;
using BepInExPath = BepInEx.Paths;

namespace LogUtils.Helpers
{
    internal static class Paths
    {
        internal static class BepInEx
        {
            public static string BackupPath = Path.Combine(RootPath, "backup");
            public static string RootPath => BepInExPath.BepInExRootPath;
            public static string PatcherPath => BepInExPath.PatcherPluginPath;
        }

        internal static class RainWorld
        {
            public static readonly string RootPath = PathUtils.Normalize(BepInExPath.GameRootPath);
            public static readonly string StreamingAssetsPath = PathUtils.Normalize(Application.streamingAssetsPath);

            /// <summary>
            /// The name of the Rain World root folder
            /// </summary>
            public static readonly string ROOT_DIRECTORY = Path.GetFileName(RootPath);
        }

        internal static class Unity
        {
            public static string DoorstopIniPath = Path.Combine(RainWorld.RootPath, "doorstop_config.ini");
        }
    }
}
