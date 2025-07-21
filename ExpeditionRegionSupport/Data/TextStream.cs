﻿using System;
using System.Collections.Generic;
using System.IO;

namespace ExpeditionRegionSupport.Data
{
    /// <summary>
    /// This custom StreamReader class implementes common line format processing and validation, and provides a way for custom handling of line data
    /// </summary>
    public class TextStream : StreamReader, IDisposable
    {
        /// <summary>
        /// The file stream source
        /// </summary>
        public string Filepath { get; }

        /// <summary>
        /// Apply modifications to the text data as it is read from file
        /// </summary>
        public StringDelegates.Format LineFormatter { get; set; }

        /// <summary>
        /// Apply conditions that pass over text data that matches certain criteria
        /// </summary>
        public List<StringDelegates.Validate> SkipConditions = new List<StringDelegates.Validate>();

        /// <summary>
        /// When set to true, stream will not close on disposal. Instance will need to be disposed externally if this is set to false
        /// </summary>
        public bool AllowStreamDisposal;

        public bool IsDisposed { get; private set; }

        public event Action<TextStream> OnDisposed;

        public Action<TextStream> OnStreamEnd;

        public TextStream(string file, bool allowDisposal) : base(file)
        {
            Filepath = file;
            AllowStreamDisposal = allowDisposal;
            LineFormatter = new StringDelegates.Format(s => s.Trim());
            SkipConditions.Add(new StringDelegates.Validate(s => s == string.Empty || s.StartsWith("//")));
        }

        public override string ReadLine()
        {
            if (IsDisposed) return null;

            string line = base.ReadLine();

            if (line == null)
            {
                OnStreamEnd?.Invoke(this);
                Close();
                return null;
            }
            return LineFormatter.Invoke(line);
        }

        /// <summary>
        /// Reads from file one line at a time
        /// </summary>
        public CachedEnumerable<string> ReadLines()
        {
            return new CachedEnumerable<string>(ReadLinesIterator());
        }

        /// <summary>
        /// Reads all lines from file returning them in an array
        /// </summary>
        public string[] ReadAllLines()
        {
            //Discard any buffer data, and start at beginning of file
            DiscardBufferedData();
            BaseStream.Seek(0, SeekOrigin.Begin);

            List<string> fileData = new List<string>();
            string line;
            while ((line = ReadLine()) != null)
            {
                if (!SkipConditions.Exists(hasFailedCheck => hasFailedCheck(line))) //Checks that line conforms with all given skip conditions)
                    fileData.Add(line);
            }
            return fileData.ToArray();
        }

        internal IEnumerable<string> ReadLinesIterator()
        {
            string line;
            do
            {
                line = ReadLine();

                if (line == null)
                    yield break;

                if (SkipConditions.Exists(hasFailedCheck => hasFailedCheck(line))) //Checks that line conforms with all given skip conditions
                    continue;

                yield return line;
            }
            while (line != null);
        }

        protected override void Dispose(bool disposing)
        {
            if (!AllowStreamDisposal) return;

            try
            {
                base.Dispose(disposing);

                if (!IsDisposed)
                {
                    IsDisposed = true;
                    OnDisposed?.Invoke(this);
                }
            }
            catch (IOException ex)
            {
                if (Plugin.DebugMode)
                {
                    Plugin.Logger.LogError(ex);
                    Plugin.Logger.LogError(ex.StackTrace);
                }
            }
        }
    }
}
