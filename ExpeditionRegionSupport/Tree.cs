using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport
{
    public class Tree
    {
        /// <summary>
        /// The zero-based length of the longest branch beginning at SourceNode
        /// </summary>
        public int HighestNodeDepth => GetNodesByDepth().Count();

        /// <summary>
        /// The starting point for tree data
        /// </summary>
        public ITreeNode SourceNode;

        /// <summary>
        /// Contains the next tier of branches from the source
        /// </summary>
        public IEnumerable<ITreeNode> SourceChildren => SourceNode.GetChildNodes();

        public Tree(ITreeNode sourceNode)
        {
            SourceNode = sourceNode;
        }

        /// <summary>
        /// Retrieves nodes belonging to the same node tier starting with the tree source node
        /// </summary>
        public IEnumerable<IEnumerable<ITreeNode>> GetNodesByDepth()
        {
            return GetNodesByDepth(SourceNode);
        }

        /// <summary>
        /// Retrieves nodes belonging to the same node tier starting with the given node
        /// </summary>
        public static IEnumerable<IEnumerable<ITreeNode>> GetNodesByDepth(ITreeNode node)
        {
            IEnumerable<ITreeNode> currentLevel = new ITreeNode[]
            {
                node
            };

            while (currentLevel.Any()) //With each iteration we return the next tier of child nodes
            {
                yield return currentLevel;
                currentLevel = currentLevel.SelectMany(node => node.GetChildNodes()); //Stores all child nodes of current nodes into a single IEnumerable 
            }
        }

        public void LogData()
        {
            int nodeDepth = 0;
            foreach (IEnumerable<ITreeNode> nodesAtThisLevel in GetNodesByDepth())
            {
                Plugin.Logger.LogInfo($"--- NODE DEPTH {nodeDepth} ---");
                Plugin.Logger.LogInfo(nodesAtThisLevel.FormatToString(',')); //All nodes at this depth separated by commas
                nodeDepth++;
            }
        }
    }

    public interface ITreeNode
    {
        public IEnumerable<ITreeNode> GetChildNodes();
    }
}
