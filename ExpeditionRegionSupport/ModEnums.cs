﻿using LogUtils;

namespace ExpeditionRegionSupport
{
    public static class ModEnums
    {
        public class LogID : LogUtils.LogID
        {
            public static readonly LogID ErsLog = new LogID("ErsLog", LogAccess.FullAccess, true);

            public LogID(string filename, LogAccess access, bool register = false) : base(filename, access, register)
            {
                Properties.ShowCategories.IsEnabled = true;
            }
        }
    }
}