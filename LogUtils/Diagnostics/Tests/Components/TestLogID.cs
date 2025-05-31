using LogUtils.Enums;
using LogUtils.Helpers.FileHandling;

namespace LogUtils.Diagnostics.Tests.Components
{
    public class TestLogID : LogID
    {
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
        /// Cycle to the next LogAccess enum value
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

        public static TestLogID Create(string filename, string path)
        {
            return new TestLogID(filename, path, LogAccess.FullAccess);
        }

        public static TestLogID FromTarget(LogID target, string path)
        {
            return new TestLogID(target.value, path, LogAccess.FullAccess);
        }

        public static TestLogID FromPath(string path)
        {
            return new TestLogID(PathUtils.GetRandomFilename(FileExt.DEFAULT), path, LogAccess.FullAccess);
        }
    }
}
