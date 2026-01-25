using LogUtils.Enums.FileSystem;
using System;
using System.IO;

namespace LogUtils.Diagnostics
{
    public class FileExceptionHandler : FileSystemExceptionHandler
    {
        /// <inheritdoc/>
        protected sealed override bool IsFileContext => true;

        public FileExceptionHandler() : base(null)
        {
        }

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

            if (exception is FileNotFoundException && (Context == ActionType.Move || Context == ActionType.Copy))
            {
                string descriptor = GetDescriptor();
                string errorMessage = descriptor + " could not be found";

                UtilityLogger.LogError(errorMessage); //Stack trace is not logged in this case
                return;
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
