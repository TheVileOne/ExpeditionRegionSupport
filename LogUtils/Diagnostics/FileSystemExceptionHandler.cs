using LogUtils;
using LogUtils.Diagnostics;
using LogUtils.Enums.FileSystem;
using System;
using System.Globalization;
using System.IO;

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

    private readonly string sourceName;

    [ThreadStatic]
    private static string _customErrorMessage;

    /// <summary>
    /// A non-empty custom message overrides other exception header messages. The value will get cleared after use.
    /// </summary>
    protected string CustomMessage => _customErrorMessage;

    /// <summary>
    /// The value of this affects the value of descriptors when exception information is logged
    /// </summary>
    protected abstract bool IsFileContext { get; }

    protected FileSystemExceptionHandler(string sourceName)
    {
        this.sourceName = sourceName;
    }

    protected FileSystemExceptionHandler(string sourceName, ActionType context) : this(sourceName)
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

    /// <inheritdoc/>
    protected override void LogError(Exception exception)
    {
        string errorMessage = null;
        if (_customErrorMessage != null) //Custom error message always overrides default provided message formatting
        {
            errorMessage = _customErrorMessage;
            UtilityLogger.LogError(errorMessage, exception);
            return;
        }

        string descriptor;
        if (Context == ActionType.Delete)
        {
            descriptor = GetSimpleDescriptor();
            errorMessage = "Unable to delete " + descriptor;
        }
        else if (Context == ActionType.Move)
        {
            descriptor = GetSimpleDescriptor();
            errorMessage = "Unable to move " + descriptor;
        }
        else if (Context == ActionType.Copy)
        {
            descriptor = GetSimpleDescriptor();
            errorMessage = "Unable to copy " + descriptor;
        }
        else if (exception is IOException && exception.Message.StartsWith("Sharing violation"))
        {
            descriptor = GetDescriptor();
            errorMessage = descriptor + " is currently in use";
        }
        else if (exception is FileNotFoundException)
        {
            descriptor = "File";
            errorMessage = descriptor + " could not be found";
        }
        else if (exception is DirectoryNotFoundException)
        {
            descriptor = "Directory";
            errorMessage = descriptor + " could not be found";
        }
        UtilityLogger.LogError(errorMessage, exception);
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

        string descriptor;
        if (contextTag != null)
        {
            descriptor = contextTag + " target";
        }
        else
        {
            //Creates a TextInfo based on the "en-US" culture.
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            descriptor = textInfo.ToTitleCase(GetSimpleDescriptor());
        }

        string name = sourceName;
        if (name != null) //Include source name when it exists
            return string.Format("{0} {1}", descriptor, name);
        return descriptor;
    }

    /// <summary>
    /// The short form identification of the target file, or directory
    /// </summary>
    protected abstract string GetSimpleDescriptor();
}
