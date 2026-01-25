using LogUtils.Enums.FileSystem;
using System;
using System.IO;

namespace LogUtils.Diagnostics
{
    public sealed class DirectoryExceptionHandler : FileSystemExceptionHandler
    {
        /// <inheritdoc/>
        protected override bool IsFileContext => false;

        public DirectoryExceptionHandler(string sourceName) : base(sourceName)
        {
        }

        public DirectoryExceptionHandler(string sourceName, ActionType context) : base(sourceName, context)
        {
        }

        /// <inheritdoc/>
        protected override void LogError(Exception exception)
        {
            if (CustomMessage != null) //Base handler is designed to handle this
            {
                base.LogError(exception);
                return;
            }

            if (exception is DirectoryNotFoundException)
            {
                string descriptor = GetDescriptor();
                UtilityLogger.LogError(descriptor + " could not be found", exception);
                return;
            }
            base.LogError(exception);
        }

        /// <inheritdoc/>
        protected override string GetSimpleDescriptor()
        {
            return "directory";
        }
    }
}
