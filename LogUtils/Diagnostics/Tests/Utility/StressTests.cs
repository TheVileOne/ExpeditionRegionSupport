using LogUtils.Enums;
using LogUtils.Threading;
using LogUtils.Timers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using DotNetTask = System.Threading.Tasks.Task;

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

        public static async DotNetTask TestMultithreadedLogging()
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

        internal static async DotNetTask TestMultithreadedLogging(LogID target, LoggingMode mode, int messageCount)
        {
            //TODO: This needs to be changed into an async implementation. Currently the logger ends up disposing before all requests are processed.
            //For context, log requests run from other threads may end up triggering a recursion flag on submission, pushing the request to be logged on the next frame.
            //This scheduling of requests doesn't work well with the async test code below. 
            Logger logger = new DiscreteLogger(mode, target);

            logger.LogDebug($"Logging mode {mode}");
            logger.LogDebug($"Logging {messageCount} messages");
            DotNetTask[] tasks = new DotNetTask[messageCount];

            //Schedule a lot of scheduled log requests to ensure messages log properly to file in a multithreaded environment
            for (int i = 0; i < messageCount; i++)
            {
                int tIndex = i;
                tasks[tIndex] = DotNetTask.Run(() => logger.LogDebug($"Log #[{tIndex}]"));
            }
            await DotNetTask.WhenAll(tasks);
            logger.LogDebug("TEST COMPLETE");
            logger.Dispose();
        }

        /// <summary>
        /// Shows that <see cref="ThreadSafeWorker"/> can lock an arbitrary number of locks without being at risk of causing a deadlock
        /// </summary>
        internal static void TestThreadSafeWorker()
        {
            const int THREAD_COUNT = 4;
            const int LOCK_COUNT = 10;
            const int MAX_EXECUTIONS = 200; //Not expected to reach this
            const int MAX_EXECUTION_TIME = 10; //seconds
            const int WORK_DURATION = 15; //milliseconds

            List<object> locks = createLocks(LOCK_COUNT);

            int threadCount = 0;
            Action captureLocks = new Action(() =>
            {
                int threadID = Interlocked.Increment(ref threadCount);

                //Make sure that each thread is capturing the locks in a different order
                locks.Shuffle();

                //Order is preserved by storing in a local variable
                object[] _locks = locks.ToArray();

                ThreadSafeWorker worker = new ThreadSafeWorker(_locks)
                {
                    UseEnumerableWrapper = false
                };

                int executionCount = 0;
                Stopwatch timer = new Stopwatch();

                long lastTimeElapsed = 0;
                //Loop until a specific number of executions complete, or timeout is reached
                Action doNothing = new Action(() => { Thread.Sleep(WORK_DURATION); });
                while (true)
                {
                    try
                    {
                        timer.Start();
                        worker.DoWork(doNothing);
                        timer.Stop();

                        long timeElapsed = timer.ElapsedMilliseconds - lastTimeElapsed;
                        lastTimeElapsed = timer.ElapsedMilliseconds;
                        executionCount++;
                        UtilityLogger.DebugLog($"Task Result: THREAD: {threadID} TIME TAKEN: {timeElapsed} ms");
                    }
                    catch (Exception ex)
                    {
                        UtilityLogger.DebugLog(ex);
                    }

                    if (executionCount == MAX_EXECUTIONS || timer.Elapsed >= TimeSpan.FromSeconds(MAX_EXECUTION_TIME))
                    {
                        UtilityLogger.DebugLog($"Task running on thread {threadID} finished after {timer.ElapsedMilliseconds / 1000} seconds.");
                        break;
                    }
                }

                if (executionCount != MAX_EXECUTIONS)
                    UtilityLogger.DebugLog($"Task thread failed to reach maximum executions. TOTAL EXECUTIONS: {executionCount}");
            });

            //Capture locks on background threads to test potential deadlock conditions
            for (int i = 0; i < THREAD_COUNT; i++)
                DotNetTask.Run(captureLocks);

            static List<object> createLocks(int locksWanted)
            {
                List<object> objects = new List<object>(locksWanted);
                while (locksWanted > objects.Count)
                    objects.Add(new object());
                return objects;
            }
        }
    }
}
