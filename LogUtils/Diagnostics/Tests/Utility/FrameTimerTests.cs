using LogUtils.Timers;

namespace LogUtils.Diagnostics.Tests.Utility
{
    public class FrameTimerTests : TestCase, ITestable
    {
        internal const string TEST_NAME = "Test - Scheduler";
        internal const int EVENT_INTERVAL = 3;

        public FrameTimerTests() : base(TEST_NAME)
        {
        }

        public void Test()
        {
            testEventFiresOnCorrectNumberOfFrames();
            testEarlyInvocationDelaysNextEventByOneCompleteInterval();

            /*
             * Testing garbage collection through a unit test didn't work. I did manage to confirm that they should get collected eventually, but may take awhile
             */
            //testTimerIsGarbageCollectedWhenEventIsCancelled();
            //testTimerIsGarbageCollectedWhenItGoesOutOfScopeAndNotHeldByScheduler();

            TestLogger.LogDebug(CreateReport());
        }

        private void testEventFiresOnCorrectNumberOfFrames()
        {
            bool didEventFire = false;
            ScheduledEvent timedEvent = null;

            timedEvent = UtilityCore.Scheduler.Schedule(() =>
            {
                didEventFire = true;
                TestCase test = this;
                FrameTimer timer = timedEvent.EventTimer;

                //Assert that event fired on the correct interval
                test.AssertThat(timer.Ticks).IsEqualTo(EVENT_INTERVAL);
            }, EVENT_INTERVAL);

            for (int i = 0; i < EVENT_INTERVAL; i++)
                UtilityCore.Scheduler.Update(); //Simulate a scheduler update

            timedEvent.Cancel();
            AssertThat(didEventFire).IsTrue();
        }

        private void testEarlyInvocationDelaysNextEventByOneCompleteInterval()
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
                    test.AssertThat(timer.Ticks).IsEqualTo(EVENT_INTERVAL * 2);
                }
                didEventFire = true;
            }, EVENT_INTERVAL);

            UtilityCore.Scheduler.Update();
            timedEvent.InvokeEarly();

            short expectedWaitFrames = EVENT_INTERVAL + EVENT_INTERVAL - 1;
            for (int i = 0; i < expectedWaitFrames; i++)
                UtilityCore.Scheduler.Update(); //Simulate a scheduler update

            timedEvent.Cancel();
            AssertThat(didEventFire).IsTrue();
        }

        private void testTimerIsGarbageCollectedWhenEventIsCancelled()
        {
            ScheduledEvent timedEvent = UtilityCore.Scheduler.Schedule(() => { }, EVENT_INTERVAL);

            int entryCountBeforeEventWasHandled = UtilityCore.Scheduler.Timers.UnsafeCount();

            for (int i = 0; i < EVENT_INTERVAL; i++)
                UtilityCore.Scheduler.Update();

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
                UtilityCore.Scheduler.Update();
            }
            AssertThat(eventDisposed).IsTrue();
        }

        private void testTimerIsGarbageCollectedWhenItGoesOutOfScopeAndNotHeldByScheduler()
        {
            createTimersInPrivateScope();
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
                UtilityCore.Scheduler.Update();
            }
            AssertThat(eventDisposed).IsTrue();
        }

        private void createTimersInPrivateScope()
        {
            for (int i = 0; i < 500; i++)
                new FrameTimer(EVENT_INTERVAL);
        }
    }
}
