using System.IO;
using UnityEngine;
using BepInExPath = BepInEx.Paths;

namespace LogUtils.Helpers
{
    internal static class Paths
    {
        internal static class BepInEx
        {
            public static string RootPath => BepInExPath.BepInExRootPath;
        }

        internal static class RainWorld
        {
            public static string RootPath => Path.GetDirectoryName(Application.dataPath);
            public static string StreamingAssetsPath => Application.streamingAssetsPath;
        }
    }
}
