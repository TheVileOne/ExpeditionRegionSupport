using System;

namespace LogUtils.Enums
{
    public enum FileStatus
    {
        AwaitingStatus,
        NoActionRequired,
        MoveRequired,
        MoveComplete,
        CopyComplete,
        FileAlreadyExists,
        ValidationFailed,
        Error
    }

    public enum FileAction
    {
        None,
        Create,
        Delete,
        Buffering,
        Write,
        Move,
        Copy,
        Open,
        PathUpdate,
        SessionStart,
        SessionEnd,
        StreamDisposal,
    }

    /// <summary>
    /// Represents permission flags that apply to a folder
    /// </summary>
    [Flags]
    public enum FolderPermissions
    {
        None = 0,
        Delete = 1,
        Move = 2,
        All = Delete | Move
    }
}
