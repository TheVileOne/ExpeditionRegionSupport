using LogUtils.Enums;
using LogUtils.Enums.FileSystem;
using LogUtils.Helpers;
using System;
using System.IO;
using UnityEngine;

namespace LogUtils
{
    public class PersistentLogFileHandle : PersistentFileHandle, IDisposable
    {
        /// <summary>
        /// Identifies the target of which the persistent <see cref="FileStream"/> is managed
        /// </summary>
        public readonly LogID FileID;

        public PersistentLogFileHandle(LogID logFile) : base()
        {
            FileID = logFile;

            //ComparisonLogIDs are unsupported
            if (FileID.Properties == null)
                throw new ArgumentException("LogProperties instance must not be null", nameof(logFile));

            CreateFileStream();
            FileID.Properties.PersistentStreamHandles.Add(this);
        }

        /// <inheritdoc/>
        public override StreamResumer InterruptStream()
        {
            var fileLock = FileID.Properties.FileLock;

            using (fileLock.Acquire())
            {
                if (!WaitingToResume)
                    fileLock.SetActivity(FileAction.StreamDisposal);
                return base.InterruptStream();
            }
        }

        /// <inheritdoc/>
        protected override void NotifyOnInterrupt()
        {
            string reportMessage = $"Interrupting filestream {FileID}";

            //Avoid notifying this event for this particular log file while handling a critical section involving its filestream
            if (FileID != LogID.BepInEx)
                UtilityLogger.Log(reportMessage);

            //Also report this to the debug log
            UtilityLogger.DebugLog(reportMessage);
        }

        /// <inheritdoc/>
        protected override void NotifyOnResume()
        {
            string reportMessage = $"Resuming filestream {FileID}";

            //Avoid notifying this event for this particular log file while handling a critical section involving its filestream
            if (FileID != LogID.BepInEx)
                UtilityLogger.Log(reportMessage);

            //Also report this to the debug log
            UtilityLogger.DebugLog(reportMessage);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        protected override void BeginDispose(bool disposeState)
        {
            FileID.Properties.FileLock.Acquire(); //Released on EndDispose()
            base.BeginDispose(disposeState);
        }

        /// <inheritdoc/>
        protected override void EndDispose(bool disposeState)
        {
            var fileLock = FileID.Properties.FileLock;

            fileLock.SetActivity(FileAction.StreamDisposal);

            base.EndDispose(disposeState); //Safe to call without error handling
            if (disposeState)
                FileID.Properties.PersistentStreamHandles.Remove(this);

            fileLock.Release();
        }
    }
}
