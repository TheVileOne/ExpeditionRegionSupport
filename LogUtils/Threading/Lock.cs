using LogUtils.Events;
using System;
using System.Threading;

namespace LogUtils.Threading
{
    /// <summary>
    /// A wrapper class for a locked object implementation. API exposes a lockable Scope designed to work with with 'using' keyword, does not work well with 'lock'
    /// </summary>
    public class Lock
    {
        /// <summary>
        /// The number of acquired locks yet to be released
        /// </summary>
        public int ActiveCount;

        private object _context;
        private ContextProvider _contextProvider;

        /// <summary>
        /// Information assigned to identify the lock instance
        /// </summary>
        public object Context
        {
            get
            {
                if (_context == null)
                    _context = _contextProvider?.Invoke();
                return _context;
            }
        }

        /// <summary/>
        public bool IsAcquiredByCurrentThread => ActiveCount > 0 && Monitor.IsEntered(lockScope);

        /// <summary>
        /// Suppresses the next Release attempt made on this lock 
        /// </summary>
        public bool SuppressNextRelease;

        private readonly Scope lockScope;

        private static readonly ThreadSafeEvent<Lock, EventID> lockEvent = new ThreadSafeEvent<Lock, EventID>();

        public static event EventHandler<Lock, EventID> OnEvent
        {
            add => lockEvent.Handler += value;
            remove => lockEvent.Handler -= value;
        }

        public Lock()
        {
            lockScope = new Scope(this);
            lockEvent.Raise(this, EventID.LockCreated);
        }

        public Lock(object context) : this()
        {
            _context = context;
        }

        public Lock(ContextProvider contextProvider) : this()
        {
            _contextProvider = contextProvider;
        }

        public Scope Acquire()
        {
            //Blockless attempt to enter scope
            bool lockEntered = Monitor.TryEnter(lockScope, 1);

            lockEvent.Raise(this, lockEntered ? EventID.LockAcquired : EventID.WaitingToAcquire);

            if (!lockEntered)
            {
                //Block until scope is entered
                Monitor.Enter(lockScope);
                lockEvent.Raise(this, EventID.LockAcquired);
            }

            ActiveCount++;
            return lockScope;
        }

        public void Release()
        {
            if (SuppressNextRelease)
            {
                SuppressNextRelease = false;
                return;
            }

            if (ActiveCount == 0) return;

            ActiveCount--;
            Monitor.Exit(lockScope);
            lockEvent.Raise(this, EventID.LockReleased);
        }

        internal void BindToRainWorld()
        {
            //RainWorld._loggingLock = lockScope;
        }

        public sealed class Scope : IDisposable
        {
            private readonly Lock _lock;

            internal Scope(Lock owner)
            {
                _lock = owner;
            }

            /// <summary>
            /// Releases a lock acquired by the calling thread
            /// </summary>
            /// <exception cref="SynchronizationLockException">The calling thread does not have any active locks to release</exception>
            public void Dispose()
            {
                _lock.Release();
            }
        }

        public enum EventID
        {
            LockCreated,
            LockReleased,
            LockAcquired,
            WaitingToAcquire
        }

        /// <summary>
        /// A provided delegate used to provide a context on demand
        /// </summary>
        public delegate object ContextProvider();
    }
}
