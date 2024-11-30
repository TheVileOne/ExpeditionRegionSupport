using System;
using System.Collections.Generic;
using System.Threading;

namespace LogUtils.Threading
{
    public sealed class ThreadSafeWorker
    {
        private readonly IEnumerable<object> _locks;

        public ThreadSafeWorker(IEnumerable<object> locks)
        {
            _locks = locks;
        }

        public void DoWork(Action work)
        {
            //Activate all locks before doing any work
            foreach (object objLock in _locks)
                Monitor.Enter(objLock);

            try
            {
                work.Invoke();
            }
            finally
            {
                //Release them when work is finished
                foreach (object objLock in _locks)
                    Monitor.Exit(objLock);
            }
        }
    }
}
