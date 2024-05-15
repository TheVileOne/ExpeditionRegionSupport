using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Data
{
    /// <summary>
    /// This custom class includes a flag indicated when it has been disposed
    /// </summary>
    public class TextStream : StreamReader, IDisposable
    {
        public IEnumerable<string> LineData = null;

        public bool IsDisposed { get; private set; }

        public TextStream(string file) : base(file)
        {
        }

        public override string ReadLine()
        {
            if (IsDisposed) return null;

            return base.ReadLine();
        }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }

        public override void Close()
        {
            if (!IsDisposed)
                base.Close();
        }
    }
}
