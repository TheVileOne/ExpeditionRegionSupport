using LogUtils.Enums.FileSystem;
using System;
using System.IO;

namespace LogUtils.Diagnostics
{
    public sealed class DirectoryExceptionHandler : FileSystemExceptionHandler
    {
        /// <inheritdoc/>
        protected override bool IsFileContext => false;

        public DirectoryExceptionHandler()
        {
        }

        public DirectoryExceptionHandler(ActionType context) : base(context)
        {
        }

        /// <inheritdoc/>
        protected override string CreateErrorMessage(ExceptionContextWrapper contextWrapper, ref bool includeStackTrace)
        {
            if (contextWrapper.CustomMessage != null) //Custom error message always overrides default provided message formatting
                return contextWrapper.CustomMessage;

            string message = null;
            if (contextWrapper.IsExceptionContext)
            {
                Exception exception = contextWrapper.Source;

                if (exception is DirectoryNotFoundException)
                {
                    string descriptor = GetDescriptor(contextWrapper);
                    message = descriptor + " could not be found";
                }
            }

            if (message != null)
                return message;

            return base.CreateErrorMessage(contextWrapper, ref includeStackTrace);
        }

        /// <inheritdoc/>
        protected override string GetSimpleDescriptor(ExceptionContextWrapper contextWrapper)
        {
            return "directory";
        }
    }
}
