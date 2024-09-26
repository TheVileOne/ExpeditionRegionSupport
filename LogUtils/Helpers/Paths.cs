using System.IO;
using UnityEngine;

namespace LogUtils.Helpers
{
    public static class Paths
    {
        public static string BepInExRootPath => BepInEx.Paths.BepInExRootPath;

        public static string GameRootPath => Path.GetDirectoryName(Application.dataPath);

        public static string StreamingAssetsPath => Application.streamingAssetsPath;
    }
}
