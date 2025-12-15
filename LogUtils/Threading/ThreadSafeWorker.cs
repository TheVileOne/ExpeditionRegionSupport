using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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

        public ThreadSafeWorker(object lockObject)
        {
            UseEnumerableWrapper = false;
            _locks = [lockObject];
        }

        public ThreadSafeWorker(IEnumerable<object> locks)
        {
            _locks = locks;
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

        internal LockEnumerator GetEnumerator()
        {
            return new LockEnumerator(this, _locks);
        }

        internal readonly struct LockEnumerator : IEnumerator<Lock>
        {
            private readonly ThreadSafeWorker _worker;
            private readonly IEnumerable<Lock> _locks;

            private readonly IEnumerator<Lock> _innerEnumerator;

            public LockEnumerator(ThreadSafeWorker worker, IEnumerable<object> locks)
            {
                _worker = worker;
                _locks = (_worker.UseEnumerableWrapper ? locks.ToArray() : locks)
                         .Where(obj => obj != null)
                         .Select(obj =>
                         {
                             Lock lockCast = obj as Lock;
                             return lockCast ?? new AdapterLock(obj);

                         });

                _innerEnumerator = _locks.GetEnumerator();
            }

            public Lock Current => _innerEnumerator.Current;

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                Reset();
            }

            public bool MoveNext()
            {
                return _innerEnumerator.MoveNext();
            }

            public void Reset()
            {
                _innerEnumerator.Reset();
            }

            /// <summary>
            /// Acquires all locks stored in the enumerable
            /// </summary>
            /// <exception cref="LockInvocationException"></exception>
            internal readonly void Acquire()
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
                            locksEntered++;
                        }
                        allLocksAcquired = true;
                    }
                    catch (LockInvocationException) //Lock acquire event was canceled
                    {
                        Reset();
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

                    if (source == accessedLock)
                    {
                        accessedLock = null; //Clean up thread local state before we abort
                        throw new LockInvocationException(source, data);
                    }
                }
            }

            internal void Release()
            {
                foreach (Lock lockObj in _locks)
                    lockObj.Release();
            }
        }

        [ThreadStatic]
        private static Lock accessedLock;
    }
}
