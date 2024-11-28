﻿using LogUtils.Enums;
using LogUtils.Helpers;
using System;

namespace LogUtils
{
    public class PersistentLogFileHandle : PersistentFileHandle, IDisposable
    {
        public readonly LogID FileID;

        public PersistentLogFileHandle(LogID logFile) : base()
        {
            FileID = logFile;

            //ComparisonLogIDs are unsupported
            if (FileID.Properties == null)
                throw new ArgumentException("LogProperties instance must not be null");

            CreateFileStream();
            FileID.Properties.PersistentStreamHandles.Add(this);
        }

        protected override void CreateFileStream()
        {
            WaitingToResume = false;
            Stream = LogFile.Open(FileID);
        }

        public override void Dispose()
        {
            if (IsDisposed) return;

            FileLock fileLock = FileID.Properties.FileLock;

            //Locked to avoid interfering with any write operations
            lock (fileLock)
            {
                fileLock.SetActivity(FileID, FileAction.StreamDisposal);
                base.Dispose();
                FileID.Properties.PersistentStreamHandles.Remove(this);
            }
        }
    }
}
