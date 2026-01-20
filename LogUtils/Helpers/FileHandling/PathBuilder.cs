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
                StartBuild();

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

        /// <summary>
        /// Initializes iterator state
        /// </summary>
        protected void StartBuild()
        {
            Reset();
            iterator = new PathIterator(this);

            if (IncludeRoot && info.HasPath)
                iterator.AppendValue(info.GetRoot());
        }

        internal class PathIterator : IPathBuilderNode
        {
            private readonly IEnumerator<string> directoryEnumerator;
            private readonly PathBuilder parent;
            private readonly StringBuilder result = new StringBuilder();

            public PathIterator(PathBuilder parent)
            {
                this.parent = parent;
                this.directoryEnumerator = parent.info.GetDirectoryEnumerator();
            }

            object IEnumerator.Current => Current;

            public IPathBuilderNode Current => this;

            public string Value => directoryEnumerator.Current;

            public void Accept()
            {
                result.Append(Path.DirectorySeparatorChar)
                      .Append(directoryEnumerator.Current);
            }

            internal void AppendValue(string value)
            {
                result.Append(value);
            }

            public string GetResult()
            {
                if (parent.IncludeFilenameInResult && parent.info.HasFilename)
                {
                    string resultFilename = parent.info.Target.Name; 
                    result.Append(Path.DirectorySeparatorChar)
                          .Append(resultFilename);
                }
                return result.ToString();
            }

            public bool MoveNext()
            {
                return directoryEnumerator.MoveNext();
            }

            public void Dispose()
            {
                result.Clear();
                directoryEnumerator.Dispose();
            }

            public void Reset()
            {
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
    }
}
