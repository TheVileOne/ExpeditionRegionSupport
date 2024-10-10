namespace LogUtils.Enums
{
    public enum FileStatus
    {
        AwaitingStatus,
        NoActionRequired,
        MoveRequired,
        MoveComplete,
        CopyComplete,
        ValidationFailed,
        Error
    }

    public enum FileAction
    {
        None,
        Create,
        Delete,
        Move,
        Copy,
        PathUpdate,
        SessionStart,
        SessionEnd,
        Log
    }
}
