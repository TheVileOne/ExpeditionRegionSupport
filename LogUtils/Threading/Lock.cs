﻿using System;
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

        /// <summary>
        /// Suppresses the next Release attempt made on this lock 
        /// </summary>
        public bool SuppressNextRelease;

        private readonly Scope _scope;

        public static event Event OnEvent;

        public Lock()
        {
            _scope = new Scope(this);
            OnEvent?.Invoke(this, EventID.LockCreated);
        }

        public Scope Acquire()
        {
            //Blockless attempt to enter scope
            bool lockEntered = Monitor.TryEnter(_scope, 1);

            OnEvent?.Invoke(this, lockEntered ? EventID.LockAcquired : EventID.WaitingToAcquire);

            if (!lockEntered)
            {
                //Block until scope is entered
                Monitor.Enter(_scope);
                OnEvent?.Invoke(this, EventID.LockAcquired);
            }

            ActiveCount++;
            return _scope;
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
            Monitor.Exit(_scope);
            OnEvent?.Invoke(this, EventID.LockReleased);
        }

        public sealed class Scope : IDisposable
        {
            private readonly Lock _lock;

            internal Scope(Lock owner)
            {
                _lock = owner;
            }

            void IDisposable.Dispose()
            {
                _lock.Release();
            }
        }

        public delegate void Event(Lock sender, EventID eventID);

        public enum EventID
        {
            LockCreated,
            LockReleased,
            LockAcquired,
            WaitingToAcquire
        }
    }
}
