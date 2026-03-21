using System;
using System.IO;

namespace LogUtils
{
    internal struct MergeFolderState
    {
        /// <summary>
        /// The number of folder levels the current source directory is from the topmost source directory
        /// </summary>
        public short FolderDepth { get; private set; }

        private DirectoryInfo _currentSource;
        /// <summary>
        /// The current directory being merged into a new directory
        /// </summary>
        public DirectoryInfo CurrentSource
        {
            get => _currentSource;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                if (_currentSource != null) //First update should be version 0
                    FolderDepth++;

                _currentSource = value;
            }
        }

        /// <summary>
        /// The specified path to merge into
        /// </summary>
        public string DestinationPath;

        /// <summary>
        /// Contains event handlers pertaining to the merge process
        /// </summary>
        public MergeEventHandler Events;

        /// <summary>
        /// The complete history of file system, LogID, and LogGroupID changes since the merge began
        /// </summary>
        public MergeHistory History;

        public MergeFolderState()
        {
            History = new MergeHistory();
            Events = new MergeEventHandler();
        }

        public readonly void ProcessConflicts()
        {
            if (History.Conflicts.Count == 0) return;

            MergeHistory history = History;
            MergeEventHandler events = Events;
            ConflictResolutionDialog.ShowDialog(history, events);
        }
    }

    public class MergeEventHandler
    {
        public event Action OnCancel;
        public event Action OnCompleted;
    }
}
