using LogUtils.Diagnostics;
using LogUtils.Enums;
using LogUtils.Helpers.FileHandling;
using LogUtils.Threading;
using System.Collections.Generic;
using System.IO;

namespace LogUtils.Helpers
{
    public static class LogGroup
    {
        public static void DeleteFolder(LogGroupID group)
        {
        }

        public static void MoveFolder(LogGroupID group, string newPath)
        {
        }

        /// <summary>
        /// Process for moving a directory containing log files - assumes folder path is valid, and log files are located within the folder
        /// </summary>
        internal static void MoveFolder(IEnumerable<LogID> logFilesInFolder, string currentPath, string newPath)
        {
            ThreadSafeWorker worker = new ThreadSafeWorker(logFilesInFolder.GetLocks());

            worker.DoWork(() =>
            {
                bool moveCompleted = false;

                List<MessageBuffer> activeBuffers = new List<MessageBuffer>();
                List<StreamResumer> streamsToResume = new List<StreamResumer>();
                try
                {
                    UtilityCore.RequestHandler.BeginCriticalSection();
                    foreach (LogID logFile in logFilesInFolder)
                    {
                        MessageBuffer writeBuffer = logFile.Properties.WriteBuffer;

                        writeBuffer.SetState(true, BufferContext.CriticalArea);
                        activeBuffers.Add(writeBuffer);

                        logFile.Properties.FileLock.SetActivity(FileAction.Move); //Lock activated by ThreadSafeWorker
                        logFile.Properties.NotifyPendingMove(newPath);

                        //The move operation requires that all persistent file activity be closed until move is complete
                        streamsToResume.AddRange(logFile.Properties.PersistentStreamHandles.InterruptAll());
                    }
                    Directory.Move(currentPath, newPath);
                    moveCompleted = true;

                    //Update path info for affected log files
                    foreach (LogID logFile in logFilesInFolder)
                    {
                        //TODO: This is wrong and doesn't account for subfolders
                        logFile.Properties.ChangePath(newPath);
                    }
                }
                finally
                {
                    if (!moveCompleted)
                    {
                        foreach (LogID logFile in logFilesInFolder)
                            logFile.Properties.NotifyPendingMoveAborted();
                    }

                    //Reopen the streams
                    streamsToResume.ResumeAll();
                    activeBuffers.ForEach(buffer => buffer.SetState(false, BufferContext.CriticalArea));
                    UtilityCore.RequestHandler.EndCriticalSection();
                }
            });
        }

        public static void MoveFiles(LogGroupID group, string newPath)
        {
        }

        /// <summary>
        /// Example showing how API can be used by a mod to move their group folder, or its contents around
        /// </summary>
        public static void MoveFolderExample()
        {
            LogGroupID myGroupID = null;
            bool hasTriedToMoveFolder = false;
        retry:
            try
            {
                //Define a new group path
                string folderName = Path.GetFileName(myGroupID.Properties.CurrentFolderPath);
                string newFolderPath = Path.Combine("new path", folderName);

                //Attempt to move entire folder, and if it fails, attempt to move only the files instead
                if (!hasTriedToMoveFolder)
                {
                    MoveFolder(myGroupID, newFolderPath);
                }
                else
                {
                    MoveFiles(myGroupID, newFolderPath);
                }
                //Confirm that we have a new path
                Assert.That(PathUtils.PathsAreEqual(newFolderPath, myGroupID.Properties.CurrentFolderPath));
            }
            catch (IOException)
            {
                if (!hasTriedToMoveFolder) //Ignore if this fails twice, but actual handling procesures may differ
                {
                    hasTriedToMoveFolder = true;
                    goto retry;
                }
            }
        }
    }
}
