using LogUtils.Enums;
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
        /// <param name="logFile">The LogID giving access to the filepath of the log file</param>
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

        public static string FindPathWithoutFileExtension(string searchPath, string filename)
        {
            return FileUtils.SupportedExtensions.Select(fileExt => Path.Combine(searchPath, filename + fileExt)).FirstOrDefault(File.Exists);
        }
    }
}
