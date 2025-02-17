using LogUtils.Enums;

namespace LogUtils.Threading
{
    public sealed class FileLock
    {
        /// <summary>
        /// The last known file change/access activity that resulted in acquiring the file lock  
        /// </summary>
        public FileAction LastActivity { get; private set; }

        public void SetActivity(LogID logID, FileAction activity)
        {
            if (activity == LastActivity) return;

            //The activity log should be immune from activity reporting
            if (logID != LogID.FileActivity)
                UtilityLogger.LogActivity(FileActivityStringFormatter.Default.GetFormat(logID, activity));

            LastActivity = activity;
        }
    }
}
