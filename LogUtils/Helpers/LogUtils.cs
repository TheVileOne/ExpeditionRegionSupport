using System.IO;

namespace LogUtils.Helpers
{
    public static class LogUtils
    {

        /// <summary>
        /// Creates a copy of a log file
        /// </summary>
        /// <param name="logFile">The LogID giving access to the filepath of the log file</param>
        /// <param name="copyPath">The full path to the destination of the log file. Log filename is optional</param>
        public static FileStatus CopyLog(LogID logFile, string copyPath)
        {
            return CopyLog(logFile.Properties.CurrentFilename, copyPath);
        }

        /// <summary>
        /// Creates a copy of a log file
        /// </summary>
        /// <param name="sourceLogPath">The full path to the log file that needs to be copied (including filename + ext)</param>
        /// <param name="destLogPath">The full path to the destination of the log file. Log filename is optional</param>
        public static FileStatus CopyLog(string sourceLogPath, string destLogPath)
        {
            LogFileMover fileMover = new LogFileMover(sourceLogPath, destLogPath);

            return fileMover.CopyFile();
        }

        public static FileStatus MoveLog(LogID logFile, string newLogPath)
        {
            FileStatus moveResult = MoveLog(logFile.Properties.CurrentFilePath, newLogPath);

            if (moveResult == FileStatus.MoveComplete)
                logFile.Properties.ChangePath(newLogPath);
            return moveResult;
        }

        public static FileStatus MoveLog(LogID logFile, string newLogPath, string logFilename)
        {
            return MoveLog(logFile.Properties.CurrentFilePath, Path.Combine(newLogPath, logFilename));
        }

        /// <summary>
        /// Moves a log file from one place to another. Allows file renaming
        /// </summary>
        /// <param name="sourceLogPath">The full path to the log file that needs to be moved (including filename + ext)</param>
        /// <param name="destLogPath">The full path to the destination of the log file. Log filename is optional</param>
        public static FileStatus MoveLog(string sourceLogPath, string destLogPath)
        {
            LogFileMover fileMover = new LogFileMover(sourceLogPath, destLogPath);

            return fileMover.MoveFile();
        }
    }
}
