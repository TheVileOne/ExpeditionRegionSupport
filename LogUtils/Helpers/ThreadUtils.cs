using LogUtils.Enums;
using System.Threading;

namespace LogUtils.Helpers
{
    public static class ThreadUtils
    {
        public static bool IsRunningOnMainThread()
        {
            return UtilityCore.ThreadID == Thread.CurrentThread.ManagedThreadId;
        }

        public static bool AssertRunningOnMainThread(object context)
        {
            if (!IsRunningOnMainThread())
            {
                UtilityLogger.Log(LogCategory.Debug, "Assert failed: currently not on main thread");
                UtilityLogger.Log(LogCategory.Debug, $"ThreadInfo: Id [{Thread.CurrentThread.ManagedThreadId}] Source [{context}]");
                return false;
            }
            return true;
        }
    }
}
