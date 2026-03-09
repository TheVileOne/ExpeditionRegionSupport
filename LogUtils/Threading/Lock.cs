using LogUtils.Events;
using System;
using System.Threading;

namespace LogUtils.Threading
{
    /// <summary>
    /// A wrapper class for a locked object implementation. API exposes a lockable Scope designed to work with with 'using' keyword, does not work well with 'lock'
    /// </summary>
    public class Lock : IComparable<Lock>
    {
        /// <summary>
        /// The number of acquired locks yet to be released
        /// </summary>
        public int ActiveCount;

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
        public bool IsAcquiredByCurrentThread => ActiveCount > 0 && Monitor.IsEntered(LockObject);

        /// <summary>
        /// Suppresses the next Release attempt made on this lock by the current thread
        /// </summary>
        public bool SuppressNextRelease
        {
            get => suppressReleaseCounter > 0;
            set
            {
                if (!value)
                {
                    //Only a lock monitor event is suppressed, we should not unsuppress it. Users should avoid suppression if it produces an undesirable effect.
                    if (suppressReleaseCounter > 0)
                        throw new InvalidOperationException("Suppression cannot be disabled");
                    return;
                }
                suppressReleaseCounter++;
            }
        }

        [ThreadStatic]
        private int suppressReleaseCounter = 0;

        private static int _nextID;
        private object _context;
        private readonly ContextProvider _contextProvider;

        private readonly int _lockID;
        private readonly Scope lockScope;

        protected object LockObject;

        #region Event Handling
        private static readonly ThreadSafeEvent<Lock, EventID> lockEvent = new ThreadSafeEvent<Lock, EventID>();

        /// <summary>
        /// Listens to lock monitoring events, (such as a lock being created, acquired, or released)
        /// </summary>
        public static event EventHandler<Lock, EventID> OnEvent
        {
            add => lockEvent.Handler += value;
            remove => lockEvent.Handler -= value;
        }

        internal void RaiseEvent(EventID eventID)
        {
            try
            {
                lockEvent.Raise(this, eventID);
            }
            catch (LockInvocationException)
            {
                throw;
            }
            catch (Exception ex) //Wrap other exceptions into an expected type
            {
                throw new LockInvocationException(this, eventID, ex);
            }
        }
        #endregion

        /// <summary>
        /// Constructs a new <see cref="Lock"/> object
        /// </summary>
        /// <exception cref="LockInvocationException">An exception was thrown during a lock monitoring event.</exception>
        public Lock() : this(false)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="Lock"/> object
        /// </summary>
        /// <param name="context">An object that identifies this instance</param>
        /// <exception cref="LockInvocationException">An exception was thrown during a lock monitoring event.</exception>
        public Lock(object context) : this(true)
        {
            _context = context;
            RaiseEvent(EventID.LockCreated);
        }

        /// <summary>
        /// Constructs a new <see cref="Lock"/> object
        /// </summary>
        /// <param name="contextProvider">A callback that returns a context on demand</param>
        /// <exception cref="LockInvocationException">An exception was thrown during a lock monitoring event.</exception>
        public Lock(ContextProvider contextProvider) : this(true)
        {
            _contextProvider = contextProvider;
            RaiseEvent(EventID.LockCreated);
        }

        private Lock(bool contextExpected)
        {
            _lockID = Interlocked.Increment(ref _nextID);

            LockObject = lockScope = new Scope(this);

            if (!contextExpected)
                RaiseEvent(EventID.LockCreated);
        }

        internal static int CurrentFileLockAcquireCount;

        /// <summary>
        /// Acquires a lock on the current thread
        /// </summary>
        /// <exception cref="LockInvocationException">An exception was thrown during a lock monitoring event.</exception>
        public Scope Acquire()
        {
            //Blockless attempt to enter lock
            bool lockEntered = Monitor.TryEnter(LockObject, 1);

            RaiseEvent(lockEntered ? EventID.LockAcquired : EventID.WaitingToAcquire);

            if (!lockEntered)
            {
                if (CurrentFileLockAcquireCount > 0 && Equals(UtilityCore.RequestHandler.RequestProcessLock))
                {
                    UtilityLogger.DebugLog("Unsafe lock conditions present. Enabling deadlock prevention feature.");
                    //Block until lock is entered or lock timeout is reached
                    if (!Monitor.TryEnter(LockObject, TimeSpan.FromMilliseconds(250)))
                    {
                        UtilityLogger.DebugLog("Lock attempt timed out");
                        RaiseEvent(EventID.FailedToAcquire);
                        suppressReleaseCounter++;
                        return lockScope;
                    }
                }
                else
                {
                    //Block until lock is entered
                    Monitor.Enter(LockObject);
                }
                RaiseEvent(EventID.LockAcquired);
            }

            if (this is FileLock)
                Interlocked.Increment(ref CurrentFileLockAcquireCount);

            ActiveCount++;
            return lockScope;
        }

        /// <summary>
        /// Releases a lock monitored on the current thread
        /// </summary>
        /// <exception cref="LockInvocationException">An exception was thrown during a lock monitoring event.</exception>
        /// <exception cref="SynchronizationLockException">"The current thread does not own the lock for the specified object."</exception>
        public void Release()
        {
            if (SuppressNextRelease)
            {
                UtilityLogger.DebugLog($"Lock release has been suppressed [{Context}]");
                suppressReleaseCounter--;
                return;
            }

            if (ActiveCount == 0) return;

            if (this is FileLock)
                Interlocked.Decrement(ref CurrentFileLockAcquireCount);

            ActiveCount--;
            Monitor.Exit(LockObject);
            RaiseEvent(EventID.LockReleased);
        }

        internal void BindToRainWorld()
        {
            //RainWorld._loggingLock = LockObject;
        }

        int IComparable<Lock>.CompareTo(Lock other)
        {
            if (other == null)
                return int.MaxValue;
            return _lockID.CompareTo(other._lockID);
        }

        /// <summary>
        /// A disposable type that will release a lock on the current thread on dispose
        /// </summary>
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
            Unknown,
            LockCreated,
            LockReleased,
            LockAcquired,
            WaitingToAcquire,
            FailedToAcquire,
        }

        /// <summary>
        /// A callback used to provide a context on demand
        /// </summary>
        public delegate object ContextProvider();
    }
}
