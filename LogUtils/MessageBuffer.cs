using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Helpers;
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

        /// <summary>
        /// Provides scopes for entering and exiting different contexts that need the buffer
        /// </summary>
        public ScopeProvider Scope = new ScopeProvider(false);

        public MessageBuffer()
        {
            Content = new StringBuilder();
            Scope.OnEnter += onScopeChanged;
            Scope.OnExit += onScopeChanged;
        }

        private void onScopeChanged(object obj)
        {
            Signal();
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
            //UtilityLogger.DebugLog("Entering context " + context);
            Scope.Enter(context);
        }

        public void LeaveContext(BufferContext context)
        {
            //UtilityLogger.DebugLog("Leaving context " + context);
            Scope.Exit(context);
        }

        public bool IsEntered(BufferContext context)
        {
            return Scope[context]?.EnterCount > 0;
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
                if (Scope.Entered)
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
