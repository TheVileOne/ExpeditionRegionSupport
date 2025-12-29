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

        public CombinationLock(IEnumerable<T> groupProvider)
        {
            Items = new ReadOnlyCollection<T>(groupProvider.ToList());
        }

        public new IScopedCollection<T> Acquire()
        {
            ThreadSafeWorker worker = new ThreadSafeWorker(Items.GetLocks());

            workFinalizer = worker.DoWorkAsync(waitForCompletion);
            return this;
        }

       private DotNetTask waitTask;
        private async DotNetTask waitForCompletion()
        {
            waitTask = Task.WaitUntil(() =>
            {
                return workCompleted;
            });
            await waitTask.ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            DotNetTask localWaitTask = waitTask;
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
