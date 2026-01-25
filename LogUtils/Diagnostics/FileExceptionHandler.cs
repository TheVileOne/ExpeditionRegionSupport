using LogUtils.Enums.FileSystem;
using System;
using System.IO;

namespace LogUtils.Diagnostics
{
    public sealed class FileExceptionHandler : FileSystemExceptionHandler
    {
        /// <inheritdoc/>
        protected override bool IsFileContext => true;

        public FileExceptionHandler(string sourceName) : base(sourceName)
        {
        }

        public FileExceptionHandler(string sourceName, FileAction context) : base(sourceName, new ActionType(context))
        {
        }

        public FileExceptionHandler(string sourceName, ActionType context) : base(sourceName, context)
        {
        }

        /// <inheritdoc/>
        protected override bool CanRecoverFrom(Exception ex)
        {
            //TODO: Determine if this should remain file context exclusive
            if (ex is FileNotFoundException && (Context == ActionType.Move || Context == ActionType.Copy)) //In context this refers to the source file
                return false;
            return true;
        }

        /// <inheritdoc/>
        protected override void LogError(Exception exception)
        {
            if (CustomMessage != null) //Base handler is designed to handle this
            {
                base.LogError(exception);
                return;
            }

            if (exception is FileNotFoundException)
            {
                string descriptor = GetDescriptor();
                string errorMessage = descriptor + " could not be found";

                if (Context == ActionType.Move || Context == ActionType.Copy)
                {
                    UtilityLogger.LogError(errorMessage); //Stack trace is not logged in this case
                    return;
                }
            }
            base.LogError(exception);
        }

        /// <inheritdoc/>
        protected override string GetSimpleDescriptor()
        {
            return "file";
        }
    }
}
