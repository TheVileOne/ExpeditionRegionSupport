using LogUtils.Enums;
using LogUtils.Timers;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LogUtils.Diagnostics.Tests.Utility
{
    internal static class StressTests
    {
        public static void LogEveryFrame(LogID logFile, int messageFrequency = 1, int logUntilThisFrame = -1, int messagesPerFrame = 1)
        {
            UtilityLogger.PerformanceMode = true;

            Logger logger = new DiscreteLogger(logFile);

            Stopwatch sw = new Stopwatch();
            int messageCount = 0;
            ScheduledEvent myEvent = UtilityCore.Scheduler.Schedule(() =>
            {
                if (messageCount == 0)
                    sw.Start();

                if (messagesPerFrame > 1)
                {
                    int i = 0;
                    while (i < messagesPerFrame)
                    {
                        logger.LogDebug($"Frame {messageCount} subcount {i}");
                        i++;
                    }
                }
                else
                {
                    logger.LogDebug($"message {messageCount}");
                }
                messageCount++;

                if (messageCount % 25 == 0)
                    UtilityLogger.Logger.LogDebug("Average milliseconds per frame " + (TimeSpan.FromTicks(sw.ElapsedTicks).TotalMilliseconds / messageCount));

            }, messageFrequency, logUntilThisFrame);

            FrameTimer.OnRelease += onEventFinished;

            void onEventFinished(FrameTimer timer, EventArgs e)
            {
                if (timer.Event != myEvent) return;

                logger.LogDebug("TEST COMPLETE");
                logger.Dispose();
                logger = null;
                FrameTimer.OnRelease -= onEventFinished;
                UtilityLogger.PerformanceMode = false;
            }
        }

        public static void TestLoggerDisposal()
        {
            //Register a high amount of logger instances, and deference them to observe dispose behavior
            Logger[] loggers = new Logger[1000];

            for (int i = 0; i < loggers.Length; i++)
            {
                loggers[i] = new Logger();
                //UtilityCore.RequestHandler.Unregister(loggers[i]);
            }
            loggers = null;
        }

        public static async Task TestMultithreadedLogging()
        {
            const int MESSAGE_COUNT = 250;

            LogID testLogID = new LogID("test.log", UtilityConsts.PathKeywords.ROOT, LogAccess.Private, false);

            //TODO: Queued messages before Rain World can update don't get processed
            try
            {
                await TestMultithreadedLogging(testLogID, LoggingMode.Normal, MESSAGE_COUNT);
                await TestMultithreadedLogging(testLogID, LoggingMode.Timed, MESSAGE_COUNT);
                //await TestMultithreadedLogging(testLogID, LoggingMode.Queue, MESSAGE_COUNT);
            }
            catch (Exception ex)
            {
                UtilityLogger.LogError(ex);
            }
        }

        internal static async Task TestMultithreadedLogging(LogID target, LoggingMode mode, int messageCount)
        {
            //TODO: This needs to be changed into an async implementation. Currently the logger ends up disposing before all requests are processed.
            //For context, log requests run from other threads may end up triggering a recursion flag on submission, pushing the request to be logged on the next frame.
            //This scheduling of requests doesn't work well with the async test code below. 
            Logger logger = new DiscreteLogger(mode, target);

            logger.LogDebug($"Logging mode {mode}");
            logger.LogDebug($"Logging {messageCount} messages");
            Task[] tasks = new Task[messageCount];

            //Schedule a lot of scheduled log requests to ensure messages log properly to file in a multithreaded environment
            for (int i = 0; i < messageCount; i++)
            {
                int tIndex = i;
                tasks[tIndex] = Task.Run(() => logger.LogDebug($"Log #[{tIndex}]"));
            }
            await Task.WhenAll(tasks);
            logger.LogDebug("TEST COMPLETE");
            logger.Dispose();
        }
    }
}
