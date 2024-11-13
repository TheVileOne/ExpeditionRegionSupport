using LogUtils.Helpers;
using LogUtils.Helpers.Comparers;
using LogUtils.Helpers.FileHandling;
using System.IO;

namespace LogUtils
{
    public static class RainWorldDirectory
    {
        /// <summary>
        /// Hardcoded tree of directory names associated with a vanilla Rain world installation 
        /// </summary>
        public static Tree<string> FolderTree;

        /// <summary>
        /// Shortcut accessor for StreamingAssets path
        /// </summary>
        private static Tree<string>.TreeNode customRoot;

        public static void Initialize()
        {
            FolderTree = new Tree<string>();

            //Rain World
            #region Root
            var rootNode = FolderTree.AddNode(Path.GetFileName(Paths.GameRootPath));

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

        /// <summary>
        /// Evaluates a directory path, determining whether it belongs to the game, mod sourced, or unknown, and returns the result
        /// </summary>
        public static PathCategory GetDirectoryCategory(string path)
        {
            path = PathUtils.PathWithoutFilename(path);

            //The path may already be resolved - search for it in the directory tree
            var pathNode = FindPositionInTree(path);

            PathCategory category = getDirectoryCategory(pathNode, path);

            if (category != PathCategory.NotRooted)
                return category;

            //Path could still be a partial path that needs to be resolved
            if (!RWInfo.MergeProcessComplete) //Too early to resolve paths
            {
                UtilityLogger.LogWarning("Path category could not be accurately determined");
                return category;
            }

            ResolveResults results = PathResolution.ResolveDirectory(path);

            if (results.ModOwner != null)
                return PathCategory.ModFolderPlugin;

            if (!results.Exists)
                return PathCategory.NotRooted; //Don't consider the resolved path as the true path, treat it as a foreign path

            //Check if directory is game-installed - Path result is guaranteed to be within StreamingAssets
            return category = getDirectoryCategory(customRoot, results.CombinedResult);
        }

        /// <summary>
        /// Finds the nearest common directory within the Rain World directory to a provided path
        /// </summary>
        internal static Tree<string>.TreeNode FindPositionInTree(string path)
        {
            if (!PathUtils.HaveSameRoot(path, Paths.GameRootPath, true))
            {
                UtilityLogger.Log("Path not part of Rain World directory");
                return null;
            }

            //Guaranteed to not have filenames at this stage
            string[] directories = PathUtils.Separate(Path.GetFullPath(path));

            //Case sensitivity should never be an issue here?
            int rootIndex = directories.IndexOf("Rain World");

            var currentNode = FolderTree.Root;
            int currentNodeIndex = rootIndex + 1;

            //Keep checking directories until we cannot find a match, or run out of directories strings
            while (currentNodeIndex < directories.Length)
            {
                //Check the child subdirectories for the current directory string in our path
                var foundNode = currentNode.FindChild(directories[currentNodeIndex]);

                if (foundNode != null)
                {
                    currentNodeIndex++;
                    currentNode = foundNode;
                }
            }
            return currentNode; //This will represent the nearest common directory pertaining to the provided path
        }

        private static PathCategory getDirectoryCategory(Tree<string>.TreeNode node, string path)
        {
            if (node == null || path == null)
                return PathCategory.NotRooted;

            //TODO: This is wrong. The common path portion needs to be stripped from path, and the first directory after needs to be compared, not the last
            //Get the last directory
            string pathDir = Path.GetFileName(path);

            if (ComparerUtils.StringComparerIgnoreCase.Equals(pathDir, node.Value))
                return PathCategory.Game;

            //These directories are generally unsupported by the utility, and a nearest common directory match  on any of these
            //will be assumed to be game-related
            string[] gameDirs =
            {
                "mergedmods",
                "mods",
                "scenes",
                "text"
            };

            if (pathDir.MatchAny(ComparerUtils.StringComparerIgnoreCase, gameDirs))
            {
                if (pathDir == "mods")
                    return PathCategory.ModFolderTopLevel;
                return PathCategory.Game;
            }
            return PathCategory.ModSourced;
        }

        static RainWorldDirectory()
        {
            Initialize();
        }
    }

    public enum PathCategory : byte
    {
        /// <summary>
        /// Path is not part of RainWorld folder
        /// </summary>
        NotRooted,
        /// <summary>
        /// Path points to a game-installed directory, or is part of mergedmods directory
        /// </summary>
        Game,
        /// <summary>
        /// Path points to a sub-folder inside a top-level directory inside the mods directory
        /// </summary>
        ModFolderPlugin,
        /// <summary>
        /// Path points to the mods directory, or a top-level directory inside the mods directory
        /// </summary>
        ModFolderTopLevel,
        /// <summary>
        /// Path is part of the RainWorld folder, is not game-installed, part of mergedmods directory, or contained within mods directory
        /// </summary>
        ModSourced
    }
}
