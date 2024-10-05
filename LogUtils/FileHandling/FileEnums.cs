using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogUtils.FileHandling
{
    public static class FileEnums
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
}
