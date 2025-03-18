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
        public bool IsBuffering;

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

        public override string ToString()
        {
            return Content.ToString();
        }
    }
}
