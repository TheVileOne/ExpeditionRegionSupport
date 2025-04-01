using LogUtils.Enums;
using LogUtils.Timers;
using System;

namespace LogUtils.Diagnostics.Tests.Utility
{
    internal static class StressTests
    {
        public static void LogEveryFrame(LogID logFile, int messageFrequency = 1, int logUntilThisFrame = -1)
        {
            DiscreteLogger logger = new DiscreteLogger(logFile);

            int messageCount = 0;
            ScheduledEvent myEvent = UtilityCore.Scheduler.Schedule(() =>
            {
                /*
                int i = 0;
                while (i < 100)
                {
                    logger.LogDebug($"message {messageCount} subcount {i}");
                    i++;
                }
                */
                logger.LogDebug($"message {messageCount}");
                messageCount++;
            }, messageFrequency, logUntilThisFrame);

            FrameTimer.OnRelease += onEventFinished;

            void onEventFinished(FrameTimer timer, EventArgs e)
            {
                if (timer.Event != myEvent) return;

                logger.LogDebug("TEST COMPLETE");
                logger.Dispose();
                logger = null;
                FrameTimer.OnRelease -= onEventFinished;
            }
        }
    }
}
