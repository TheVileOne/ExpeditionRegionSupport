using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Helpers;
using LogUtils.Timers;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace LogUtils
{
    public class MessageBuffer
    {
        protected StringBuilder Content;

        public bool HasContent => Content.Length > 0;

        /// <summary>
        /// When true, the buffer will be added to instead of writing to file on handling a write request
        /// </summary>
        public bool IsBuffering { get; protected set; }

        public ICollection<PollingTimer> ActivityListeners = new List<PollingTimer>();

        protected ICollection<BufferContext> Scopes = new HashSet<BufferContext>();

        public MessageBuffer()
        {
            Content = new StringBuilder();
        }

        public void AppendMessage(string message)
        {
            Content.AppendLine(message);
        }

        public void Clear()
        {
            Content.Clear();
        }

        public void EnterContext(BufferContext context)
        {
            Scopes.Add(context);
            Signal();
        }

        public void LeaveContext(BufferContext context)
        {
            Scopes.Remove(context);
            Signal();
        }

        public bool IsEntered(BufferContext context)
        {
            return Scopes.Contains(context);
        }

        public PollingTimer PollForActivity(SignalEventHandler onSignal, EventHandler<Timer, ElapsedEventArgs> onTimeout, double timeoutInMilliseconds)
        {
            PollingTimer listener = new PollingTimer(timeoutInMilliseconds);

            if (onSignal != null)
                listener.OnSignal += onSignal;

            if (onTimeout != null)
                listener.OnTimeout += onTimeout;
            listener.OnTimeout += eventTimeout;

            listener.Start();
            ActivityListeners.Add(listener);
            return listener;

            void eventTimeout(Timer timer, ElapsedEventArgs e)
            {
                if (onSignal != null)
                    listener.OnSignal -= onSignal;

                if (onTimeout != null)
                    listener.OnTimeout -= onTimeout;

                listener.OnTimeout -= eventTimeout;
                ActivityListeners.Remove(listener);
            }
        }

        public bool SetState(bool state, BufferContext context)
        {
            if (state == true)
            {
                EnterContext(context);
            }
            else
            {
                LeaveContext(context);

                //Only disable the buffer when all scopes have exited
                if (Scopes.Count > 0)
                    return false;
            }

            IsBuffering = state;
            return true;
        }

        protected void Signal()
        {
            foreach (var listener in ActivityListeners)
                listener.Signal();
        }

        public override string ToString()
        {
            return StringParser.TrimNewLine(Content.ToString());
        }
    }
}
