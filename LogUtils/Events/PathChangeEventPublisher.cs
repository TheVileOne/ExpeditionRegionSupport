using LogUtils.Enums.FileSystem;

namespace LogUtils.Events
{
    public sealed class PathChangeEventPublisher
    {
        private PathChangeEventHandler _changeHandler,
                                       _pendingHandler,
                                       _completedHandler,
                                       _abortedHandler;
        /// <summary>
        /// Event raised when a path change event occurs
        /// </summary>
        public event PathChangeEventHandler Event
        {
            add => _changeHandler += value;
            remove => _changeHandler -= value;
        }

        /// <summary>
        /// Event raised when a path is about to be changed
        /// </summary>
        public event PathChangeEventHandler PendingEvent
        {
            add => _pendingHandler += value;
            remove => _pendingHandler -= value;
        }

        /// <summary>
        /// Event raised when a path change is completed
        /// </summary>
        public event PathChangeEventHandler CompletedEvent
        {
            add => _completedHandler += value;
            remove => _completedHandler -= value;
        }

        /// <summary>
        /// Event raised when a path change is canceled
        /// </summary>
        public event PathChangeEventHandler AbortedEvent
        {
            add => _abortedHandler += value;
            remove => _abortedHandler -= value;
        }

        internal void OnChangeEvent(PathChangeEventArgs eventArgs)
        {
            _changeHandler?.Invoke(eventArgs);
        }

        internal void OnPending(string newPath, string newFilename = null)
        {
            var eventArgs = new PathChangeEventArgs(ActionStatus.Pending, newPath, newFilename);

            OnChangeEvent(eventArgs);
            _pendingHandler?.Invoke(eventArgs);
        }

        internal void OnCompleted(string newPath, string newFilename = null)
        {
            var eventArgs = new PathChangeEventArgs(ActionStatus.Complete, newPath, newFilename);

            OnChangeEvent(eventArgs);
            _completedHandler?.Invoke(eventArgs);
        }

        internal void OnAbort(string newPath, string newFilename = null)
        {
            var eventArgs = new PathChangeEventArgs(ActionStatus.Aborted, newPath, newFilename);

            OnChangeEvent(eventArgs);
            _abortedHandler?.Invoke(eventArgs);
        }
    }

    public class PathChangeEventArgs
    {
        /// <summary>
        /// Describes the nature of the path change event
        /// </summary>
        public readonly ActionStatus EventType;

        /// <summary>
        /// The path targeted by the path change event
        /// </summary>
        public readonly string NewPath;

        /// <summary>
        /// The filename targeted by the path change event, or <see langword="null"/> if targeted path is not a file path
        /// </summary>
        public readonly string NewFilename;

        public PathChangeEventArgs(string newPath, string newFilename = null)
        {
            EventType = ActionStatus.None;
            NewPath = newPath;
            NewFilename = newFilename;
        }

        public PathChangeEventArgs(ActionStatus status, string newPath, string newFilename = null)
        {
            EventType = status ?? ActionStatus.None;
            NewPath = newPath;
            NewFilename = newFilename;
        }

        public bool IsRenamed => NewFilename != null;
    }
}
