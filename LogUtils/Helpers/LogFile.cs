using LogUtils.Enums;
using LogUtils.Helpers.FileHandling;
using LogUtils.Requests;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LogUtils.Helpers
{
    /// <summary>
    /// Contains helper methods for basic file operations involving log files
    /// </summary>
    public static class LogFile
    {
        /// <summary>
        /// Creates a copy of a log file
        /// </summary>
        /// <param name="logFile">The LogID that accesses the log file path</param>
        /// <param name="copyPath">The full path to the destination of the log file. Log filename is optional</param>
        public static FileStatus Copy(LogID logFile, string copyPath)
        {
            return Copy(logFile.Properties.CurrentFilePath, copyPath, false);
        }

        /// <summary>
        /// Creates a copy of a log file
        /// </summary>
        /// <param name="sourceLogPath">The full path to the log file that needs to be copied (including filename + ext)</param>
        /// <param name="destLogPath">The full path to the destination of the log file. Log filename is optional</param>
        /// <param name="overwriteExisting">Specifies the behavior that happens when the file already exists at the destination path</param>
        internal static FileStatus Copy(string sourceLogPath, string destLogPath, bool overwriteExisting)
        {
            LogFileMover fileMover = new LogFileMover(sourceLogPath, destLogPath)
            {
                ReplaceExistingFile = overwriteExisting
            };

            return fileMover.CopyFile();
        }

        public static FileStatus Move(LogID logFile, string newLogPath)
        {
            return InternalMove(logFile, newLogPath);
        }

        public static FileStatus Move(LogID logFile, string newLogPath, string logFilename)
        {
            return InternalMove(logFile, Path.Combine(newLogPath, logFilename));
        }

        internal static FileStatus InternalMove(LogID logFile, string newLogPath)
        {
            var fileLock = logFile.Properties.FileLock;

            using (fileLock.Acquire())
            {
                UtilityLogger.Log($"Attempting to move {logFile} to {newLogPath}");

                if (!logFile.Properties.FileExists)
                {
                    logFile.Properties.ChangePath(newLogPath);
                    return FileStatus.NoActionRequired;
                }

                fileLock.SetActivity(FileAction.Move);

                //The move operation requires that all persistent file activity be closed until move is complete
                var streamsToResume = logFile.Properties.PersistentStreamHandles.InterruptAll();

                logFile.Properties.WriteBuffer.SetState(true, BufferContext.CriticalArea);

                FileStatus moveResult = Move(logFile.Properties.CurrentFilePath, newLogPath, false);

                if (moveResult == FileStatus.FileAlreadyExists)
                {
                    string lastFilePath = logFile.Properties.CurrentFilePath;

                    //Attempt to resolve the conflict - This will change the filename if conflicting log file is accesible to LogUtils
                    logFile.Properties.ChangePath(newLogPath);

                    moveResult = Move(lastFilePath, newLogPath, false);

                    if (moveResult == FileStatus.FileAlreadyExists)
                    {
                        UtilityLogger.LogWarning($"Path conflict exists: Deleting file at target destination");
                        UtilityLogger.LogWarning($"Path: {newLogPath}");

                        //Last resort effort to move the file - if this fails, we must abort the move
                        moveResult = Move(lastFilePath, newLogPath, true);

                        if (moveResult != FileStatus.MoveComplete)
                        {
                            UtilityLogger.LogWarning($"Failed to move file");

                            //We have no choice, but to restore the original filename and path
                            logFile.Properties.ChangePath(lastFilePath);
                        }
                    }
                }
                else if (moveResult == FileStatus.MoveComplete)
                {
                    logFile.Properties.ChangePath(newLogPath);
                }

                logFile.Properties.WriteBuffer.SetState(false, BufferContext.CriticalArea);
                streamsToResume.ResumeAll();
                return moveResult;
            }
        }

        /// <summary>
        /// Moves a log file from one place to another. Allows file renaming
        /// </summary>
        /// <param name="sourceLogPath">The full path to the log file that needs to be moved (including filename + ext)</param>
        /// <param name="destLogPath">The full path to the destination of the log file. Log filename is optional</param>
        /// <param name="overwriteExisting">Specifies the behavior that happens when the file already exists at the destination path</param>
        internal static FileStatus Move(string sourceLogPath, string destLogPath, bool overwriteExisting)
        {
            LogFileMover fileMover = new LogFileMover(sourceLogPath, destLogPath)
            {
                ReplaceExistingFile = overwriteExisting
            };

            return fileMover.MoveFile();
        }

        /// <summary>
        /// Opens a FileStream instance for a log file
        /// </summary>
        /// <param name="logFile">The LogID that accesses the log file path</param>
        /// <returns>The opened FileStream, or null if the file could not be opened, or created</returns>
        /// <exception cref="IOException"></exception>
        public static FileStream Open(LogID logFile)
        {
            var fileLock = logFile.Properties.FileLock;

            using (fileLock.Acquire())
            {
                fileLock.SetActivity(FileAction.Open);
                bool retryAttempt = false;

            retry:
                FileStream stream = OpenNoCreate(logFile);

                if (stream == null)
                {
                    if (!retryAttempt)
                    {
                        logFile.Properties.FileExists = false;

                        //Try to create the file, and reattempt to open stream
                        if (TryCreate(logFile))
                        {
                            retryAttempt = true;
                            goto retry;
                        }
                    }
                    throw new IOException("Unable to create log file");
                }
                return stream;
            }
        }

        internal static FileStream OpenNoCreate(LogID logFile)
        {
            return InternalOpen(logFile, FileMode.Open);
        }

        internal static FileStream InternalOpen(LogID logFile, FileMode mode)
        {
            try
            {
                FileAccess access = FileAccess.ReadWrite;
                if (!UtilityCore.IsControllingAssembly)
                {
                    mode = FileMode.Open;
                    access = FileAccess.Read;
                }

                //Open filestream using maximal FileShare privileges
                FileStream stream = File.Open(logFile.Properties.CurrentFilePath, mode, access, FileShare.ReadWrite);

                //Seeks to the end of the file - I don't know of a better way of handling this. Other methods that append to the file also create the file
                //for us. It is important for the utility to handle creating the file using its own process
                stream.Seek(0, SeekOrigin.End);
                return stream;
            }
            catch (IOException ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException)
            {
                return null;
            }
        }

        internal static FileStream Create(LogID logFile)
        {
            return InternalOpen(logFile, FileMode.OpenOrCreate);
        }

        /// <summary>
        /// Starts a log file session creating the log file if it doesn't exist
        /// </summary>
        /// <param name="logFile">The LogID that accesses the log file path</param>
        /// <returns>The active state of the log session</returns>
        public static bool TryCreate(LogID logFile)
        {
            //Check access state to prevent log file from being created too early
            if (logFile.Properties.CanBeAccessed)
                logFile.Properties.BeginLogSession();

            return logFile.Properties.FileExists;
        }

        /// <summary>
        /// Ends the current logging session, and starts a new one if allowed to do so 
        /// </summary>
        public static void StartNewSession(LogID logFile)
        {
            if (!UtilityCore.IsControllingAssembly)
            {
                UtilityLogger.DebugLog("Replacing log file from alternate Rain World processes is unsupported");
                return;
            }

            logFile.Properties.EndLogSession();

            var streamsToResume = logFile.Properties.PersistentStreamHandles.InterruptAll();
            var fileLock = logFile.Properties.FileLock;

            using (fileLock.Acquire())
            {
                fileLock.SetActivity(FileAction.Delete);

                if (logFile.Properties.FileExists)
                {
                    bool fileRemoved = FileUtils.TryDelete(logFile.Properties.CurrentFilePath, "Unable to delete log file");

                    if (fileRemoved)
                        logFile.Properties.FileExists = false;
                }

                logFile.Properties.BeginLogSession();

                if (logFile.Properties.FileExists)
                    streamsToResume.ResumeAll();
                else
                {
                    string reportMessage = $"Unable to start {logFile} log";

                    //Cannot log to a file that doesn't exist
                    if (logFile != LogID.BepInEx)
                        UtilityLogger.LogWarning(reportMessage);
                    Debug.LogWarning(reportMessage);

                    reportMessage = "Disposing handle";

                    //There does not seem to be a point to keep an invalid handle around, dispose any handles for this log file
                    var handlesToDispose = logFile.Properties.PersistentStreamHandles.ToArray();

                    foreach (var handle in handlesToDispose)
                    {
                        //Cannot log to a file that doesn't exist
                        if (logFile != LogID.BepInEx)
                            UtilityLogger.LogWarning(reportMessage);
                        Debug.LogWarning(reportMessage);
                        handle.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Finds a path containing this filename with an unknown file extension
        /// </summary>
        public static string FindExistingPath(string searchPath, string filename)
        {
            //TODO: Can we use Directory search wildcards for this?
            foreach (string fileExt in FileExtension.SupportedExtensions)
            {
                string foundPath = Path.Combine(searchPath, filename + fileExt);

                if (File.Exists(foundPath))
                    return foundPath;
            }
            return null;
        }

        public static ILogWriter FindWriter(LogID logFile)
        {
            var writer = UtilityCore.RequestHandler.GameLogger.GetWriter(logFile);

            if (writer == null)
            {
                //There is no specified RequestType to base this search on, so we retrieve all compatible examples using RequestType.Local
                IEnumerable<ILogHandler> availableHandlers = UtilityCore.RequestHandler.AvailableLoggers.CompatibleWith(logFile, RequestType.Local);

                writer = availableHandlers.GetWriters(logFile).FirstOrDefault();
            }
            return writer;
        }

        /// <summary>
        /// Retrieves all file handles for log files with at least one persistent FileStream open (file is currently in use), or temporarily closed
        /// </summary>
        public static IEnumerable<PersistentLogFileHandle> GetPersistentLogFiles()
        {
            return UtilityCore.PersistenceManager.References.OfType<PersistentLogFileHandle>();
        }
    }
}
