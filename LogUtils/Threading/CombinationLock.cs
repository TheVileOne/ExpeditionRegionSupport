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
            ThreadSafeWorker worker = new ThreadSafeWorker(Items.GetLocks());

            workFinalizer = worker.DoWorkAsync(waitForCompletion);
            return this;
        }

        private async DotNetTask waitForCompletion()
        {
            DotNetTask waitTask = Task.WaitUntil(() =>
            {
                return workCompleted;
            });
            await waitTask.ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            DotNetTask localWaitTask = workFinalizer.Current;
            workCompleted = true;
            try
            {
                localWaitTask.Wait(); //Wait for task to actually complete
                localWaitTask.Dispose();
            }
            catch (Exception ex)
            {
                UtilityLogger.LogError(ex);
            }
            finally
            {
                workFinalizer.CompleteTask();
            }
        }
    }

    public interface IScopedCollection<T> : IDisposable
    {
        IReadOnlyCollection<T> Items { get; }
    }
}
