using System;
using System.IO;
using UnityEngine;

namespace LogUtils
{
    public class LogFileMover
    {
        private string sourcePath, destPath;

        /// <summary>
        /// Creates an object capable of moving, or copying log files to a new destination
        /// </summary>
        /// <param name="sourceLogPath">The full path to the log file that needs to be moved (including filename + ext)</param>
        /// <param name="destLogPath">The full path to the destination of the log file. Log filename is optional.</param>
        public LogFileMover(string sourceLogPath, string destLogPath)
        {
            sourcePath = sourceLogPath;
            destPath = destLogPath;
        }

        /// <summary>
        /// Moves a log file from one place to another. Allows file renaming.
        /// </summary>
        public FileStatus MoveFile()
        {
            LogValidator logValidator = new LogValidator(sourcePath, destPath);

            if (logValidator.Validate())
            {
                FileStatus status;
                try
                {
                    status = PrepareToMoveFile(logValidator);

                    if (status == FileStatus.MoveRequired)
                    {
                        FileInfo sourceFilePath = logValidator.SourceFile;
                        FileInfo destFilePath = logValidator.DestinationFile;

                        sourceFilePath.MoveTo(destFilePath.FullName);
                        return FileStatus.MoveComplete;
                    }
                }
                catch (Exception ex)
                {
                    logMoveError(ex);
                    status = _CopyFile(logValidator);
                }

                return status;
            }

            return FileStatus.ValidationFailed;
        }

        /// <summary>
        /// Copies a log file from one place to another. Allows file renaming.
        /// </summary>
        public FileStatus CopyFile()
        {
            LogValidator logValidator = new LogValidator(sourcePath, destPath);

            if (logValidator.Validate())
            {
                FileStatus status;
                try
                {
                    status = PrepareToMoveFile(logValidator);

                    if (status == FileStatus.MoveRequired)
                        return _CopyFile(logValidator);
                }
                catch (Exception ex)
                {
                    logCopyError(ex);
                    status = FileStatus.Error;
                }

                return status;
            }

            return FileStatus.ValidationFailed;
        }

        internal FileStatus _CopyFile(LogValidator logValidator)
        {
            FileInfo sourceFilePath = logValidator.SourceFile;
            FileInfo destFilePath = logValidator.DestinationFile;

            FileStatus status;
            try
            {
                sourceFilePath.CopyTo(destFilePath.FullName, true);
                status = FileStatus.CopyComplete;
            }
            catch (Exception ex)
            {
                logCopyError(ex);
                status = FileStatus.Error;
            }

            return status;
        }

        /// <summary>
        /// Handles FileSystem operations that are necessary before a move/copy operation can be possible
        /// </summary>
        internal FileStatus PrepareToMoveFile(LogValidator logValidator)
        {
            FileInfo sourceFilePath = logValidator.SourceFile;
            FileInfo destFilePath = logValidator.DestinationFile;

            DirectoryInfo sourceFileDir = sourceFilePath.Directory;
            DirectoryInfo destFileDir = destFilePath.Directory;

            //Files are in the same folder
            if (sourceFileDir.FullName == destFileDir.FullName)
            {
                string sourceFilename = sourceFilePath.Name;
                string destFilename = destFilePath.Name;

                if (FileUtils.ExtensionsMatch(sourceFilename, destFilename) && sourceFilename == destFilename)
                    return FileStatus.NoActionRequired; //Same file, no copy necessary

                destFilePath.Delete(); //Move will fail if a file already exists
            }
            else if (destFileDir.Exists)
            {
                destFilePath.Delete();
            }
            else
            {
                destFileDir.Create(); //Make sure the directory exists at the destination
            }

            return FileStatus.MoveRequired;
        }

        private void logMoveError(Exception ex)
        {
            Debug.LogError("Unable to move file. Attempt copy instead");
            Debug.LogError(ex);
        }

        private void logCopyError(Exception ex)
        {
            Debug.LogError("Unable to copy file");
            Debug.LogError(ex);
        }
    }

    public enum FileStatus
    {
        AwaitingStatus,
        NoActionRequired,
        MoveRequired,
        MoveComplete,
        CopyComplete,
        ValidationFailed,
        Error
    }
}
