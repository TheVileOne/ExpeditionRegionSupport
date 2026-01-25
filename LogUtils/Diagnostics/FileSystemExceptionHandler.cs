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
    /// <value>Currently supported values: <see cref="ActionType.Move"/>, <see cref="ActionType.Copy"/></value>
    public ActionType Context = ActionType.None;

    private readonly string sourceFilename;

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

    protected override void LogError(Exception exception)
    {
        string descriptor;
        if (exception is FileNotFoundException)
        {
            descriptor = GetDescriptor();
            UtilityLogger.LogError(descriptor + " could not be found");
            return;
        }

        if (exception is IOException)
        {
            if (exception.Message.StartsWith("Sharing violation"))
            {
                descriptor = GetDescriptor();
                UtilityLogger.LogError(descriptor + " is currently in use");
            }
        }
        UtilityLogger.LogError(exception);
    }

    internal static bool CanRecoverFrom(Exception ex) => ex is not FileNotFoundException; //In context this refers to the source file

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
