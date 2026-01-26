using LogUtils.Enums.FileSystem;
using System;
using System.IO;

namespace LogUtils.Diagnostics
{
    public class FileExceptionHandler : FileSystemExceptionHandler
    {
        /// <inheritdoc/>
        protected sealed override bool IsFileContext => true;

        public FileExceptionHandler()
        {
        }

        public FileExceptionHandler(FileAction context) : base(new ActionType(context))
        {
        }

        public FileExceptionHandler(ActionType context) : base(context)
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
        protected override string CreateErrorMessage(ExceptionContextWrapper contextWrapper, ref bool includeStackTrace)
        {
            if (contextWrapper.CustomMessage != null) //Custom error message always overrides default provided message formatting
                return contextWrapper.CustomMessage;

            if (contextWrapper.IsExceptionContext)
            {
                ActionType context = contextWrapper.Context;
                Exception exception = contextWrapper.Source;

                if (exception is FileNotFoundException && (context == ActionType.Move || context == ActionType.Copy))
                {
                    includeStackTrace = false; //Stack trace is not logged in this case
                    string descriptor = GetDescriptor(contextWrapper);
                    string message = descriptor + " could not be found";
                    return message;
                }
            }
            return base.CreateErrorMessage(contextWrapper, ref includeStackTrace);
        }

        /// <inheritdoc/>
        protected override string GetSimpleDescriptor(ExceptionContextWrapper contextWrapper)
        {
            return "file";
        }
    }
}
