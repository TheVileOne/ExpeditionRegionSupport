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
            public static string RootPath => BepInExPath.GameRootPath;
            public static string StreamingAssetsPath => Application.streamingAssetsPath;
        }
    }
}
