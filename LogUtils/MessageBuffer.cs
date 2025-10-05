using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Formatting;
using LogUtils.Timers;
using System;
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

        public void Append(string value)
        {
            try
            {
                Content.Append(value);
            }
            catch (ArgumentOutOfRangeException)
            {
                UtilityLogger.LogWarning("Write buffer exceeded capacity");
            }
        }

        public void AppendLine(string value)
        {
            try
            {
                Content.AppendLine(value);
            }
            catch (ArgumentOutOfRangeException)
            {
                UtilityLogger.LogWarning("Write buffer exceeded capacity");
            }
        }

        public void Clear()
        {
            Content.Clear();
        }

        public void EnterContext(BufferContext context)
        {
            //UtilityLogger.DebugLog($"Entering context: {context}");
            Scopes.Add(context);
            Signal();
        }

        public void LeaveContext(BufferContext context)
        {
            //UtilityLogger.DebugLog($"Leaving context: {context}");
            Scopes.Remove(context);
            Signal();
        }

        public bool IsEntered(BufferContext context)
        {
            return Scopes.Contains(context);
        }

        public PollingTimer PollForActivity(SignalEventHandler onSignal, EventHandler<PollingTimer, ElapsedEventArgs> onTimeout, TimeSpan timeout)
        {
            PollingTimer listener = new PollingTimer(timeout.TotalMilliseconds);

            if (onSignal != null)
                listener.OnSignal += onSignal;

            if (onTimeout != null)
                listener.OnTimeout += onTimeout;
            listener.OnTimeout += eventTimeout;

            listener.Start();
            ActivityListeners.Add(listener);
            return listener;

            void eventTimeout(PollingTimer timer, ElapsedEventArgs e)
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
