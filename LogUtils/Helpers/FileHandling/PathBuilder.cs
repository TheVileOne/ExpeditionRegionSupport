using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LogUtils.Helpers.FileHandling
{
    /// <summary>
    /// Type makes it easier to construct new paths 
    /// </summary>
    public class PathBuilder : IPathBuilder
    {
        private readonly PathInfo info;
        private PathIterator iterator;

        /// <summary>
        /// Indicates whether root should be included in path result
        /// </summary>
        public bool IncludeRoot;

        /// <summary>
        /// Set to <see langword="true"/> when a filename (if present) should be appended before getting path result
        /// </summary>
        public bool IncludeFilenameInResult;

        public PathBuilder(PathInfo info)
        {
            this.info = info;
        }

        IPathBuilderNode IEnumerator<IPathBuilderNode>.Current
        {
            get
            {
                if (iterator == null)
                    throw new InvalidOperationException("Enumeration has not yet started.");
                return iterator.Current;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                if (iterator == null)
                    throw new InvalidOperationException("Enumeration has not yet started.");
                return iterator.Current;
            }
        }

        /// <inheritdoc/>
        public string GetResult()
        {
            if (iterator == null)
                throw new InvalidOperationException("Enumeration has not yet started.");
            return iterator.GetResult();
        }

        /// <inheritdoc/>
        public bool MoveNext()
        {
            if (iterator == null)
                iterator = new PathIterator(this);

            return iterator.MoveNext();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            iterator?.Dispose();
            iterator = null;
        }

        /// <inheritdoc/>
        public void Reset()
        {
            try
            {
                iterator?.Reset();
            }
            catch //Though the iterator could not be reset directly, disposing will create a new iterator for the next enumeration
            {
                Dispose();
            }
        }

        private class PathIterator : IPathBuilder
        {
            private readonly PathBuilder builder;

            private PathNode node;
            private PathNodeState nodeState = PathNodeState.NotStarted;

            /// <summary>
            /// Increments when iterator gets reset
            /// </summary>
            private int version = 0;

            public IPathBuilderNode Current => node;

            object IEnumerator.Current => node;

            public PathIterator(PathBuilder builder)
            {
                this.builder = builder;
                node = new PathNode(this);
            }

            public string GetResult()
            {
                return node.GetResult();
            }

            public void Dispose()
            {
                if (nodeState == PathNodeState.Disposed) return;

                nodeState = PathNodeState.Disposed; //This needs to be set before node is disposed
                node.Dispose();

                if (builder.iterator == this) //Dispose iterator at the builder level
                    builder.Dispose();
            }

            public bool MoveNext()
            {
                PathNode nextNode = node;
                if (nodeState != PathNodeState.NotStarted || version > 0)
                {
                    //Each time we need to advance the enumeration, we clone a new node to avoid overwriting the old node's state.
                    nextNode = node.Clone();
                }

                try
                {
                    if (nodeState != PathNodeState.InProgress) //First iteration
                    {
                        node = null; //This hack avoids an equality check that would otherwise trigger an infinite loop
                        nodeState = PathNodeState.InProgress;
                        return nextNode.MoveNextRare();
                    }
                    return nextNode.MoveNext();
                }
                finally
                {
                    node = nextNode;
                }
            }

            private bool isResetting; 
            public void Reset()
            {
                if (nodeState == PathNodeState.NotStarted) return;

                //We want to reset under the error handling protections of the builder class which would not be applied when invoked from a node
                if (!isResetting && builder.iterator == this)
                {
                    isResetting = true;
                    builder.Reset();
                    return;
                }

                version++;
                isResetting = false;
                nodeState = PathNodeState.NotStarted; //This needs to be set before node is disposed
                node.Reset();
            }

            protected class PathNode : IPathBuilderNode
            {
                private readonly PathIterator iterator;

                private readonly IEnumerator<string> directoryEnumerator;
                private readonly StringBuilder result = new StringBuilder();

                object IEnumerator.Current => iterator.node;

                public IPathBuilderNode Current => iterator.node;

                private string _value;
                public string Value => _value;

                public PathNode(PathIterator iterator)
                {
                    this.iterator = iterator;
                    directoryEnumerator = iterator.builder.info.GetDirectoryEnumerator();
                }

                public void Accept()
                {
                    result.Append(Value)
                          .Append(Path.DirectorySeparatorChar);
                }

                internal void AppendValue(string value)
                {
                    result.Append(value);
                }

                /// <summary>
                /// Gets the path result. Directory paths will contain a trailing separator char.
                /// </summary>
                public string GetResult()
                {
                    if (iterator.builder.IncludeFilenameInResult)
                        return GetResultWithFilename();
                    return result.ToString();
                }

                internal string GetResultWithFilename()
                {
                    var buildInfo = iterator.builder.info;
                    if (buildInfo.HasFilename)
                    {
                        string resultFilename = buildInfo.Target.Name;
                        return result.ToString() + resultFilename;
                    }
                    return result.ToString();
                }

                public bool MoveNext()
                {
                    if (Current == this)
                    {
                        //This ensures that we are operating on a fresh node each time MoveNext() is invoked
                        return iterator.MoveNext();
                    }

                    //We must be working with the new node
                    if (directoryEnumerator.MoveNext())
                    {
                        _value = directoryEnumerator.Current;
                        return true;
                    }
                    return false;
                }

                internal bool MoveNextRare()
                {
                    if (iterator.builder.IncludeRoot)
                    {
                        var buildInfo = iterator.builder.info;
                        if (buildInfo.HasFilename)
                            AppendValue(buildInfo.GetRoot());
                    }
                    return MoveNext();
                }

                public PathNode Clone() => (PathNode)MemberwiseClone();

                public void Dispose()
                {
                    if (iterator.nodeState == PathNodeState.Disposed) return;

                    //Inform the iterator of a dispose request
                    iterator.Dispose();

                    result.Clear();
                    directoryEnumerator.Dispose();
                }

                public void Reset()
                {
                    if (iterator.nodeState == PathNodeState.NotStarted) return;

                    try
                    {
                        directoryEnumerator.Reset();
                        result.Clear(); //Failing to reset will not change the enumeration position. Let the result state be unaffected too.
                    }
                    catch (Exception ex)
                    {
                        throw new NotSupportedException("Reset operation is not supported by this enumerator", ex);
                    }
                }
            }

            private enum PathNodeState
            {
                NotStarted = -1,
                InProgress = -2,
                Disposed = -3,
            }
        }
    }

    public interface IPathBuilder : IEnumerator<IPathBuilderNode>
    {
        /// <summary>
        /// Returns the compiled path string containing all selected values
        /// </summary>
        string GetResult();
    }

    public interface IPathBuilderNode : IPathBuilder
    {
        /// <summary>
        /// Name of a directory in a specific place in the path string
        /// </summary>
        string Value { get; }

        /// <summary>
        /// Signals that path segment should be added to path string (only invoke once per node)
        /// </summary>
        void Accept();
    }

    public static class PathBuilderExtensions
    {
        public static IPathBuilderNode Skip(this IPathBuilder node, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            while (node.MoveNext() && count > 0)
            {
                node = node.Current;
                count--;
            }
            return node.Current;
        }

        public static IPathBuilderNode Take(this IPathBuilder node, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            while (node.MoveNext() && count > 0)
            {
                node.Current.Accept();
                node = node.Current;
                count--;
            }
            return node.Current;
        }

        public static IPathBuilderNode TakeLast(this IPathBuilder node, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (count == 0)
            {
                while (node.MoveNext())
                {
                    node = node.Current;
                    continue;
                }
                return node.Current;
            }

            List<IPathBuilderNode> selectedNodes = new List<IPathBuilderNode>(count);
            while (node.MoveNext())
            {
                if (selectedNodes.Count == count) //When at capacity remove the earliest node
                    selectedNodes.RemoveAt(0);

                selectedNodes.Add(node.Current);
                node = node.Current;
            }

            selectedNodes.ForEach(node => node.Accept());
            return node.Current;
        }
    }
}
