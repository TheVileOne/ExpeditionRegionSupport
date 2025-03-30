using System;
using System.Collections.Generic;

namespace LogUtils.Timers
{
    public class EventScheduler : UtilityComponent
    {
        /// <summary>
        /// Events waiting to be scheduled
        /// </summary>
        private Queue<ScheduledEvent> pendingEvents = new Queue<ScheduledEvent>();

        private List<ScheduledEvent> scheduledEvents = new List<ScheduledEvent>();

        internal WeakReferenceCollection<FrameTimer> Timers = new WeakReferenceCollection<FrameTimer>();

        public override string Tag => UtilityConsts.ComponentTags.SCHEDULER;

        public EventScheduler()
        {
            enabled = true;
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
            if (frameInterval < 0)
                throw new ArgumentOutOfRangeException(nameof(frameInterval) + " must be greater than zero");

            ScheduledEvent pendingEvent = new ScheduledEvent(action, frameInterval, invokeLimit);
            lock (this)
            {
                pendingEvents.Enqueue(pendingEvent);
            }
            return pendingEvent;
        }

        public void Update()
        {
            //Add pending events in a threadsafe way
            while (pendingEvents.Count > 0)
                scheduledEvents.Add(pendingEvents.Dequeue());

            foreach (FrameTimer timer in Timers)
            {
                ScheduledEvent timedEvent = timer.Event;

                if (timedEvent != null && timedEvent.Cancelled)
                {
                    //Release event resources
                    scheduledEvents.Remove(timedEvent);
                    continue;
                }
                timer.Update();
            }
        }
    }

    public class ScheduledEvent
    {
        public bool Cancelled { get; private set; }

        public event Action Event;

        internal FrameTimer EventTimer;

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

        public ScheduledEvent(Action frameEvent, int frameInterval, int invokeLimit = -1)
        {
            Event = frameEvent;
            InvokeLimit = invokeLimit;

            EventTimer = new FrameTimer(frameInterval)
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
            Cancelled = true;
            EventTimer.OnInterval -= Event;
            EventTimer.Stop();
        }
    }
}
