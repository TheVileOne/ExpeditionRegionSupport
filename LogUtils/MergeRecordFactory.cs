using LogUtils.Enums;

namespace LogUtils
{
    internal static class MergeRecordFactory
    {
        public static MergeRecord Create(LogID logFile, bool canHandle)
        {
            return new FileMoveRecord()
            {
                Target = logFile,
                OriginalPath = logFile.Properties.CurrentFilePath,
                CanHandleFile = canHandle,
            };
        }

        public static MergeRecord Create(LogGroupID logGroup)
        {
            return new LogGroupMoveRecord()
            {
                Target = logGroup,
                OriginalPath = logGroup.Properties.CurrentFolderPath,
            };
        }
    }
}
