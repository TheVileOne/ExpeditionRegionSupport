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
            bool triggeredEventsExist = false;

            foreach (ScheduledEvent e in Events)
            {
                e.OnFrameReached(FrameCount);

                if (e.Triggered)
                    triggeredEventsExist = true;
            }

            if (triggeredEventsExist)
                Events = Events.Where(e => !e.Triggered).ToList();

            FrameCount++;
        }
    }

    public class ScheduledEvent
    {
        public event Action Event;

        public long StartFrame;

        public long EndFrame => StartFrame + Duration;

        public long Duration;

        public bool Triggered;

        public ScheduledEvent(Action action, long startFrame, long duration)
        {
            Event = action;
            StartFrame = startFrame;
            Duration = duration;
        }

        public void OnFrameReached(long frame)
        {
            if (EndFrame >= frame)
            {
                Event.Invoke();
                Triggered = true;
            }
        }
    }
}
