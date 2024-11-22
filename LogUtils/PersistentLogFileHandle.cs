using LogUtils.Enums;

namespace LogUtils
{
    public class PersistentLogFileHandle : PersistentFileHandle
    {
        public readonly LogID FileID;

        public PersistentLogFileHandle(LogID logFile) : base()
        {
            FileID = logFile;
        }

        protected override void CreateFileStream()
        {
            Stream = LogWriter.GetWriteStream(FileID.Properties.CurrentFilePath, true);
        }
    }
}
