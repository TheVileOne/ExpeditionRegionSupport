using LogUtils;
using LogUtils.Diagnostics;
using LogUtils.Enums.FileSystem;
using System;
using System.IO;

internal sealed class FileSystemExceptionHandler : ExceptionHandler
{
    /// <summary>
    /// Can the process recover from an exceptional state
    /// </summary>
    public bool CanContinue = true;

    /// <summary>
    /// Is the context a file move, or file copy
    /// </summary>
    public bool IsCopyContext;

    private string contextTag => IsCopyContext ? "Copy" : "Move";

    private readonly string sourceFilename;

    public FileSystemExceptionHandler(string sourceFilename)
    {
        this.sourceFilename = sourceFilename;
    }

    public override void OnError(Exception exception)
    {
        CanContinue = CanRecoverFrom(exception);
        base.OnError(exception);
    }

    protected override void LogError(Exception exception)
    {
        if (exception is FileNotFoundException)
        {
            UtilityLogger.LogError($"{contextTag} target file {sourceFilename} could not be found");
            return;
        }

        if (exception is IOException)
        {
            if (exception.Message.StartsWith("Sharing violation"))
                UtilityLogger.LogError($"{contextTag} target file {sourceFilename} is currently in use");
        }
        UtilityLogger.LogError(exception);
    }

    internal static bool CanRecoverFrom(Exception ex) => ex is not FileNotFoundException; //In context this refers to the source file
}
