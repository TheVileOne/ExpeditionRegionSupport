﻿using LogUtils.Enums;
using LogUtils.Helpers.FileHandling;
using System.IO;
using System.Linq;

namespace LogUtils.Helpers
{
    public static class LogFile
    {

        /// <summary>
        /// Creates a copy of a log file
        /// </summary>
        /// <param name="logFile">The LogID that accesses the log file path</param>
        /// <param name="copyPath">The full path to the destination of the log file. Log filename is optional</param>
        public static FileStatus Copy(LogID logFile, string copyPath)
        {
            return Copy(logFile.Properties.CurrentFilename, copyPath);
        }

        /// <summary>
        /// Creates a copy of a log file
        /// </summary>
        /// <param name="sourceLogPath">The full path to the log file that needs to be copied (including filename + ext)</param>
        /// <param name="destLogPath">The full path to the destination of the log file. Log filename is optional</param>
        internal static FileStatus Copy(string sourceLogPath, string destLogPath)
        {
            LogFileMover fileMover = new LogFileMover(sourceLogPath, destLogPath);

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
            FileStatus moveResult;

            var fileLock = logFile.Properties.FileLock;
            lock (fileLock)
            {
                fileLock.SetActivity(logFile, FileAction.Move);
                moveResult = Move(logFile.Properties.CurrentFilePath, newLogPath);

                if (moveResult == FileStatus.MoveComplete)
                    logFile.Properties.ChangePath(newLogPath);
            }
            return moveResult;
        }

        /// <summary>
        /// Moves a log file from one place to another. Allows file renaming
        /// </summary>
        /// <param name="sourceLogPath">The full path to the log file that needs to be moved (including filename + ext)</param>
        /// <param name="destLogPath">The full path to the destination of the log file. Log filename is optional</param>
        internal static FileStatus Move(string sourceLogPath, string destLogPath)
        {
            //TODO: LogFileMover should support LogIDs
            LogFileMover fileMover = new LogFileMover(sourceLogPath, destLogPath);

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

            lock (fileLock)
            {
                fileLock.SetActivity(logFile, FileAction.Open);

                string writePath = logFile.Properties.CurrentFilePath;
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
                //Open filestream using maximal FileShare privileges
                FileStream stream = File.Open(logFile.Properties.CurrentFilePath, mode, FileAccess.ReadWrite, FileShare.ReadWrite);

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

        public static string FindPathWithoutFileExtension(string searchPath, string filename)
        {
            return FileUtils.SupportedExtensions.Select(fileExt => Path.Combine(searchPath, filename + fileExt)).FirstOrDefault(File.Exists);
        }
    }
}