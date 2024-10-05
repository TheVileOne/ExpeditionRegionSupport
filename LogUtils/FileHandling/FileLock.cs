using static LogUtils.FileHandling.FileEnums;

namespace LogUtils.FileHandling
{
    public sealed class FileLock
    {
        private static readonly LogID activityLog = new LogID("LogActivity", UtilityConsts.PathKeywords.ROOT, false);
        private static readonly Logger activityLogger = new Logger(activityLog);

        /// <summary>
        /// The last known file change/access activity that resulted in acquiring the file lock  
        /// </summary>
        public FileAction LastActivity { get; private set; }

        public void SetActivity(LogID logID, FileAction activity)
        {
            if (activity == LastActivity) return;

            //The activity log should be immune from activity reporting
            if (logID != activityLog)
                activityLogger.Log(string.Format(getResourceString(activity), logID.Properties.CurrentFilename));
        }

        private static string getResourceString(FileAction activity)
        {
            string resourceFormat = "Log file {0} ";

            string appendString;
            switch (activity)
            {
                case FileAction.Log:
                    appendString = "updated";
                    break;
                case FileAction.SessionStart:
                    appendString = "started";
                    break;
                case FileAction.SessionEnd:
                    appendString = "ended";
                    break;
                case FileAction.PathUpdate:
                    appendString = "path updated";
                    break;
                case FileAction.Move:
                    appendString = "moved";
                    break;
                case FileAction.Copy:
                    appendString = "copied";
                    break;
                case FileAction.Create:
                    appendString = "created";
                    break;
                case FileAction.Delete:
                    appendString = "deleted";
                    break;
                default:
                    appendString = "accessed";
                    break;
            }
            return resourceFormat + appendString;
        }
    }
}
