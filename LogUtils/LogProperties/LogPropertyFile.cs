﻿using LogUtils.Helpers;
using System.IO;

namespace LogUtils.Properties
{
    /// <summary>
    /// A class for reading, or writing to the LogProperties file
    /// </summary>
    public class LogPropertyFile : PersistentFileHandle
    {
        /// <summary>
        /// The full path to the file containing properties for all log files
        /// </summary>
        public readonly string FilePath;

        internal LogPropertyReader Reader;
        internal LogPropertyWriter Writer;

        public LogPropertyFile() : base()
        {
            Lifetime.UpdateTask.Name = "PropertyFile";
            FilePath = Path.Combine(Paths.StreamingAssetsPath, "logs.txt");

            if (File.Exists(FilePath))
                CreateFileStream();

            Reader = new LogPropertyReader(this);
            Writer = new LogPropertyWriter(this);
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
            Stream.Seek(0, SeekOrigin.Begin);
        }

        protected override void CreateFileStream()
        {
            //It is possible to redirect here by referencing resumeHandle. Unsure if that would be good behavior or not.
            if (WaitingToResume)
                throw new IOException("Attempt to create an interrupted filestream is not allowed");

            Stream = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.SequentialScan);
        }
    }
}
