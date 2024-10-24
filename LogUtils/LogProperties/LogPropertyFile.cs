using LogUtils.Helpers;
using System;
using System.IO;

namespace LogUtils.Properties
{
    /// <summary>
    /// A class for reading, or writing to the LogProperties file
    /// </summary>
    public class LogPropertyFile
    {
        /// <summary>
        /// The full path to the file containing properties for all log files
        /// </summary>
        public string FilePath;

        /// <summary>
        /// The underlying filestream if it exists, null otherwise. This stream is always active when the file is present. 
        /// Please do not close the stream. Interrupt and resume the stream instead.
        /// </summary>
        public FileStream FileStream;

        /// <summary>
        /// Gets whether the underlying filestream has been closed
        /// </summary>
        public bool IsClosed => FileStream?.SafeFileHandle.IsClosed ?? true;

        internal LogPropertyReader Reader;
        internal LogPropertyWriter Writer;

        public LogPropertyFile()
        {
            FilePath = Path.Combine(Paths.StreamingAssetsPath, "logs.txt");

            if (File.Exists(FilePath))
                CreateFileStream();

            Reader = new LogPropertyReader(this);
            Writer = new LogPropertyWriter(this);
        }

        /// <summary>
        /// Closes the stream. Mod should resume stream when file operations are finished
        /// </summary>
        public StreamResumer InterruptStream()
        {
            FileStream?.Close();
            return new StreamResumer(CreateFileStream);
        }

        /// <summary>
        /// Creates a new filestream if current one is closed, and seeks to the start of the filestream
        /// </summary>
        public void PrepareStream()
        {
            if (IsClosed)
            {
                CreateFileStream();
                return;
            }
            FileStream.Seek(0, SeekOrigin.Begin);
        }

        internal void CreateFileStream()
        {
            FileStream = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.SequentialScan);
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
