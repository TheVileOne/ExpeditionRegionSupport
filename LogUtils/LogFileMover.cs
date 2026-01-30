using LogUtils.Diagnostics;
using LogUtils.Enums.FileSystem;
using LogUtils.Helpers.FileHandling;
using System;
using System.IO;
using ExceptionDataKey = LogUtils.UtilityConsts.ExceptionDataKey;

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
                    ex.Data[ExceptionDataKey.SOURCE_PATH] = sourcePath;
                    ex.Data[ExceptionDataKey.DESTINATION_PATH] = destPath;

                    var handler = CreateExceptionHandler(ErrorContext.Move);
                    handler.OnError(ex);

                    status = CopyFile(logValidator);
                }

                return status;
            }

            Exception validationException = logValidator.GetLastException();
            if (validationException != null)
            {
                var handler = CreateExceptionHandler(ErrorContext.Move);
                handler.OnError(validationException, "Validation error occurred");
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
                    ex.Data[ExceptionDataKey.SOURCE_PATH] = sourcePath;
                    ex.Data[ExceptionDataKey.DESTINATION_PATH] = destPath;

                    var handler = CreateExceptionHandler(ErrorContext.Copy);
                    handler.OnError(ex);

                    status = FileStatus.Error;
                }
                return status;
            }

            Exception validationException = logValidator.GetLastException();
            if (validationException != null)
            {
                var handler = CreateExceptionHandler(ErrorContext.Copy);
                handler.OnError(validationException, "Validation error occurred");
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
                ex.Data[ExceptionDataKey.SOURCE_PATH] = sourcePath;
                ex.Data[ExceptionDataKey.DESTINATION_PATH] = destPath;

                var handler = CreateExceptionHandler(ErrorContext.Copy);
                handler.OnError(ex);

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

        protected virtual FileExceptionHandler CreateExceptionHandler(ErrorContext context)
        {
            var handler = new LogFileMoverExceptionHandler();

            switch (context)
            {
                case ErrorContext.Move:
                    handler.BeginContext(ActionType.Move);
                    break;
                case ErrorContext.Copy:
                    handler.BeginContext(ActionType.Copy);
                    break;
            }
            return handler;
        }

        internal sealed class LogFileMoverExceptionHandler : FileExceptionHandler
        {
            protected override string CreateErrorMessage(ExceptionContextWrapper contextWrapper, ref bool includeStackTrace)
            {
                if (contextWrapper.CustomMessage != null) //Custom error message always overrides default provided message formatting
                    return contextWrapper.CustomMessage;

                string message = null;
                if (contextWrapper.IsExceptionContext)
                {
                    if (contextWrapper.Context == ActionType.Move)
                        message = "Unable to move file. Attempting to copy instead";
                }

                if (message != null)
                    return message;

                return base.CreateErrorMessage(contextWrapper, ref includeStackTrace);
            }
        }

        protected enum ErrorContext
        {
            Move,
            Copy,
            Validation,
        }
    }

    internal delegate LogFileMover LogFileMoverProvider(string sourceLogPath, string destLogPath);
}
