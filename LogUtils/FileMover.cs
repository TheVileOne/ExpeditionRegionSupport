using LogUtils.Diagnostics;
using LogUtils.Enums.FileSystem;
using LogUtils.Helpers.FileHandling;
using System;
using System.IO;
using static LogUtils.UtilityConsts;

namespace LogUtils
{
    public class FileMover
    {
        private readonly string sourcePath, destPath;

        /// <summary>
        /// Move attempt will replace a file at the destination path when true; fail to move when false
        /// </summary>
        public bool ReplaceExistingFile { get; set; } = true;

        public bool AttemptCopyOnFailure { get; set; } = true;

        /// <summary>
        /// Indicate whether files with unsupported file extensions should be handled
        /// </summary>
        public bool EnforceSupportedFileTypes { get; set; }

        private FileExceptionHandler _exceptionHandler;
        /// <summary>
        /// Handles exceptions caught while handling file system operations through this instance
        /// </summary>
        public FileExceptionHandler ExceptionHandler
        {
            get
            {
                if (_exceptionHandler == null)
                    _exceptionHandler = CreateExceptionHandler();
                return _exceptionHandler;
            }
            set => _exceptionHandler = value;
        }

        /// <summary>
        /// Creates an object capable of moving, or copying files to a new destination
        /// </summary>
        /// <param name="sourcePath">The full path to the file that needs to be moved (including filename + ext)</param>
        /// <param name="destPath">The full path to the destination of the file. Filename is optional.</param>
        public FileMover(string sourcePath, string destPath)
        {
            this.sourcePath = sourcePath;
            this.destPath = destPath;
        }

        /// <summary>
        /// Moves a file from one place to another. Allows file renaming.
        /// </summary>
        public virtual FileStatus MoveFile()
        {
            FilePathValidator validator = CreateValidator();

            if (validator.Validate())
            {
                FileStatus status;
                try
                {
                    status = PrepareToMoveFile(validator);

                    if (status == FileStatus.MoveRequired)
                    {
                        FileInfo sourceFilePath = validator.SourceFile;
                        FileInfo destFilePath = validator.DestinationFile;

                        sourceFilePath.MoveTo(destFilePath.FullName);
                        return FileStatus.MoveComplete;
                    }
                }
                catch (Exception ex)
                {
                    ex.Data[ExceptionDataKey.SOURCE_PATH] = sourcePath;
                    ex.Data[ExceptionDataKey.DESTINATION_PATH] = destPath;

                    SetExceptionHandlerContext(ErrorContext.Move);
                    ExceptionHandler.OnError(ex);

                    status = FileStatus.Error;
                    if (AttemptCopyOnFailure)
                        status = CopyFile(validator);
                }
                return status;
            }

            Exception validationException = validator.GetLastException();
            if (validationException != null)
            {
                SetExceptionHandlerContext(ErrorContext.Move);
                ExceptionHandler.OnError(validationException, "Validation error occurred");
            }
            return FileStatus.ValidationFailed;
        }

        /// <summary>
        /// Copies a file from one place to another. Allows file renaming.
        /// </summary>
        public virtual FileStatus CopyFile()
        {
            FilePathValidator validator = CreateValidator();

            if (validator.Validate())
            {
                FileStatus status;
                try
                {
                    status = PrepareToMoveFile(validator);

                    if (status == FileStatus.MoveRequired)
                        return CopyFile(validator);
                }
                catch (Exception ex)
                {
                    ex.Data[ExceptionDataKey.SOURCE_PATH] = sourcePath;
                    ex.Data[ExceptionDataKey.DESTINATION_PATH] = destPath;

                    SetExceptionHandlerContext(ErrorContext.Copy);
                    ExceptionHandler.OnError(ex);

                    status = FileStatus.Error;
                }
                return status;
            }

            Exception validationException = validator.GetLastException();
            if (validationException != null)
            {
                SetExceptionHandlerContext(ErrorContext.Copy);
                ExceptionHandler.OnError(validationException, "Validation error occurred");
            }
            return FileStatus.ValidationFailed;
        }

        internal FileStatus CopyFile(FilePathValidator validator)
        {
            FileInfo sourceFilePath = validator.SourceFile;
            FileInfo destFilePath = validator.DestinationFile;

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

                SetExceptionHandlerContext(ErrorContext.Copy);
                ExceptionHandler.OnError(ex);

                status = FileStatus.Error;
            }
            return status;
        }

        /// <summary>
        /// Handles file system operations that are necessary before a move/copy operation can be possible
        /// </summary>
        internal FileStatus PrepareToMoveFile(FilePathValidator validator)
        {
            FileInfo sourceFilePath = validator.SourceFile;
            FileInfo destFilePath = validator.DestinationFile;

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

        protected virtual FilePathValidator CreateValidator()
        {
            return new FilePathValidator(sourcePath, destPath)
            {
                EnforceSupportedFileTypes = EnforceSupportedFileTypes
            };
        }

        protected virtual FileExceptionHandler CreateExceptionHandler()
        {
            return new FileMoveExceptionHandler();
        }

        protected void SetExceptionHandlerContext(ErrorContext context)
        {
            var handler = ExceptionHandler;
            switch (context)
            {
                case ErrorContext.Move:
                    handler.BeginContext(ActionType.Move);
                    break;
                case ErrorContext.Copy:
                    handler.BeginContext(ActionType.Copy);
                    break;
            }
        }

        protected enum ErrorContext
        {
            Move,
            Copy,
            Validation,
        }
    }

    internal sealed class FileMoveExceptionHandler : FileExceptionHandler
    {
        protected override string CreateErrorMessage(ExceptionContextWrapper contextWrapper, ref bool includeStackTrace)
        {
            if (contextWrapper.CustomMessage != null) //Custom error message always overrides default provided message formatting
                return contextWrapper.CustomMessage;

            string message = null;
            if (contextWrapper.IsExceptionContext && contextWrapper.Context == ActionType.Move)
            {
                message = "Unable to move file. Attempting to copy instead";
            }

            if (message != null)
                return message;

            return base.CreateErrorMessage(contextWrapper, ref includeStackTrace);
        }
    }
}
