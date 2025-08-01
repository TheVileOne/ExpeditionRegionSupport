using LogUtils.Enums;
using LogUtils.Formatting;

namespace LogUtils.Threading
{
    public sealed class FileLock : Lock
    {
        /// <summary>
        /// The associated log file context
        /// </summary>
        public LogID FileContext => Context as LogID;

        /// <summary>
        /// The last known file change/access activity that resulted in acquiring the file lock  
        /// </summary>
        public FileAction LastActivity { get; private set; }

        public FileLock() : base()
        {
        }

        public FileLock(object context) : base(context)
        {
        }

        public FileLock(ContextProvider contextProvider) : base(contextProvider)
        {
        }

        public void SetActivity(FileAction activity)
        {
            if (activity == LastActivity) return;

            LogID context = FileContext;

            //The activity log should be immune from activity reporting
            if (context != LogID.FileActivity)
                UtilityLogger.LogActivity(FileActivityStringFormatter.Default.GetFormat(context, activity));

            LastActivity = activity;
        }
    }
}
