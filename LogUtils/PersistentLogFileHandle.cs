using LogUtils.Enums;
using LogUtils.Helpers;
using System;
using System.IO;
using UnityEngine;

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

        public override StreamResumer InterruptStream()
        {
            var fileLock = FileID.Properties.FileLock;

            using (fileLock.Acquire())
            {
                if (!WaitingToResume)
                    fileLock.SetActivity(FileID, FileAction.StreamDisposal);
                return base.InterruptStream();
            }
        }

        protected override void NotifyOnInterrupt()
        {
            string reportMessage = $"Interrupting filestream {FileID}";

            //Avoid notifying this event for this particular log file while handling a critical section involving its filestream
            if (FileID != LogID.BepInEx)
                UtilityLogger.Log(reportMessage);

            //Also report this to the debug log
            UtilityLogger.DebugLog(reportMessage);
        }

        protected override void NotifyOnResume()
        {
            string reportMessage = $"Resuming filestream {FileID}";

            //Avoid notifying this event for this particular log file while handling a critical section involving its filestream
            if (FileID != LogID.BepInEx)
                UtilityLogger.Log(reportMessage);

            //Also report this to the debug log
            UtilityLogger.DebugLog(reportMessage);
        }

        protected override void CreateFileStream()
        {
            try
            {
                //It is possible to redirect here by referencing resumeHandle. Unsure if that would be good behavior or not.
                if (WaitingToResume)
                    throw new IOException("Attempt to create an interrupted filestream is not allowed");

                Stream = LogFile.Open(FileID);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                UtilityLogger.DebugLog(ex);
            }
        }

        protected override void Dispose(bool disposing)
        {
            var fileLock = FileID.Properties.FileLock;

            //Locked to avoid interfering with any write operations
            using (fileLock.Acquire())
            {
                if (IsDisposed) return;

                if (disposing)
                    OnDispose();

                fileLock.SetActivity(FileID, FileAction.StreamDisposal);
                base.Dispose(disposing);

                if (disposing)
                    FileID.Properties.PersistentStreamHandles.Remove(this);
            }
        }
    }
}
