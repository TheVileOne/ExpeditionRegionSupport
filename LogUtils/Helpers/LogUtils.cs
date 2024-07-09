using Expedition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LogUtils.Helpers
{
    public static class LogUtils
    {
        public static LogID GetLogID(string value)
        {
            LogID found = null;
            if (ExtEnumBase.TryParse(typeof(LogID), value, true, out ExtEnumBase extBase))
                found = (LogID)extBase;
            return found;
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
        /// Moves a log file from one place to another. Allows file renaming.
        /// </summary>
        /// <param name="sourceLogPath">The full path to the log file that needs to be moved (including filename + ext)</param>
        /// <param name="destLogPath">The full path to the destination of the log file. Log filename is optional.</param>
        public static FileStatus MoveLog(string sourceLogPath, string destLogPath)
        {
            LogFileMover fileMover = new LogFileMover(sourceLogPath, destLogPath);

            return fileMover.MoveFile();
        }
    }
}
