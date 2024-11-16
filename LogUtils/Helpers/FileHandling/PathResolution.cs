using System;
using System.IO;

namespace LogUtils.Helpers.FileHandling
{
    /// <summary>
    /// Performs helper functions similar to AssetManager path resolution
    /// </summary>
    public static class PathResolution
    {
        public static ResolveResults ResolveDirectory(string path)
        {
            return ResolvePath(path, Directory.Exists);
        }

        public static ResolveResults ResolveFilePath(string path)
        {
            return ResolvePath(path, File.Exists);
        }

        internal static ResolveResults ResolvePath(string path, Func<string, bool> existCallback)
        {
            if (!RWInfo.MergeProcessComplete)
                UtilityLogger.LogWarning("Resolving path before mod merging has completed");

            if (string.IsNullOrWhiteSpace(path))
            {
                return new ResolveResults()
                {
                    Exists = false,
                    Result = Paths.StreamingAssetsPath
                };
            }

            //Make sure path is in a consistent comparable format
            path = path.Normalize().ToLowerInvariant();

            string resolvePath = Path.Combine(Paths.StreamingAssetsPath, "mergedmods");

            if (existCallback(Path.Combine(resolvePath, path)))
            {
                return new ResolveResults()
                {
                    Exists = true,
                    Original = path,
                    Result = resolvePath
                };
            }

            for (int i = ModManager.ActiveMods.Count - 1; i >= 0; i--)
            {
                ModManager.Mod mod = ModManager.ActiveMods[i];

                if (mod.hasTargetedVersionFolder)
                {
                    resolvePath = mod.TargetedPath;
                    if (existCallback(Path.Combine(resolvePath, path)))
                    {
                        return new ResolveResults()
                        {
                            Exists = true,
                            ModOwner = mod,
                            Original = path,
                            Result = resolvePath
                        };
                    }
                }
                if (mod.hasNewestFolder)
                {
                    resolvePath = mod.NewestPath;
                    if (existCallback(Path.Combine(resolvePath, path)))
                    {
                        return new ResolveResults()
                        {
                            Exists = true,
                            ModOwner = mod,
                            Original = path,
                            Result = resolvePath
                        };
                    }
                }
                resolvePath = mod.path;
                if (existCallback(Path.Combine(resolvePath, path)))
                {
                    return new ResolveResults()
                    {
                        Exists = true,
                        ModOwner = mod,
                        Original = path,
                        Result = resolvePath
                    };
                }
            }
            return new ResolveResults()
            {
                Exists = existCallback(Path.Combine(Paths.StreamingAssetsPath, path)),
                Original = path,
                Result = Paths.StreamingAssetsPath
            };
        }
    }
    public struct ResolveResults
    {
        /// <summary>
        /// Does the resolved path exist, or does the result represent a non-existant fallback path
        /// </summary>
        public bool Exists;

        /// <summary>
        /// Contains the associated mod instance if it exists
        /// </summary>
        public ModManager.Mod ModOwner;

        /// <summary>
        /// The original path
        /// </summary>
        public string Original;

        /// <summary>
        /// The portion of the path that was resolved
        /// </summary>
        public string Result;

        /// <summary>
        /// Gets the complete resolved path
        /// </summary>
        public readonly string CombinedResult => Path.Combine(Original, Result);
    }
}
