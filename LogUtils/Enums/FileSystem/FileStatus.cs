namespace LogUtils.Enums.FileSystem
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
}
