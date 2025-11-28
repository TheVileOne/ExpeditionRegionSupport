using LogUtils.Helpers.Comparers;
using System;
using System.Collections.Generic;
using System.IO;

namespace LogUtils.Helpers.FileHandling
{
    public class DirectoryTree
    {
        public readonly DirectoryTreeNode Root;

        public readonly string RootPath;

        /// <summary>
        /// Construct a tree data structure using a directory path as a root
        /// </summary>
        public DirectoryTree(string rootPath)
        {
            Root = new DirectoryTreeNode(this, Path.GetFileName(rootPath));
            RootPath = Path.GetFullPath(rootPath).TrimEnd(Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Finds the nearest common directory within the root directory to a provided path
        /// </summary>
        public DirectoryTreeNode FindPositionInTree(string path)
        {
            string fullPath = PathUtils.ResolvePath(path); //Ensure that we are comparing two normalized full paths
            string commonRoot = PathUtils.FindCommonRoot(fullPath, RootPath);

            if (commonRoot.Length < RootPath.Length) //Path isn't part of tree
                return null;

            //Get the part of the path we are interested in relative to the root directory
            string adjustedPath = fullPath.Substring(Math.Min(commonRoot.Length + 1, fullPath.Length)); //Add one to account for separator

            return Root.FindMostRelativeNode(adjustedPath);
        }

        public class DirectoryTreeNode
        {
            protected DirectoryTree Source;
            protected DirectoryTreeNode Parent;
            protected List<DirectoryTreeNode> Children;

            public string DirName;

            public string DirPath
            {
                get
                {
                    //Move up through parent nodes until we reach the root
                    if (Parent != null)
                        return Path.Combine(Parent.DirPath, DirName);

                    //Root shares the dir name for this node
                    if (Source != null)
                        return Source.RootPath;

                    return Path.GetFullPath(DirName);
                }
            }

            protected bool IsRoot => Source != null && Source.Root == this;

            public DirectoryTreeNode(DirectoryTree source, string dirName)
            {
                DirName = dirName;
                Source = source;
                Children = new List<DirectoryTreeNode>();
            }

            public DirectoryTreeNode AddNode(string childDirName)
            {
                DirectoryTreeNode child = new DirectoryTreeNode(Source, childDirName)
                {
                    Parent = this
                };
                Children.Add(child);
                return child;
            }

            public void Attach(DirectoryTreeNode node)
            {
                if (node.Parent == this || node.IsRoot) return;

                node.Parent = this;
                Children.Add(node);

                node.ValidateSource(); //We need to inherit a new source for this node
            }

            /// <summary>
            /// Detach node from the parent node
            /// </summary>
            public void Detach()
            {
                //The root node cannot be separated from the directory tree
                if (IsRoot) return;

                if (Parent != null)
                    Parent.Children.Remove(this);

                //Detaching from parent severs connection with root
                InvalidateSource();
            }

            internal void ValidateSource()
            {
                Source = Parent.Source;
                Children.ForEach(c => c.ValidateSource());
            }

            internal void InvalidateSource()
            {
                Source = null;
                Children.ForEach(c => c.InvalidateSource());
            }

            /// <summary>
            /// Find an immediate child node with a given value, otherwise null
            /// </summary>
            public DirectoryTreeNode FindChild(string childDirName)
            {
                UtilityLogger.DebugLog("Finding child");
                UtilityLogger.DebugLog(childDirName);
                return Children.Find(child => ComparerUtils.StringComparerIgnoreCase.Equals(child.DirName, childDirName));
            }

            internal DirectoryTreeNode FindMostRelativeNode(string path)
            {
                UtilityLogger.DebugLog("Finding most relevant node");
                UtilityLogger.DebugLog(path);
                int dirIndex = path.IndexOf(Path.DirectorySeparatorChar);

                if (dirIndex == -1)
                    dirIndex = path.Length;

                string dirNameToCheck = path.Substring(0, dirIndex);

                DirectoryTreeNode child = FindChild(dirNameToCheck);

                if (child != null)
                {
                    UtilityLogger.DebugLog("Child found");
                    if (dirIndex < path.Length)
                    {
                        path = path.Substring(dirIndex + 1); //Add one to account for separator
                        return child.FindMostRelativeNode(path);
                    }
                    return child;
                }
                return this;
            }
        }
    }
}
