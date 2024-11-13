using LogUtils.Helpers.Comparers;
using System.Collections.Generic;

namespace LogUtils.Helpers
{
    public class Tree<T>
    {
        public TreeNode Root { get; protected set; }

        public Tree()
        {
        }

        public TreeNode AddNode(T value)
        {
            Root?.Detach(true);
            Root = new TreeNode(this, value);
            return Root;
        }

        public class TreeNode
        {
            protected Tree<T> Source;
            protected TreeNode Parent;
            protected List<TreeNode> Children;

            public T Value;

            public TreeNode(Tree<T> source, T value)
            {
                Value = value;
                Children = new List<TreeNode>();
            }

            public TreeNode AddNode(T value)
            {
                TreeNode child = new TreeNode(Source, value)
                {
                    Parent = this
                };
                Children.Add(child);
                return child;
            }

            /// <summary>
            /// Detach node from the source, and optionally the parent node
            /// </summary>
            public void Detach(bool keepParent)
            {
                if (Source?.Root == this)
                    Source.Root = null;
                Source = null;

                if (!keepParent)
                {
                    Parent?.Children?.Remove(this);
                    Parent = null;
                }

                //keepParent applies to the topmost node, not its children
                Children.ForEach(c => c.Detach(false));
            }

            /// <summary>
            /// Find an immediate child node with a given value, otherwise null
            /// </summary>
            public TreeNode FindChild(T value)
            {
                return Children.Find(child => ComparerUtils.StringComparerIgnoreCase.Equals(child.Value, value));
            }
        }
    }
}
