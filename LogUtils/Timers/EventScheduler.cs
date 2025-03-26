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

        public override string Tag => UtilityConsts.ComponentTags.SCHEDULER;

        public EventScheduler()
        {
            enabled = true;
        }

        public ScheduledEvent Schedule(Action action, int frameInterval)
        {
            if (frameInterval < 0)
                throw new ArgumentOutOfRangeException(nameof(frameInterval) + " must be greater than zero");

            ScheduledEvent pendingEvent = new ScheduledEvent(action, frameInterval);
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
                if (e.Cancelled)
                    eventCleanupRequired = true;
            }

            if (eventCleanupRequired)
                scheduledEvents.RemoveAll(e => e.Cancelled);
        }
    }

    public class ScheduledEvent
    {
        public event Action Event;

        public bool Cancelled;

        public ScheduledEvent(Action action, int frameInterval)
        {
            Event = action;
        }

        public void Cancel()
        {
            Cancelled = true;
        }
    }
}
