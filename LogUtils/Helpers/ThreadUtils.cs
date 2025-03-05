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
                UtilityLogger.Logger.LogDebug("Assert failed: currently not on main thread");
                UtilityLogger.Logger.LogDebug($"ThreadInfo: Id [{Thread.CurrentThread.ManagedThreadId}] Source [{context}]");
                return false;
            }
            return true;
        }
    }
}
