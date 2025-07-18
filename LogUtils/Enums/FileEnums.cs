﻿namespace LogUtils.Enums
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
}
