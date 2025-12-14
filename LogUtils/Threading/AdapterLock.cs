using System;

namespace LogUtils.Threading
{
    /// <summary>
    /// A lock type that acts as a wrapper around a provided lock object
    /// </summary>
    public sealed class AdapterLock : Lock
    {
        /// <summary>
        /// Constructs a new <see cref="AdapterLock"/> object
        /// </summary>
        /// <param name="lockObject">An object that will be used for locking</param>
        /// <exception cref="ArgumentNullException">Lock object is a null value.</exception>
        /// <exception cref="LockInvocationException">An exception was thrown during a lock monitoring event.</exception>
        public AdapterLock(object lockObject) : base()
        {
            validateAndAssign(lockObject);
        }

        /// <summary>
        /// Constructs a new <see cref="AdapterLock"/> object
        /// </summary>
        /// <param name="lockObject">An object that will be used for locking</param>
        /// <param name="context">An object that identifies this instance</param>
        /// <exception cref="ArgumentNullException">Lock object is a null value.</exception>
        /// <exception cref="LockInvocationException">An exception was thrown during a lock monitoring event.</exception>
        public AdapterLock(object lockObject, object context) : base(context)
        {
            validateAndAssign(lockObject);
        }

        /// <summary>
        /// Constructs a new <see cref="AdapterLock"/> object
        /// </summary>
        /// <param name="lockObject">An object that will be used for locking</param>
        /// <param name="contextProvider">A callback that returns a context on demand</param>
        /// <exception cref="ArgumentNullException">Lock object is a null value.</exception>
        /// <exception cref="LockInvocationException">An exception was thrown during a lock monitoring event.</exception>
        public AdapterLock(object lockObject, ContextProvider contextProvider) : base(contextProvider)
        {
            validateAndAssign(lockObject);
        }

        private void validateAndAssign(object lockObject)
        {
            if (lockObject == null)
                throw new ArgumentNullException(nameof(lockObject));
            LockObject = lockObject;
        }
    }
}
