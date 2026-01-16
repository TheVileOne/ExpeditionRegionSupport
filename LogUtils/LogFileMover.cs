using LogUtils.Diagnostics;
using LogUtils.Enums;
using LogUtils.Helpers.FileHandling;
using System;
using System.IO;

namespace LogUtils
{
    public class LogFileMover
    {
        private readonly string sourcePath, destPath;

        /// <summary>
        /// Move attempt will replace a file at the destination path when true; fail to move when false
        /// </summary>
        public bool ReplaceExistingFile = true;

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
                    var handler = CreateExceptionHandler();

                    handler.OnError(ex, ErrorContext.Move);
                    status = CopyFile(logValidator);
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
                        return CopyFile(logValidator);
                }
                catch (Exception ex)
                {
                    var handler = CreateExceptionHandler();

                    handler.OnError(ex, ErrorContext.Copy);
                    status = FileStatus.Error;
                }

                return status;
            }

            return FileStatus.ValidationFailed;
        }

        internal FileStatus CopyFile(LogValidator logValidator)
        {
            FileInfo sourceFilePath = logValidator.SourceFile;
            FileInfo destFilePath = logValidator.DestinationFile;

            FileStatus status;
            try
            {
                sourceFilePath.CopyTo(destFilePath.FullName, ReplaceExistingFile);
                status = FileStatus.CopyComplete;
            }
            catch (Exception ex)
            {
                var handler = CreateExceptionHandler();

                handler.OnError(ex, ErrorContext.Copy);
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

            if (!sourceFilePath.Exists)
            {
                UtilityLogger.DebugLog("Source file not present");
                return FileStatus.NoActionRequired;
            }

            //Files are in the same folder
            if (sourceFileDir.FullName == destFileDir.FullName)
            {
                string sourceFilename = sourceFilePath.Name;
                string destFilename = destFilePath.Name;

                if (FileExtension.Match(sourceFilename, destFilename) && sourceFilename == destFilename)
                    return FileStatus.NoActionRequired; //Same file, no copy necessary

                if (!ReplaceExistingFile && destFilePath.Exists)
                    return FileStatus.FileAlreadyExists;

                destFilePath.Delete(); //Move will fail if a file already exists
            }
            else if (destFileDir.Exists)
            {
                if (!ReplaceExistingFile && destFilePath.Exists)
                    return FileStatus.FileAlreadyExists;

                destFilePath.Delete();
            }
            else
            {
                destFileDir.Create(); //Make sure the directory exists at the destination
            }

            return FileStatus.MoveRequired;
        }

        protected virtual ExceptionHandler CreateExceptionHandler()
        {
            return new LogFileMoverExceptionHandler();
        }

        internal sealed class LogFileMoverExceptionHandler : ExceptionHandler
        {
            protected override void LogError(Exception exception)
            {
                ErrorContext value = (ErrorContext)exception.Data["Context"];

                string message = getErrorMessage(value);
                UtilityLogger.LogError(message, exception);
            }

            private string getErrorMessage(ErrorContext context)
            {
                if (context == ErrorContext.Move)
                    return "Unable to move file. Attempting to copy instead";
                else if (context == ErrorContext.Copy)
                    return "Unable to copy file";
                return null;
            }
        }

        private enum ErrorContext
        {
            Move,
            Copy
        }
    }

    internal delegate LogFileMover LogFileMoverProvider(string sourceLogPath, string destLogPath);
}
