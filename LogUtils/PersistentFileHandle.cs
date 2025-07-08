using System;
using System.IO;

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
        /// Contains a reference to the handle responsible for reopening the FileStream after interruption
        /// </summary>
        private StreamResumer resumeHandle;

        public bool WaitingToResume => resumeHandle != null;

        public PersistentFileHandle()
        {
            UtilityCore.PersistenceManager.References.Add(this);
        }

        /// <summary>
        /// Closes the stream. Mod should resume stream when file operations are finished
        /// </summary>
        public virtual StreamResumer InterruptStream()
        {
            if (WaitingToResume)
            {
                UtilityLogger.LogWarning("Filestream already interrupted.. returning existing resume state");
                return resumeHandle;
            }

            NotifyOnInterrupt();
            Stream?.Close();
            return new StreamResumer(PersistentFileHandle_OnResume);
        }

        private void PersistentFileHandle_OnResume()
        {
            resumeHandle = null; //Must be set to null before CreateFileStream is invoked
            NotifyOnResume();
            CreateFileStream();
        }

        protected virtual void NotifyOnInterrupt()
        {
            UtilityLogger.Log("Interrupting filestream");
        }

        protected virtual void NotifyOnResume()
        {
            UtilityLogger.Log("Resuming filestream");
        }

        protected abstract void CreateFileStream();

        #region Dispose pattern

        protected bool IsDisposed;
        protected bool IsDisposing;

        /// <inheritdoc/>
        public void Dispose()
        {
            //Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
                OnDispose();

            Stream?.Dispose();
            Stream = null;
            IsDisposed = true;
            IsDisposing = false;
        }

        ~PersistentFileHandle()
        {
            //Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        /// <summary>
        /// Runs logic that should happen at the very start of a dispose request
        /// </summary>
        protected void OnDispose()
        {
            if (IsDisposing) return;

            IsDisposing = true; //Ensures that OnDispose is only handled once
            UtilityCore.PersistenceManager.NotifyOnDispose(this);
            Lifetime.SetDuration(0); //Disposed handles should not be considered alive
        }
        #endregion
    }

    public class StreamResumer
    {
        private readonly Action resumeCallback;

        public bool Handled { get; protected set; }

        public StreamResumer(Action callback)
        {
            resumeCallback = callback;
        }

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
