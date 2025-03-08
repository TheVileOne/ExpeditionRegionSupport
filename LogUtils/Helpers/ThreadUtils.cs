using System;

namespace LogUtils.Helpers
{
    public static class ThreadUtils
    {
        public static int MainThreadID; 

        public static bool IsRunningOnMainThread => MainThreadID == Environment.CurrentManagedThreadId;

        public static bool AssertRunningOnMainThread(object context)
        {
            if (IsRunningOnMainThread) return true;

            UtilityLogger.Logger.LogDebug("Assert failed: currently not on main thread");
            UtilityLogger.Logger.LogDebug($"ThreadInfo: Id [{Environment.CurrentManagedThreadId}] Source [{context}]");
            return false;
        }
    }
}
