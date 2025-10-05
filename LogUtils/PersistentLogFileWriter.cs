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
        /// Flushes the stream buffer to file
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

        #region Dispose handling

        /// <summary/>
        protected bool IsDisposed;

        /// <summary>
        /// Performs tasks for disposing a <see cref="PersistentLogFileWriter"/>
        /// </summary>
        /// <param name="disposing">Whether or not the dispose request is invoked by the application (true), or invoked by the destructor (false)</param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                Flush();
            }
            catch (ObjectDisposedException) { }

            try
            {
                Handle?.Dispose();
                Handle = null;
            }
            finally
            {
                IsDisposed = true;
            }
        }

        /// <summary/>
        ~PersistentLogFileWriter()
        {
            Dispose(disposing: false);
        }
        #endregion
    }
}
