using BepInEx;
using System;
using System.IO;

namespace LogUtils
{
    public class PersistentLogFileWriter : StreamWriter, IDisposable
    {
        public PersistentLogFileHandle Handle { get; private set; }

        public PersistentLogFileWriter(PersistentLogFileHandle handle) : base(handle.Stream, Utility.UTF8NoBom)
        {
            Handle = handle;
        }

        /// <summary>
        /// Writes the log buffer to file
        /// </summary>
        /// <exception cref="ObjectDisposedException">The underlying stream element for this instance has been disposed of</exception>
        public override void Flush()
        {
            if (Handle == null || Handle.IsClosed)
                throw new ObjectDisposedException("Cannot access a disposed LogWriter");
            base.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            if (Handle != null)
            {
                Handle.Dispose();
                Handle = null;
            }
            base.Dispose(disposing);
        }

        ~PersistentLogFileWriter()
        {
            Dispose(false);
        }
    }
}
