using BepInEx;
using System;
using System.IO;
using System.Reflection;

namespace LogUtils
{
    public class PersistentLogFileWriter : StreamWriter, IDisposable
    {
        public bool CanWrite
        {
            get
            {
                if (IsDisposed) return false;

                //This will happen when resuming from a stream interruption
                if (Handle.Stream != BaseStream)
                    SetStreamFromHandle();

                return !Handle.IsClosed;
            }
        }

        public PersistentLogFileHandle Handle { get; private set; }

        public PersistentLogFileWriter(PersistentLogFileHandle handle) : base(handle.Stream, Utility.UTF8NoBom)
        {
            //AutoFlush = true;
            Handle = handle;
        }

        /// <summary>
        /// Writes the log buffer to file
        /// </summary>
        /// <exception cref="ObjectDisposedException">The underlying stream element for this instance has been disposed of</exception>
        public override void Flush()
        {
            if (CanWrite)
                base.Flush();
        }

        /// <summary>
        /// Injects file handle stream into the base StreamWriter
        /// </summary>
        internal void SetStreamFromHandle()
        {
            UtilityLogger.Log("Refreshing write stream");

            BindingFlags searchFlags = BindingFlags.NonPublic | BindingFlags.Instance;
            typeof(StreamWriter).GetField("stream", searchFlags).SetValue(this, Handle.Stream);
        }

        protected bool IsDisposed;
        protected bool IsDisposing;

        protected override void Dispose(bool disposing)
        {
            IsDisposing = true;

            try
            {
                //Handle base logic before disposing handle. The stream buffer needs to be flushed before handle can be disposed
                base.Dispose(disposing);
                Handle?.Dispose();
                Handle = null;
            }
            finally
            {
                IsDisposed = true;
                IsDisposing = false;
            }
        }

        ~PersistentLogFileWriter()
        {
            Dispose(false);
        }
    }
}
