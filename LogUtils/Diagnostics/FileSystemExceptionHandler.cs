using LogUtils;
using LogUtils.Diagnostics;
using LogUtils.Enums.FileSystem;
using System;
using System.IO;

internal class FileSystemExceptionHandler : ExceptionHandler
{
    /// <summary>
    /// Can the process recover from an exceptional state
    /// </summary>
    public bool CanContinue = true;

    /// <summary>
    /// The current contextual state - this value affects the phrasing of certain contextual information when exception information is logged
    /// </summary>
    /// <value>Currently supported values: <see cref="ActionType.Move"/>, <see cref="ActionType.Copy"/>, <see cref="ActionType.Delete"/></value>
    public ActionType Context = ActionType.None;

    private readonly string sourceFilename;

    [ThreadStatic]
    private static string _customErrorMessage;

    /// <summary>
    /// A non-empty custom message overrides other exception header messages. The value will get cleared after use.
    /// </summary>
    protected string CustomMessage => _customErrorMessage;

    public FileSystemExceptionHandler(string sourceFilename)
    {
        this.sourceFilename = sourceFilename;
    }

    public FileSystemExceptionHandler(string sourceFilename, ActionType context) : this(sourceFilename)
    {
        Context = context;
    }

    public FileSystemExceptionHandler(string sourceFilename, FileAction context) : this(sourceFilename, new ActionType(context))
    {
    }

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

    protected override void LogError(Exception exception)
    {
        string errorMessage = null;
        if (_customErrorMessage != null) //Custom error message always overrides default provided message formatting
        {
            errorMessage = _customErrorMessage;
            UtilityLogger.LogError(errorMessage, exception);
            return;
        }

        if (Context == ActionType.Delete)
        {
            errorMessage = "Unable to delete file";
        }
        else
        {
            string descriptor;
            if (exception is FileNotFoundException)
            {
                descriptor = GetDescriptor();
                errorMessage = descriptor + " could not be found";

                if (Context == ActionType.Move || Context == ActionType.Copy)
                {
                    UtilityLogger.LogError(errorMessage); //Stack trace is not logged in this case
                    return;
                }
            }

            if (exception is IOException && exception.Message.StartsWith("Sharing violation"))
            {
                descriptor = GetDescriptor();
                errorMessage = descriptor + " is currently in use";
            }
        }
        UtilityLogger.LogError(errorMessage, exception);
    }

    internal bool CanRecoverFrom(Exception ex)
    {
        if (Context == ActionType.Move || Context == ActionType.Copy)
            return ex is not FileNotFoundException; //In context this refers to the source file
        return false;
    }

    /// <summary>
    /// Identifies the target file, or directory
    /// </summary>
    /// <returns>A string that represents a file, or directory (for logging purposes)</returns>
    protected virtual string GetDescriptor()
    {
        string contextTag = null;
        if (Context == ActionType.Move)
            contextTag = "Move";
        else if (Context == ActionType.Copy)
            contextTag = "Copy";

        //TODO: This needs to make sense for both files and directories
        string descriptor;
        string filename = sourceFilename;
        if (contextTag != null)
        {
            descriptor = contextTag + " target file";
        }
        else
        {
            descriptor = "File";
        }

        if (filename != null) //Include filename when it exists
            return string.Format("{0} {1}", descriptor, filename);
        return descriptor;
    }
}
