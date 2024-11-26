using LogUtils.Enums;
using LogUtils.Helpers;

namespace LogUtils
{
    public class PersistentLogFileHandle : PersistentFileHandle
    {
        public readonly LogID FileID;

        public PersistentLogFileHandle(LogID logFile) : base()
        {
            FileID = logFile;
            CreateFileStream();
        }

        protected override void CreateFileStream()
        {
            Stream = LogFile.Open(FileID);
        }
    }
}
