using LogUtils.Properties;
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

        private DotNetTask workTask;
        private bool workCompleted;

        public CombinationLock(IEnumerable<T> groupProvider)
        {
            Items = new ReadOnlyCollection<T>(groupProvider.ToList());
        }

        public new IScopedCollection<T> Acquire()
        {
            ThreadSafeWorker worker = new ThreadSafeWorker(Items.GetLocks());

            workTask = worker.DoWorkAsync(waitForCompletion);
            return this;
        }

        private async DotNetTask waitForCompletion()
        {
            await Task.WaitUntil(() => workCompleted);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (workTask == null)
            {
                UtilityLogger.LogWarning("Lock was disposed without any work");
                return;
            }

            workCompleted = true;
            try
            {
                workTask.Wait(); //Wait for task to actually complete
            }
            catch (Exception ex)
            {
                UtilityLogger.LogError(ex);
            }
            finally
            {
                workTask.Dispose();
                workTask = null;
            }
        }
    }

    public interface IScopedCollection<T> : IDisposable
    {
        IReadOnlyCollection<T> Items { get; }
    }
}
