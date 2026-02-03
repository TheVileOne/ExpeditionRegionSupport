using System;
using System.IO;
using UnityEngine;

namespace LogUtils
{
    /// <summary>
    /// A mod friendly class for handling persistent file stream operations 
    /// </summary>
    public abstract class PersistentFileHandle : IDisposable
    {
        public bool IsAlive => Lifetime.IsAlive;

        /// <summary>
        /// Gets whether the underlying filestream has been closed
        /// </summary>
        public bool IsClosed => Stream == null || (!Stream.CanWrite && !Stream.CanRead);

        /// <summary>
        /// A managed representation of the time remaining before filestream is disposed in milliseconds
        /// </summary>
        public readonly Lifetime Lifetime = Lifetime.FromMilliseconds(LifetimeDuration.Infinite);

        /// <summary>
        /// The underlying filestream if it exists, null otherwise. This stream is always active when the file is present 
        /// </summary>
        /// <remarks>Please do not close the stream. Interrupt and resume the stream instead</remarks>
        public FileStream Stream;

        /// <summary>
        /// Contains a reference to the handle responsible for reopening the stream after interruption
        /// </summary>
        private StreamResumer resumeHandle;

        public bool WaitingToResume => resumeHandle != null;

        public PersistentFileHandle()
        {
            UtilityCore.PersistenceManager.References.Add(this);
        }

        /// <summary>
        /// Temporarily closes the stream
        /// </summary>
        /// <returns>A <see cref="StreamResumer"/> object that supports resuming the stream when file operations are finished</returns>
        public virtual StreamResumer InterruptStream()
        {
            if (WaitingToResume)
            {
                UtilityLogger.LogWarning("Filestream already interrupted.. returning existing resume state");
                return resumeHandle;
            }

            NotifyOnInterrupt();
            Stream?.Close();

            void onResume()
            {
                resumeHandle = null; //Must be set to null before CreateFileStream is invoked
                NotifyOnResume();
                CreateFileStream();
            }
            return new StreamResumer(onResume);
        }

        /// <summary>
        /// Notifies that a stream interruption operation has started
        /// </summary>
        protected virtual void NotifyOnInterrupt()
        {
            UtilityLogger.Log("Interrupting filestream");
        }

        /// <summary>
        /// Notifies that a stream interruption operation has completed
        /// </summary>
        protected virtual void NotifyOnResume()
        {
            UtilityLogger.Log("Resuming filestream");
        }

        /// <summary>
        /// Opens a filestream for the targeted file 
        /// </summary>
        /// <exception cref="IOException">Stream has been interrupted</exception>
        protected abstract void CreateFileStream();

        #region Dispose handling

        /// <summary/>
        protected bool IsDisposed;
        internal bool IsDisposing;

        /// <summary>
        /// Performs tasks for disposing a <see cref="PersistentFileHandle"/>
        /// </summary>
        /// <param name="disposeState">Whether or not the dispose request is invoked by the application (true), or invoked by the destructor (false)</param>
        protected void Dispose(bool disposeState)
        {
            if (IsDisposed || IsDisposing) return;

            IsDisposing = true;
            Action<bool>[] stages = [BeginDispose, EndDispose];

            //Ensure that begin, and end stages always invoke even in the case of an exception
            foreach (var disposeStage in stages)
            {
                try
                {
                    disposeStage.Invoke(disposeState);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            IsDisposed = true;
            IsDisposing = false; //Dispose complete, no longer in disposing state
        }

        /// <summary>
        /// Dispose logic that must run at the start of a dispose request
        /// </summary>
        /// <inheritdoc cref="Dispose(bool)" select="param"/>
        protected virtual void BeginDispose(bool disposeState)
        {
            if (!disposeState) return; //The code here is not necessary to be called from the destructor

            UtilityCore.PersistenceManager.NotifyOnDispose(this);
            Lifetime.SetDuration(0); //Disposed handles should not be considered alive
        }

        /// <summary>
        /// Dispose logic that must run at the end of a dispose request
        /// </summary>
        /// <inheritdoc cref="Dispose(bool)" select="param"/>
        protected virtual void EndDispose(bool disposeState)
        {
            //This code is alright to be called from any dispose state
            Stream?.Dispose();
            Stream = null;
        }

        /// <inheritdoc cref="Dispose(bool)"/>
        public void Dispose()
        {
            Dispose(disposeState: true);
            GC.SuppressFinalize(this);
        }

        /// <summary/>
        ~PersistentFileHandle()
        {
            Dispose(disposeState: false);
        }
        #endregion
    }

    /// <summary>
    /// A class instantiated by LogUtils for the purpose of resuming a stream that has been interrupted
    /// </summary>
    public class StreamResumer
    {
        private readonly Action resumeCallback;

        /// <summary>
        /// Indicates the resumed state has been handled
        /// </summary>
        public bool Handled { get; protected set; }

        public StreamResumer(Action callback)
        {
            resumeCallback = callback;
        }

        /// <summary>
        /// Refreshes an interrupted stream
        /// </summary>
        public void Resume()
        {
            if (Handled)
            {
                UtilityLogger.LogWarning("Filestream cannot be resumed more than once per interrupt");
                return;
            }

            try
            {
                resumeCallback.Invoke();
            }
            finally
            {
                Handled = true;
            }
        }
    }
}
