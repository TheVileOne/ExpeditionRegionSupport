using System;
using System.Collections.Generic;
using System.Linq;

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
            IEnumerable<Lock> locksEnumerable = selectLocks();
            int locksEntered = 0;

            bool allLocksEntered = false;
            try
            {
                //Activate all locks before doing any work
                foreach (Lock lockObj in locksEnumerable)
                {
                    lockObj.Acquire();
                    locksEntered++;
                }

                allLocksEntered = true;
                work.Invoke();
            }
            finally
            {
                if (!allLocksEntered)
                    UtilityLogger.LogWarning("LogUtils was unable to enter all locks. Work was aborted");

                var locksEnumerator = locksEnumerable.GetEnumerator();

                //Release them when work is finished
                while (locksEntered != 0)
                {
                    locksEntered--;
                    locksEnumerator.MoveNext();

                    locksEnumerator.Current.Release();
                }
            }
        }

        //internal void AcquireLock(object lockObj)
        //{
        //    Lock lockCast = lockObj as Lock;

        //    if (lockCast != null)
        //    {
        //        lockCast.Acquire();
        //        return;
        //    }
        //    Monitor.Enter(lockObj);
        //}

        //internal void ReleaseLock(object lockObj)
        //{
        //    Lock lockCast = lockObj as Lock;

        //    if (lockCast != null)
        //    {
        //        lockCast.Release();
        //        return;
        //    }
        //    Monitor.Exit(lockObj);
        //}

        private IEnumerable<object> safeGetLocks() => (UseEnumerableWrapper ? _locks.ToArray() : _locks).Where(o => o != null);

        private IEnumerable<Lock> selectLocks()
        {
            var locks = safeGetLocks();

            return locks.Select(obj =>
            {
                Lock lockCast = obj as Lock;

                if (lockCast != null)
                    return lockCast;
                return new AdapterLock(obj);
            });
        }
    }
}
