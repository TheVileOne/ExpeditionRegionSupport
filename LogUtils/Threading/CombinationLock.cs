using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DotNetTask = System.Threading.Tasks.Task;

namespace LogUtils.Threading
{
    internal class CombinationLock<T> : Lock, IScopedCollection<T> where T : ILockable
    {
        public IReadOnlyCollection<T> Items { get; }

        private bool workCompleted;
        private TaskFinalizer workFinalizer;

        /// <summary>
        /// Constructs a new <see cref="Lock"/> object
        /// </summary>
        /// <param name="groupProvider">A collection of objects to acquire locks from</param>
        /// <exception cref="LockInvocationException">An exception was thrown during a lock monitoring event.</exception>
        public CombinationLock(IEnumerable<T> groupProvider) : this(groupProvider, "CombinationLock")
        {
        }

        /// <summary>
        /// Constructs a new <see cref="Lock"/> object
        /// </summary>
        /// <param name="groupProvider">A collection of objects to acquire locks from</param>
        /// <param name="context">An object that identifies this instance</param>
        /// <exception cref="LockInvocationException">An exception was thrown during a lock monitoring event.</exception>
        public CombinationLock(IEnumerable<T> groupProvider, object context) : base(context)
        {
            Items = new ReadOnlyCollection<T>(groupProvider.ToList());
        }

        /// <summary>
        /// Constructs a new <see cref="CombinationLock{T}"/> object
        /// </summary>
        /// <param name="groupProvider">A collection of objects to acquire locks from</param>
        /// <param name="contextProvider">A callback that returns a context on demand</param>
        /// <exception cref="LockInvocationException">An exception was thrown during a lock monitoring event.</exception>
        public CombinationLock(IEnumerable<T> groupProvider, ContextProvider contextProvider) : base(contextProvider)
        {
            Items = new ReadOnlyCollection<T>(groupProvider.ToList());
        }

        public new IScopedCollection<T> Acquire()
        {
            if (workCompleted)
                throw new ObjectDisposedException(nameof(CombinationLock<T>));

            ThreadSafeWorker worker = new ThreadSafeWorker(Items.GetLocks());

            workFinalizer = worker.DoWorkAsync(waitForCompletion);
            return this;
        }

        private async DotNetTask waitForCompletion()
        {
            DotNetTask waitTask = Task.WaitUntil(waitOperation, cancellationToken: workFinalizer.CancellationToken);
            await waitTask.ConfigureAwait(false);

            bool waitOperation()
            {
                return workCompleted;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (workCompleted) return;

            //Work is complete at this point. It is safe to release the locks.
            workCompleted = true;
            workFinalizer.Cancel();
        }
    }

    public interface IScopedCollection<T> : IDisposable
    {
        IReadOnlyCollection<T> Items { get; }
    }
}
