using LogUtils.Events;

namespace LogUtils.Requests
{
    public class ConsoleLogRequest : LogRequest
    {
        public ConsoleLogRequest(LogMessageEventArgs data) : base(RequestType.Console, data)
        {
        }
    }
}
