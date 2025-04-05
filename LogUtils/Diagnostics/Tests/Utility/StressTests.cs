using LogUtils.Enums;
using LogUtils.Timers;
using System;
using System.Diagnostics;

namespace LogUtils.Diagnostics.Tests.Utility
{
    internal static class StressTests
    {
        public static void LogEveryFrame(LogID logFile, int messageFrequency = 1, int logUntilThisFrame = -1)
        {
            LogID.FileActivity.IsEnabled = false;

            DiscreteLogger logger = new DiscreteLogger(logFile);

            Stopwatch sw = new Stopwatch();
            int messageCount = 0;
            ScheduledEvent myEvent = UtilityCore.Scheduler.Schedule(() =>
            {
                if (messageCount == 0)
                    sw.Start();

                int i = 0;
                while (i < 100)
                {
                    logger.LogDebug($"Frame {messageCount} subcount {i}");
                    i++;
                }
                //logger.LogDebug($"message {messageCount}");
                messageCount++;

                if (messageCount % 25 == 0)
                {
                    UtilityLogger.Logger.LogDebug("Average milliseconds per frame " + (TimeSpan.FromTicks(sw.ElapsedTicks).TotalMilliseconds / messageCount));
                }

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
