using LogUtils.Events;
using LogUtils.Timers;
using System;

namespace LogUtils.Diagnostics.Tests.Utility
{
    internal sealed class FrameTimerTests : TestCase, ITestable
    {
        internal const string TEST_NAME = "Test - Scheduler";
        internal const int EVENT_INTERVAL = 3;

        public FrameTimerTests() : base(TEST_NAME)
        {
        }

        public void Test()
        {
            testEvents(syncToRainWorld: false);
            testEvents(syncToRainWorld: true);
        }

        [PostTest]
        public void ShowResults()
        {
            TestLogger.LogDebug(CreateReport());
        }

        private void testEvents(bool syncToRainWorld)
        {
            testEventFiresOnCorrectNumberOfFrames(syncToRainWorld);
            testEarlyInvocationDelaysNextEventByOneCompleteInterval(syncToRainWorld);

            /*
             * Testing garbage collection through a unit test didn't work. I did manage to confirm that they should get collected eventually, but may take awhile
             */
            //testTimerIsGarbageCollectedWhenEventIsCancelled(syncToRainWorld);
            //testTimerIsGarbageCollectedWhenItGoesOutOfScopeAndNotHeldByScheduler(syncToRainWorld);
        }

        private void testEventFiresOnCorrectNumberOfFrames(bool syncToRainWorld)
        {
            bool didEventFire = false;
            ScheduledEvent timedEvent = null;

            timedEvent = UtilityCore.Scheduler.Schedule(() =>
            {
                didEventFire = true;
                TestCase test = this;
                FrameTimer timer = timedEvent.EventTimer;

                //Assert that event fired on the correct interval
                test.AssertThat(timer.ElapsedTicks).IsEqualTo(EVENT_INTERVAL);
            }, EVENT_INTERVAL, syncToRainWorld);

            for (int i = 0; i < EVENT_INTERVAL; i++)
                simulateUpdate(syncToRainWorld);

            timedEvent.Cancel();
            AssertThat(didEventFire).IsTrue();
        }

        private void testEarlyInvocationDelaysNextEventByOneCompleteInterval(bool syncToRainWorld)
        {
            bool didEventFire = false;
            ScheduledEvent timedEvent = null;

            timedEvent = UtilityCore.Scheduler.Schedule(() =>
            {
                TestCase test = this;
                FrameTimer timer = timedEvent.EventTimer;

                if (didEventFire)
                {
                    //Assert that event fired on the correct interval
                    test.AssertThat(timer.ElapsedTicks).IsEqualTo(EVENT_INTERVAL * 2);
                }
                didEventFire = true;
            }, EVENT_INTERVAL, syncToRainWorld);

            simulateUpdate(syncToRainWorld);
            timedEvent.InvokeEarly();

            short expectedWaitFrames = EVENT_INTERVAL + EVENT_INTERVAL - 1;
            for (int i = 0; i < expectedWaitFrames; i++)
                simulateUpdate(syncToRainWorld);

            timedEvent.Cancel();
            AssertThat(didEventFire).IsTrue();
        }

        private void testTimerIsGarbageCollectedWhenEventIsCancelled(bool syncToRainWorld)
        {
            ScheduledEvent timedEvent = UtilityCore.Scheduler.Schedule(() => { }, EVENT_INTERVAL, syncToRainWorld);

            int entryCountBeforeEventWasHandled = UtilityCore.Scheduler.Timers.UnsafeCount();

            for (int i = 0; i < EVENT_INTERVAL; i++)
                simulateUpdate(syncToRainWorld);

            timedEvent.Cancel();
            timedEvent = null;

            int entryCount = 0;
            bool eventDisposed = false;
            for (int i = 0; i < 5000; i++)
            {
                if (i % 100 == 0)
                {
                    entryCount = UtilityCore.Scheduler.Timers.UnsafeCount();

                    if (entryCount != entryCountBeforeEventWasHandled)
                    {
                        eventDisposed = true;
                        break;
                    }
                }

                //Simulate updates until we can detect a different collection count, or timeout trying
                simulateUpdate(syncToRainWorld);
            }
            AssertThat(eventDisposed).IsTrue();
        }

        private void testTimerIsGarbageCollectedWhenItGoesOutOfScopeAndNotHeldByScheduler(bool syncToRainWorld)
        {
            createTimersInPrivateScope(syncToRainWorld);
            int entryCountBeforeEventWasHandled = UtilityCore.Scheduler.Timers.UnsafeCount();

            bool eventDisposed = false;
            for (int i = 0; i < 5000; i++)
            {
                if (i % 100 == 0)
                {
                    int entryCount = UtilityCore.Scheduler.Timers.UnsafeCount();

                    if (entryCount != entryCountBeforeEventWasHandled)
                    {
                        eventDisposed = true;
                        break;
                    }
                }

                //Simulate updates until we can detect a different collection count, or timeout trying
                simulateUpdate(syncToRainWorld);
            }
            AssertThat(eventDisposed).IsTrue();
        }

        private void createTimersInPrivateScope(bool syncToRainWorld)
        {
            for (int i = 0; i < 500; i++)
                new FrameTimer(EVENT_INTERVAL, syncToRainWorld);
        }

        private static void simulateUpdate(bool syncToRainWorld)
        {
            //Synced implementation requires a single scheduler update to subscribe to its update event handler
            if (syncToRainWorld)
                simulateUpdate(syncToRainWorld: false);

            Action updateDelegate = !syncToRainWorld ? UtilityCore.Scheduler.Update : syncUpdate;

            updateDelegate.Invoke();

            static void syncUpdate()
            {
                UtilityEvents.OnNewUpdateSynced.Invoke(null, EventArgs.Empty);
            }
        }
    }
}
