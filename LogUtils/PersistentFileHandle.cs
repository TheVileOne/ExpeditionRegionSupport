using LogUtils.Threading;
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

        protected bool IsDisposed;

        /// <summary>
        /// A managed representation of the time remaining before filestream is disposed in milliseconds
        /// </summary>
        public readonly Lifetime Lifetime = new Lifetime();

        /// <summary>
        /// The underlying filestream if it exists, null otherwise. This stream is always active when the file is present 
        /// <br>Please do not close the stream. Interrupt and resume the stream instead</br>
        /// </summary>
        public FileStream Stream;

        public bool WaitingToResume { get; protected set; }

        public PersistentFileHandle()
        {
            UtilityCore.PersistenceManager.References.Add(new WeakReference<PersistentFileHandle>(this));
        }

        /// <summary>
        /// Closes the stream. Mod should resume stream when file operations are finished
        /// </summary>
        public StreamResumer InterruptStream()
        {
            WaitingToResume = true;
            Stream?.Close();
            return new StreamResumer(CreateFileStream);
        }

        protected abstract void CreateFileStream();

        public virtual void Dispose()
        {
            if (IsDisposed) return;

            Lifetime.SetDuration(0);

            if (Stream != null)
            {
                Stream.Dispose();
                Stream = null;
            }
            IsDisposed = true;
        }
    }

    public class StreamResumer
    {
        private Action resumeCallback;

        public StreamResumer(Action callback)
        {
            resumeCallback = callback;
        }

        public void Resume()
        {
            resumeCallback.Invoke();
        }
    }
}
