﻿using LogUtils.Enums;

namespace LogUtils.Threading
{
    public sealed class FileLock : Lock
    {
        /// <summary>
        /// The last known file change/access activity that resulted in acquiring the file lock  
        /// </summary>
        public FileAction LastActivity { get; private set; }

        public void SetActivity(LogID logID, FileAction activity)
        {
            if (activity == LastActivity) return;

#if DEBUG
            //The activity log should be immune from activity reporting
            if (logID != LogID.FileActivity)
                UtilityLogger.LogActivity(FileActivityStringFormatter.Default.GetFormat(logID, activity));
#endif
            LastActivity = activity;
        }
    }
}
