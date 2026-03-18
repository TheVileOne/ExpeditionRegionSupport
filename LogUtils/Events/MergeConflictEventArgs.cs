using LogUtils.Enums;
using System;

namespace LogUtils.Events
{
    /// <summary>
    /// Event arguments for a merge conflict event
    /// </summary>
    public sealed class MergeConflictEventArgs : EventArgs
    {
        /// <summary>
        /// Affected log group by this event
        /// </summary>
        public LogGroupID Target { get; }

        /// <summary>
        /// Make a request to cancel the merge process
        /// </summary>
        public bool CancelMerge { get; set; }

        /// <summary>
        /// Affects whether path is updated immediately or after merge process completes (after post mods)
        /// </summary>
        public PathUpdateMode ShouldUpdatePath = PathUpdateMode.UpdateImmediately;

        private readonly MergeEventHandler _eventHandler;
        /// <summary>
        /// Optional action that must run if merge process doesn't complete
        /// </summary>
        public event Action OnMergeCanceled
        {
            add => _eventHandler.OnCancel += value;
            remove => _eventHandler.OnCancel -= value;
        }

        /// <summary>
        /// Optional action that must run when conflicts involving this group is resolved
        /// </summary>
        public event Action OnConflictResolved
        {
            add => _eventHandler.OnCompleted += value;
            remove => _eventHandler.OnCompleted -= value;
        }

        public MergeConflictEventArgs(LogGroupID target, MergeEventHandler handler)
        {
            _eventHandler = handler;
            Target = target;
        }
    }

    public enum PathUpdateMode
    {
        UpdateImmediately = 0,
        WaitForConflictResolution = 1,
    }
}
