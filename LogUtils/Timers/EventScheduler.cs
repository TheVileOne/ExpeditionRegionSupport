using LogUtils.Events;
using LogUtils.Threading;
using System;
using System.Collections.Generic;

namespace LogUtils.Timers
{
    public class EventScheduler : UtilityComponent, IDisposable
    {
        public Lock EventLock = new Lock();

        private Queue<ScheduledEvent> pendingEvents = new Queue<ScheduledEvent>();
        private Queue<FrameTimer> pendingTimers = new Queue<FrameTimer>();

        /// <summary>
        /// Maintain a strong list of ScheduledEvents to prevent their associated FrameTimers from disposing
        /// </summary>
        private List<ScheduledEvent> scheduledEvents = [];

        internal WeakReferenceCollection<FrameTimer> Timers = [];

        public override string Tag => UtilityConsts.ComponentTags.SCHEDULER;

        public EventScheduler()
        {
            enabled = true;
            FrameTimer.OnRelease += onTimerReleased;
        }

        /// <summary>
        /// Adds a ScheduledEvent to be managed by the current instance
        /// </summary>
        internal void AddEvent(ScheduledEvent pendingEvent)
        {
            using (EventLock.Acquire())
                pendingEvents.Enqueue(pendingEvent);
        }

        /// <summary>
        /// Adds a timer to be managed by the current instance
        /// </summary>
        internal void AddTimer(FrameTimer timer)
        {
            using (EventLock.Acquire())
                pendingTimers.Enqueue(timer);
        }

        /// <summary>
        /// Schedules an event delegate to be invoked periodically after a specified number of frames 
        /// </summary>
        /// <param name="action">The delegate to invoke</param>
        /// <param name="frameInterval">The number of frames in between event invocations</param>
        /// <param name="invokeLimit">The maximum number of invocations to attempt</param>
        /// <returns>An object containing the event state</returns>
        /// <exception cref="ArgumentOutOfRangeException">The frame interval is an invalid value</exception>
        public ScheduledEvent Schedule(Action action, int frameInterval, int invokeLimit = -1)
        {
            return Schedule(action, frameInterval, syncToRainWorld: false, invokeLimit);
        }

        /// <summary>
        /// Schedules an event delegate to be invoked periodically after a specified number of frames 
        /// </summary>
        /// <param name="action">The delegate to invoke</param>
        /// <param name="frameInterval">The number of frames in between event invocations</param>
        /// <param name="syncToRainWorld">When true, event will be handled in MainLoopProcess.Update instead of through EventSceduler.Update</param>
        /// <param name="invokeLimit">The maximum number of invocations to attempt</param>
        /// <returns>An object containing the event state</returns>
        /// <exception cref="ArgumentOutOfRangeException">The frame interval is an invalid value</exception>
        public ScheduledEvent Schedule(Action action, int frameInterval, bool syncToRainWorld, int invokeLimit = -1)
        {
            if (frameInterval < 0)
                throw new ArgumentOutOfRangeException(nameof(frameInterval) + " must be greater than zero");

            ScheduledEvent pendingEvent = new ScheduledEvent(action, frameInterval, syncToRainWorld, invokeLimit);

            AddEvent(pendingEvent);
            return pendingEvent;
        }

        public void Update()
        {
            bool hasPendingObjects = pendingTimers.Count > 0 || pendingEvents.Count > 0;

            if (hasPendingObjects)
            {
                using (EventLock.Acquire())
                {
                    while (pendingEvents.Count > 0)
                        scheduledEvents.Add(pendingEvents.Dequeue());

                    while (pendingTimers.Count > 0)
                    {
                        FrameTimer timer = pendingTimers.Dequeue();

                        if (timer.IsSynchronous)
                        {
                            timer.SyncHandler = (s, e) =>
                            {
                                timer.Update();
                            };
                            UtilityEvents.OnNewUpdateSynced += timer.SyncHandler;
                        }
                        Timers.Add(timer);
                    }
                }
            }

            foreach (FrameTimer timer in Timers)
            {
                if (!timer.IsSynchronous) //Timer is updated through a separate delegate
                    timer.Update();

                ScheduledEvent timedEvent = timer.Event;

                if (timedEvent?.Cancelled == true)
                    timer.Release();
            }
        }

        #region Dispose pattern

        private bool isDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    //Release any remaining timers
                    foreach (FrameTimer timer in Timers)
                        timer.Release();
                    FrameTimer.OnRelease -= onTimerReleased;
                }
                isDisposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion

        private void onTimerReleased(FrameTimer timer, EventArgs e)
        {
            ScheduledEvent timedEvent = timer.Event;

            using (EventLock.Acquire())
            {
                //Release event resources
                scheduledEvents.Remove(timedEvent);

                if (timer.IsSynchronous)
                    UtilityEvents.OnNewUpdateSynced -= timer.SyncHandler;
            }
        }
    }

    public class ScheduledEvent
    {
        public bool Cancelled { get; private set; }

        public event Action Event;

        public FrameTimer EventTimer { get; private set; }

        /// <summary>
        /// The number of times this event has been fired
        /// </summary>
        public int InvokeCount { get; private set; }

        /// <summary>
        /// The amount of times event may be invoked
        /// </summary>
        public int InvokeLimit = -1;

        public string Name;

        private bool eventHandledEarly;

        public ScheduledEvent(Action frameEvent, int frameInterval, bool syncToRainWorld, int invokeLimit = -1)
        {
            Event = frameEvent;
            InvokeLimit = invokeLimit;

            EventTimer = new FrameTimer(frameInterval, syncToRainWorld)
            {
                Event = this
            };
            EventTimer.OnInterval += onEvent;
            EventTimer.Start();
        }

        public void InvokeEarly()
        {
            if (Cancelled) return;

            onEvent();
            eventHandledEarly = true;
        }

        private void onEvent()
        {
            //UtilityLogger.DebugLog("--------------------- EVENT FIRED ---------------------------");
            //UtilityLogger.DebugLog("TIMERS " + UtilityCore.Scheduler.Timers.UnsafeCount());
            //UtilityLogger.DebugLog("----------------------- TESTING -----------------------------");

            //We do not want to handle an event at its scheduled frame if it was handled on an earlier frame
            if (eventHandledEarly)
            {
                eventHandledEarly = false;
                return;
            }

            try
            {
                Event?.Invoke();
            }
            finally
            {
                InvokeCount++;

                bool invocationLimitReached = InvokeLimit >= 0 && InvokeCount >= InvokeLimit;

                if (invocationLimitReached)
                    Cancel();
            }
        }

        public void Cancel()
        {
            if (Cancelled) return;

            Cancelled = true;
            EventTimer.OnInterval -= Event;
            EventTimer.Stop();
        }
    }
}
