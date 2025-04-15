using LogUtils.Enums;
using System.IO;

namespace LogUtils.Diagnostics.Tests.Components
{
    public class TestLogID : LogID
    {
        public TestLogID() : this(LogAccess.FullAccess)
        {
        }

        public TestLogID(LogAccess access) : base(Path.GetRandomFileName(), access, false)
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
    }
}
