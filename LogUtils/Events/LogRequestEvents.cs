using LogUtils.Requests;

namespace LogUtils.Events
{
    public static class LogRequestEvents
    {
        public static LogRequestEventHandler OnSubmit;
        public static LogRequestEventHandler OnStatusChange;
    }

    public delegate void LogRequestEventHandler(LogRequest request);
}
