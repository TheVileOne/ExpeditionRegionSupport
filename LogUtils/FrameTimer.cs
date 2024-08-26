using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LogUtils
{
    public class FrameTimer : UtilityComponent
    {
        public long FrameCount = 0;

        public List<ScheduledEvent> Events = new List<ScheduledEvent>();

        public override string Tag => UtilityConsts.ComponentTags.SCHEDULER;

        public FrameTimer()
        {
            enabled = true;
        }

        public ScheduledEvent Schedule(Action action, long frameDelay)
        {
            if (frameDelay < 0)
                throw new ArgumentException(nameof(frameDelay) + " must be positive");

            Events.Add(new ScheduledEvent(action, FrameCount, frameDelay));
            return Events.Last();
        }

        public void Update()
        {
            bool eventCleanupRequired = false;

            foreach (ScheduledEvent e in Events)
            {
                e.OnFrameReached(FrameCount);

                if (e.Triggered || e.Cancelled)
                    eventCleanupRequired = true;
            }

            if (eventCleanupRequired)
                Events = Events.Where(e => !e.Triggered && !e.Cancelled).ToList();

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
