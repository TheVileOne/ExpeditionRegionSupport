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
        /// <summary>
        /// Apply modifications to the text data as it is read from file
        /// </summary>
        public StringDelegates.Format LineFormatter { get; set; }

        /// <summary>
        /// Apply conditions that pass over text data that matches certain criteria
        /// </summary>
        public StringDelegates.Validate SkipConditions { get; set; }

        public bool IsDisposed { get; private set; }

        public TextStream(string file) : base(file)
        {
            LineFormatter = new StringDelegates.Format(s => s);
            SkipConditions = new StringDelegates.Validate(s => false);
        }

        public override string ReadLine()
        {
            if (IsDisposed) return null;

            string line = base.ReadLine();

            if (line != null)
                return LineFormatter.Invoke(line);
            return line;
        }

        /// <summary>
        /// Reads from file one line at a time
        /// </summary>
        public IEnumerable<string> ReadLines()
        {
            string line;
            do
            {
                line = ReadLine();

                if (line == null)
                    yield break;

                if (SkipConditions(line))
                    continue;

                yield return line;
            }
            while (line != null);
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
