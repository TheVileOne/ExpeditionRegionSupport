using LogUtils.Enums;
using System.Collections.Generic;
using System.Text;

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
        }

        public void LeaveContext(BufferContext context)
        {
            Scopes.Remove(context);
        }

        public bool IsEntered(BufferContext context)
        {
            return Scopes.Contains(context);
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

        public override string ToString()
        {
            return Content.ToString();
        }
    }
}
