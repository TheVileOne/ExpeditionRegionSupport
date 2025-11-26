using LogUtils.Enums;
using LogUtils.Helpers.FileHandling;

namespace LogUtils.Diagnostics.Tests.Components
{
    public class TestLogID : LogID
    {
        /// <inheritdoc cref="LogID.Factory"/>
        public static new TestLogIDFactory Factory = new TestLogIDFactory();

        public TestLogID() : this(LogAccess.FullAccess)
        {
        }

        public TestLogID(LogAccess access) : base(PathUtils.GetRandomFilename(FileExt.DEFAULT), access, false)
        {
        }

        public TestLogID(string filename, string path, LogAccess access) : base(filename, path, access, false)
        {
        }

        /// <summary>
        /// Cycle to the next <see cref="LogAccess"/> value
        /// </summary>
        public void CycleAccess()
        {
            switch (Access)
            {
                case LogAccess.FullAccess:
                    Access = LogAccess.RemoteAccessOnly;
                    break;
                case LogAccess.RemoteAccessOnly:
                    Access = LogAccess.Private;
                    break;
                case LogAccess.Private:
                    Access = LogAccess.FullAccess;
                    break;
            }
        }
    }
}
