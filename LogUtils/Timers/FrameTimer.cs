using System;
using System.Collections.Generic;

namespace LogUtils.Timers
{
    public class FrameTimer : UtilityComponent
    {
        public long FrameCount = 0;

        /// <summary>
        /// Events waiting to be scheduled
        /// </summary>
        private Queue<ScheduledEvent> pendingEvents = new Queue<ScheduledEvent>();

        private List<ScheduledEvent> scheduledEvents = new List<ScheduledEvent>();

        public override string Tag => UtilityConsts.ComponentTags.SCHEDULER;

        public FrameTimer()
        {
            enabled = true;
        }

        public ScheduledEvent Schedule(Action action, long frameDelay)
        {
            if (frameDelay < 0)
                throw new ArgumentException(nameof(frameDelay) + " must be positive");

            ScheduledEvent pendingEvent = new ScheduledEvent(action, FrameCount, frameDelay);
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

            bool eventCleanupRequired = false;

            foreach (ScheduledEvent e in scheduledEvents)
            {
                e.OnFrameReached(FrameCount);

                if (e.Triggered || e.Cancelled)
                    eventCleanupRequired = true;
            }

            if (eventCleanupRequired)
                scheduledEvents.RemoveAll(e => e.Triggered || e.Cancelled);
            FrameCount++;
        }
    }

    public class ScheduledEvent
    {
        public event Action Event;

        public long StartFrame;

        public long EndFrame => StartFrame + Duration;

        public long Duration;

        public bool Cancelled;

        public bool Triggered;

        public ScheduledEvent(Action action, long startFrame, long duration)
        {
            Event = action;
            StartFrame = startFrame;
            Duration = duration;
        }

        public void Cancel()
        {
            Cancelled = true;
        }

        public void OnFrameReached(long frame)
        {
            if (Cancelled) return;

            if (EndFrame >= frame)
            {
                Event.Invoke();
                Triggered = true;
            }
        }
    }
}
