using LogUtils.Helpers.Comparers;
using LogUtils.Helpers.FileHandling;
using System.IO;
using RainWorldPath = LogUtils.Helpers.Paths.RainWorld;

namespace LogUtils
{
    public static class RainWorldDirectory
    {
        /// <summary>
        /// Converts partial, and non-partial Rain World file or directory paths into a normalized equivalent form within the Rain World directory  
        /// </summary>
        public static IFileSystemPathFinder PathFinder = new RainWorldPathFinder();

        /// <summary>
        /// Hardcoded tree of directory names associated with a vanilla Rain world installation 
        /// </summary>
        public static DirectoryTree FolderTree;

        /// <summary>
        /// Shortcut accessor for StreamingAssets path
        /// </summary>
        private static DirectoryTree.DirectoryTreeNode customRoot;

        public static void Initialize()
        {
            FolderTree = new DirectoryTree(RainWorldPath.RootPath);

            //Rain World
            #region Root
            var rootNode = FolderTree.Root;

            //BepInEx
            #region BepInEx
            var bNode = rootNode.AddNode("BepInEx");

            bNode.AddNode("backup");
            bNode.AddNode("cache");
            bNode.AddNode("config");
            bNode.AddNode("core");
            bNode.AddNode("monomod");
            bNode.AddNode("patchers");
            bNode.AddNode("plugins");
            bNode.AddNode("utils");
            #endregion

            //Mono and related directories
            #region Mono
            var monoNode = rootNode.AddNode("MonoBleedingEdge");

            monoNode.AddNode("EmbedRuntime");
            monoNode = monoNode.AddNode("etc")
                               .AddNode("mono");

            monoNode.AddNode("2.0")
                    .AddNode("Browsers");

            monoNode.AddNode("4.0")
                    .AddNode("Browsers");

            monoNode.AddNode("4.5")
                    .AddNode("Browsers");

            monoNode.AddNode("mconfig");
            #endregion

            //Data
            #region Data
            var dataNode = rootNode.AddNode("RainWorld_Data");

            dataNode.AddNode("Managed");
            dataNode.AddNode("Plugins")
                    .AddNode("x86");
            dataNode.AddNode("Resources");

            //StreamingAssets
            #region CustomRoot
            var customRootNode = dataNode.AddNode("StreamingAssets");

            var miscNode = customRootNode.AddNode("aa");

            miscNode.AddNode("AddressablesLink");
            miscNode.AddNode("StandaloneWindows");

            customRootNode.AddNode("AssetBundles");
            customRootNode.AddNode("decals");
            customRootNode.AddNode("illustrations");
            customRootNode.AddNode("levels");
            customRootNode.AddNode("loadedsoundeffects")
                          .AddNode("ambient");
            customRootNode.AddNode("mergedmods"); //No folders in here are static
            customRootNode.AddNode("mods"); //No folders here either, except perhaps DLC mods

            miscNode = customRootNode.AddNode("music");

            miscNode.AddNode("procedural");
            miscNode.AddNode("songs");

            customRootNode.AddNode("palettes");
            customRootNode.AddNode("projections");
            customRootNode.AddNode("scenes"); //Assume all directories here are game-installed
            customRootNode.AddNode("shaders");
            customRootNode.AddNode("soundeffects");
            customRootNode.AddNode("text") //Not includes language folders
                          .AddNode("credits");
            customRoot = customRootNode;
            //Worlds
            #region Worlds
            var worldNode = customRootNode.AddNode("world");

            worldNode.AddNode("cc");
            worldNode.AddNode("cc-rooms");
            worldNode.AddNode("ds");
            worldNode.AddNode("ds-rooms");
            worldNode.AddNode("gate shelters");
            worldNode.AddNode("gates");
            worldNode.AddNode("gw");
            worldNode.AddNode("gw-rooms");
            worldNode.AddNode("hi");
            worldNode.AddNode("hi-rooms");
            worldNode.AddNode("indexmaps");
            worldNode.AddNode("lf");
            worldNode.AddNode("lf-rooms");
            worldNode.AddNode("sb");
            worldNode.AddNode("sb-rooms");
            worldNode.AddNode("sh");
            worldNode.AddNode("sh-rooms");
            worldNode.AddNode("si");
            worldNode.AddNode("si-rooms");
            worldNode.AddNode("sl");
            worldNode.AddNode("sl-rooms");
            worldNode.AddNode("ss");
            worldNode.AddNode("ss-rooms");
            worldNode.AddNode("su");
            worldNode.AddNode("su-rooms");
            worldNode.AddNode("uw");
            worldNode.AddNode("uw-rooms");
            #endregion
            #endregion
            #endregion
            #endregion
        }

        public static DirectoryInfo GetDirectoryInfo()
        {
            return new DirectoryInfo(RainWorldPath.RootPath);
        }

        internal static bool IsIllegalLogPath(string path)
        {
            //Mod containing directories are always invalid
            if (Array.Exists(RainWorldPath.ModContainingDirectories, dirPath => PathUtils.PathsAreEqual(path, dirPath)))
                return true;

            if (path.StartsWith(RainWorldPath.StreamingAssetsPath))
            {
                string[] illegalPaths =
                {
                    "mergedmods",
                    "scenes",
                    "text"
                };
                return Path.GetFileName(path).MatchAny(ComparerUtils.StringComparerIgnoreCase, illegalPaths);
            }
            return false;
        }

        public static string Locate(string path)
        {
            return PathFinder.FindMatch(path);
        }

        /// <summary>
        /// Evaluates a directory path, determining whether it belongs to the game, mod sourced, or unknown, and returns the result
        /// </summary>
        public static PathCategory GetDirectoryCategory(string path)
        {
            path = PathUtils.PathWithoutFilename(path);

            //The path may already be resolved - search for it in the directory tree
            var pathNode = FolderTree.FindPositionInTree(path);

            if (pathNode == null)
                UtilityLogger.Log("Path not part of Rain World directory");

            return getDirectoryCategory(pathNode, path);
        }

        private static PathCategory getDirectoryCategory(DirectoryTree.DirectoryTreeNode node, string path)
        {
            if (node == null)
                return PathCategory.NotRooted;

            path = PathUtils.ResolvePath(path);

            //The possible states include the path being a game directory, or a directory inside a game directory 
            if (PathUtils.PathsAreEqual(node.DirPath, path))
                return PathCategory.Game;

            //The path is within the mods directory - supported paths include 
            if (ComparerUtils.StringComparerIgnoreCase.Equals(node.DirName, "mods"))
            {
                if (isRequiredModDirectory(path))
                {
                    UtilityLogger.Log("Path belongs to a mod required directory");
                    return PathCategory.ModRequiredFolder;
                }
                return PathCategory.ModSourced;
            }

            //We know this path isn't directing to a tracked game directory, but it could still be an untracked one
            string[] gameDirs =
            {
                "mergedmods",
                "scenes",
                "text"
            };

            if (node.DirName.MatchAny(ComparerUtils.StringComparerIgnoreCase, gameDirs))
                return PathCategory.Game;

            //All other possibilities involve a directory not put there by the game
            return PathCategory.ModSourced;
        }

        private static bool isRequiredModDirectory(string path)
        {
            string targetDir = Path.GetFileName(path);
            string containingDir = Path.GetFileName(Path.GetDirectoryName(path));

            string[] requiredModDirs =
            {
                "modify",
                "newest",
                "plugins"
            };

            return targetDir.MatchAny(ComparerUtils.StringComparerIgnoreCase, requiredModDirs) || ComparerUtils.StringComparerIgnoreCase.Equals(containingDir, "mods");
        }

        static RainWorldDirectory()
        {
            Initialize();
        }
    }

    public enum PathCategory : byte
    {
        /// <summary>
        /// Path is not part of Rain World folder
        /// </summary>
        NotRooted,
        /// <summary>
        /// Path points to a game-installed directory, or is part of mergedmods directory
        /// </summary>
        Game,
        /// <summary>
        /// Path points to a top-level directory inside the mods directory, or similar directory for mods, or a required mod subdirectory
        /// </summary>
        ModRequiredFolder,
        /// <summary>
        /// Path is not a game-installed directory, not associated with a mod's directory structure, and is defined within the Rain World directory or a mod-specific directory
        /// </summary>
        ModSourced
    }
}
