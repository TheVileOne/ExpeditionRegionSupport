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
        }

        protected override void CreateFileStream()
        {
            Stream = LogFile.Open(FileID);
        }
    }
}
