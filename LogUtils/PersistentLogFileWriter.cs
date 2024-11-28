using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// <exception cref="ObjectDisposedException"></exception>
        public override void Flush()
        {
            if (Handle.IsClosed)
                throw new ObjectDisposedException("Cannot access a disposed LogWriter");
            base.Flush();
        }

        public new void Dispose()
        {
            base.Dispose();

            Handle.Dispose();
            Handle = null;
        }
    }
}
