using LogUtils.Enums.FileSystem;
using LogUtils.Helpers.FileHandling;
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using ExceptionDataKey = LogUtils.UtilityConsts.ExceptionDataKey;

namespace LogUtils.Diagnostics
{
    public abstract class FileSystemExceptionHandler : ExceptionHandler
    {
        /// <summary>
        /// Can the process recover from an exceptional state
        /// </summary>
        public bool CanContinue { get; protected set; } = true;

        /// <summary>
        /// The current contextual state - this value affects the phrasing of certain contextual information when exception information is logged
        /// </summary>
        /// <value>Currently supported values: <see cref="ActionType.Move"/>, <see cref="ActionType.Copy"/>, <see cref="ActionType.Delete"/></value>
        public ActionType Context = ActionType.None;

        [ThreadStatic]
        private static string _customErrorMessage;

        /// <summary>
        /// The value of this affects the value of descriptors when exception information is logged
        /// </summary>
        protected abstract bool IsFileContext { get; }

        protected FileSystemExceptionHandler()
        {
        }

        protected FileSystemExceptionHandler(ActionType context)
        {
            Context = context;
        }

        /// <inheritdoc/>
        public override void OnError(Exception exception)
        {
            CanContinue = CanRecoverFrom(exception);
            base.OnError(exception);
        }

        public void OnError(Exception exception, string customErrorMessage)
        {
            try
            {
                _customErrorMessage = customErrorMessage;
                OnError(exception);
            }
            finally
            {
                _customErrorMessage = null;
            }
        }

        /// <summary>
        /// Influences the value of <see cref="CanContinue"/> which is used particular in cases where exceptions need to be caught inside a loop operation
        /// </summary>
        protected virtual bool CanRecoverFrom(Exception ex) => true;

        internal string CreateErrorMessage(Exception exception, ref bool includeStackTrace)
        {
            var contextWrapper = new ExceptionContextWrapper(exception, GetPrimaryContext(exception, out bool hasSecondaryContext))
            {
                IsExceptionContext = !hasSecondaryContext,
                CustomMessage = !hasSecondaryContext ? _customErrorMessage : null,
            };

            //TODO: The primary context may not always be desirable. We need a way to avoid indicating that the main process failed,
            //when any secondary process logs an Exception
            string primaryMessage,
                   secondaryMessage = null;

            //Primary context message
            primaryMessage = CreateErrorMessage(contextWrapper, ref includeStackTrace);

            if (hasSecondaryContext)
            {
                contextWrapper = new ExceptionContextWrapper(exception, (ActionType)exception.Data[ExceptionDataKey.CONTEXT])
                {
                    IsExceptionContext = true,
                    CustomMessage = _customErrorMessage,
                };

                secondaryMessage = CreateErrorMessage(contextWrapper, ref includeStackTrace);
            }

            //Combine the primary and secondary messages into a reportable string
            string message;
            if (primaryMessage != null)
            {
                message = primaryMessage;
                if (secondaryMessage != null)
                {
                    message += Environment.NewLine;
                    message += "REASON: " + secondaryMessage;
                }
            }
            else
            {
                message = secondaryMessage;
            }
            return message;
        }

        /// <summary>
        /// Generates a message to be included when logging the <see cref="Exception"/>
        /// </summary>
        /// <param name="contextWrapper">Contains context specific information such as the <see cref="Exception"/> instance</param>
        /// <param name="includeStackTrace">
        /// Affects whether stack trace is going to be part of the logging output; true by default. Don't change unless you need to change it.
        /// </param>
        protected virtual string CreateErrorMessage(ExceptionContextWrapper contextWrapper, ref bool includeStackTrace)
        {
            if (contextWrapper.CustomMessage != null) //Custom error message always overrides default provided message formatting
                return contextWrapper.CustomMessage;

            ActionType context = contextWrapper.Context;
            Exception exception = contextWrapper.Source;

            string descriptor, message = null;
            if (context == ActionType.Delete)
            {
                descriptor = GetSimpleDescriptor(contextWrapper);
                message = "Unable to delete " + descriptor;
            }
            else if (context == ActionType.Move)
            {
                descriptor = GetSimpleDescriptor(contextWrapper);
                message = "Unable to move " + descriptor;
            }
            else if (context == ActionType.Copy)
            {
                descriptor = GetSimpleDescriptor(contextWrapper);
                message = "Unable to copy " + descriptor;
            }
            else if (contextWrapper.IsExceptionContext)
            {
                if (exception is IOException && exception.Message.StartsWith("Sharing violation"))
                {
                    descriptor = GetDescriptor(contextWrapper);
                    message = descriptor + " is currently in use";
                }
                else if (exception is FileNotFoundException)
                {
                    descriptor = "File";
                    message = descriptor + " could not be found";
                }
                else if (exception is DirectoryNotFoundException)
                {
                    descriptor = "Directory";
                    message = descriptor + " could not be found";
                }
            }
            return message;
        }

        /// <inheritdoc/>
        protected sealed override void LogError(Exception exception)
        {
            bool includeStackTrace = true;
            LogError(exception, CreateErrorMessage(exception, ref includeStackTrace), includeStackTrace);
        }

        /// <inheritdoc cref="LogError(Exception)"/>
        protected virtual void LogError(Exception exception, string message, bool includeStackTrace)
        {
            if (!includeStackTrace)
            {
                if (message != null)
                    UtilityLogger.LogError(message);
                return;
            }
            UtilityLogger.LogError(message, exception);
        }

        protected ActionType GetPrimaryContext(Exception exception, out bool hasSecondaryContext)
        {
            ActionType handlerContext = Context;
            ActionType exceptionContext = exception.Data[ExceptionDataKey.CONTEXT] as ActionType;

            hasSecondaryContext = false;

            //Verify that the handler has a valid context
            if (!ActionType.IsValid(handlerContext)) //Not a valid context
            {
                //Verify that Exception has a valid context
                if (!ActionType.IsValid(exceptionContext))
                    return ActionType.None;

                //This is the only valid context and is treated as the primary context
                return exceptionContext;
            }

            //The handler does have a valid context. In this case we need to check whether we have two distinct contexts, or the same one.
            //It is a common situation for the Exception to inherit the same context of the handler.
            if (handlerContext == exceptionContext || !ActionType.IsValid(exceptionContext))
            {
                //This must mean that there is only one valid context
                return handlerContext;
            }

            //The options have been exhausted. At this point, both contexts must be distinct and valid.
            hasSecondaryContext = true;
            return handlerContext;
        }

        /// <summary>
        /// Identifies the target file, or directory
        /// </summary>
        /// <returns>A string that represents a file, or directory (for logging purposes)</returns>
        protected virtual string GetDescriptor(ExceptionContextWrapper contextWrapper)
        {
            ActionType context = contextWrapper.Context;

            string contextTag = null;
            if (context == ActionType.Move)
                contextTag = "Move";
            else if (context == ActionType.Copy)
                contextTag = "Copy";

            string descriptor;
            if (contextTag != null)
            {
                descriptor = contextTag + " target";
            }
            else
            {
                //Creates a TextInfo based on the "en-US" culture.
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

                descriptor = textInfo.ToTitleCase(GetSimpleDescriptor(contextWrapper));
            }

            string name = null;
            if (contextWrapper.IsExceptionContext)
            {
                PathInfo info = GetPathFromExceptionData(contextWrapper.Source);

                if (info != null)
                {
                    if (info.HasFilename || info.HasDirectory)
                        name = info.Target.Name;
                    else
                        name = info.TargetPath; //It could just be a root
                }
            }

            if (!string.IsNullOrWhiteSpace(name)) //Include source name when it exists
                return string.Format("{0} {1}", descriptor, name);
            return descriptor;
        }

        /// <summary>
        /// The short form identification of the target file, or directory
        /// </summary>
        protected abstract string GetSimpleDescriptor(ExceptionContextWrapper contextWrapper);

        /// <summary>
        /// Attempts to extract path data from the <see cref="Exception"/> data store.
        /// </summary>
        /// <returns>
        /// Information about the path. When there are invalid path characters, this returns the <see cref="PathInfo"/> instance for an empty <see langword="string"/>.
        /// When no path data exists, this method returns <see langword="null"/>.
        /// </returns>
        protected PathInfo GetPathFromExceptionData(Exception exception)
        {
            IDictionary exceptionData = exception.Data;

            object pathObject = exceptionData[ExceptionDataKey.TARGET_PATH] //Only one of these should be set at a time
                             ?? exceptionData[ExceptionDataKey.SOURCE_PATH];
            try
            {
                if (pathObject is string pathString)
                    return new PathInfo(pathString, includeFilenameInPath: true);
            }
            catch (ArgumentException) //Path most likely has invalid characters
            {
                return new PathInfo(string.Empty);
            }
            return null;
        }

        protected struct ExceptionContextWrapper(Exception source, ActionType context)
        {
            /// <summary>
            /// The <see cref="Exception"/> object associated with the context
            /// </summary>
            public Exception Source = source;

            /// <summary>
            /// A value that helps identify what file system activity that was happening when the <see cref="Exception"/> occurred.
            /// </summary>
            public ActionType Context = context;

            /// <summary>
            /// The context most directly associated with the <see cref="Exception"/> itself, and its data.
            /// </summary>
            public bool IsExceptionContext;

            /// <summary>
            /// A message that should replace the standard message when not null
            /// </summary>
            public string CustomMessage;
        }
    }
}
