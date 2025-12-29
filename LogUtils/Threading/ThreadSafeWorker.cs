using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DotNetTask = System.Threading.Tasks.Task;

namespace LogUtils.Threading
{
    public sealed class ThreadSafeWorker
    {
        private readonly IEnumerable<object> _locks;

        /// <summary>
        /// A flag that indicates whether locks should be extracted from the provided enumerable, and placed into an array before work starts
        /// </summary>
        /// <remarks>May be set to false if underlying enumerable is unlikely to be modified during work</remarks>
        public bool UseEnumerableWrapper = true;

        /// <summary>
        /// Wait time to retry acquiring the locks (in the case of lock contention) 
        /// </summary>
        public TimeSpan RetryInterval = TimeSpan.FromMilliseconds(5);

        public ThreadSafeWorker(params object[] locks)
        {
            _locks = locks;
            UseEnumerableWrapper = false; //Even if the user passes in an array, it should be safe from collection enumeration issues 
        }

        public ThreadSafeWorker(IEnumerable<object> locks)
        {
            _locks = locks;
            if (_locks is object[])
                UseEnumerableWrapper = false;
        }

        public void DoWork(Action work)
        {
            LockEnumerator locks = GetEnumerator();

            locks.Acquire();
            try
            {
                work.Invoke();
            }
            finally
            {
                locks.Release();
            }
        }

        /// <summary>
        /// Perform work that must be run asynchronously on a background thread
        /// </summary>
        /// <param name="work">Work to be performed</param>
        /// <param name="cancellationToken">A token for cancelling the task</param>
        /// <returns>An enumerator to signal the end of work</returns>
        public TaskFinalizer DoWorkAsync(Func<DotNetTask> work, CancellationToken cancellationToken = default)
        {
            LockEnumerator locks = GetEnumerator();

            locks.Acquire();
            try
            {
                return TaskFinalizer.CreateFinalizer(DotNetTask.Run(work, cancellationToken));
            }
            finally
            {
                locks.Release();
            }
        }

        internal LockEnumerator GetEnumerator()
        {
            return new LockEnumerator(this, UseEnumerableWrapper ? _locks.ToArray() : _locks);
        }

        internal struct LockEnumerator : IEnumerator<Lock>
        {
            private readonly ThreadSafeWorker _worker;
            private readonly SortedSet<Lock> _locks;

            private IEnumerator<Lock> _innerEnumerator;

            public LockEnumerator(ThreadSafeWorker worker, IEnumerable<object> locks)
            {
                _worker = worker;
                _locks =
                [
                    .. locks.Where(obj => obj != null)
                            .Select(obj =>
                            {
                                Lock lockCast = obj as Lock;
                                return lockCast ?? new AdapterLock(obj);
                            }),
                ];

                _innerEnumerator = _locks.GetEnumerator();
            }

            public readonly Lock Current => _innerEnumerator.Current;

            readonly object IEnumerator.Current => Current;

            public void Dispose()
            {
                Reset();
            }

            public readonly bool MoveNext()
            {
                return _innerEnumerator.MoveNext();
            }

            public void Reset()
            {
                _innerEnumerator = _locks.GetEnumerator();
            }

            /// <summary>
            /// Acquires all locks stored in the enumerable
            /// </summary>
            /// <exception cref="LockInvocationException"></exception>
            internal void Acquire()
            {
                Lock.OnEvent += onLockEvent;

                bool allLocksAcquired = false;
                while (!allLocksAcquired)
                {
                    int locksEntered = 0;
                    try
                    {
                        while (MoveNext())
                        {
                            accessedLock = Current;
                            Current.Acquire();

                            accessedLock = null;
                            locksEntered++;
                        }
                        allLocksAcquired = true;
                    }
                    catch (LockInvocationException) //Lock acquire event was canceled
                    {
                        //UtilityLogger.DebugLog($"Aborting with {locksEntered} processed");
                        try
                        {
                            Reset();
                        }
                        catch (Exception ex)
                        {
                            UtilityLogger.DebugLog(ex);
                        }
                        IEnumerable<Lock> acquiredLocks = _locks.Take(locksEntered);

                        foreach (Lock lockObj in acquiredLocks)
                            lockObj.Release();

                        Thread.Sleep(_worker.RetryInterval);
                    }
                }
                Lock.OnEvent -= onLockEvent;
                Reset();

                static void onLockEvent(Lock source, Lock.EventID data)
                {
                    if (data != Lock.EventID.WaitingToAcquire)
                        return;

                    //UtilityLogger.DebugLog("Waiting for access");
                    if (source == accessedLock)
                    {
                        accessedLock = null; //Clean up thread local state before we abort
                        throw new LockInvocationException(source, data);
                    }
                }
            }

            internal readonly void Release()
            {
                foreach (Lock lockObj in _locks)
                    lockObj.Release();
            }
        }

        [ThreadStatic]
        private static Lock accessedLock;
    }
}
