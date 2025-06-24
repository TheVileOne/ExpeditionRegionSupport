using System;

namespace LogUtils.Helpers
{
    public static class ThreadUtils
    {
        public static int MainThreadID = -1;

        public static bool IsRunningOnMainThread => MainThreadID == Environment.CurrentManagedThreadId;

        public static bool AssertRunningOnMainThread(object context)
        {
            if (IsRunningOnMainThread) return true;

            if (MainThreadID == -1)
            {
                UtilityLogger.Logger.LogDebug("Assert failed: Unable to determine main thread ID");
            }
            else
            {
                UtilityLogger.Logger.LogDebug("Assert failed: Currently not on main thread");
                UtilityLogger.Logger.LogDebug($"ThreadInfo: Id [{Environment.CurrentManagedThreadId}] Source [{context}]");
            }
            return false;
        }
    }
}
