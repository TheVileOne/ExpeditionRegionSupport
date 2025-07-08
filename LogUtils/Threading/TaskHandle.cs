using System;
using System.Threading;
using System.Threading.Tasks;
using DotNetTask = System.Threading.Tasks.Task;

namespace LogUtils.Threading
{
    public class TaskHandle : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public bool IsValid => Task != null;

        /// <summary>
        /// The asynchronous execution task
        /// </summary>
        public DotNetTask Task;

        /// <summary>
        /// The task that this handle belongs to
        /// </summary>
        public Task Owner;

        /// <summary>
        /// An object that LogTasker uses to request early termination of a task
        /// </summary>
        public CancellationTokenSource CancellationToken = new CancellationTokenSource();

        /// <summary>
        /// The number of unreleased references to this instance
        /// </summary>
        private int accessCount = 0;

        public TaskHandle(Task owner)
        {
            Owner = owner;
        }

        public void OnAccess()
        {
            accessCount++;
        }

        protected void RevokeAccess()
        {
            if (accessCount == 0)
                UtilityLogger.LogWarning("Abnormal amount of revoke access attempts made");

            accessCount--;
        }

        public void BlockUntilTaskEnds(int frequency = 5, int timeout = -1)
        {
            var task = Task;

            if (task == null)
            {
                UtilityLogger.LogWarning("Attempted to block a null task");
                return;
            }

            Threading.Task.WaitUntil(() => task.Status >= TaskStatus.RanToCompletion, frequency, timeout).Wait();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            RevokeAccess();

            //Disposal should only occur when there are no longer any references to the handle
            if (accessCount == 0)
            {
                Task = null;

                if (Owner != null) //Not expected to be null
                {
                    if (Owner.Handle == this)
                        Owner.Handle = null;
                    Owner = null;
                }
            }
        }
    }
}
