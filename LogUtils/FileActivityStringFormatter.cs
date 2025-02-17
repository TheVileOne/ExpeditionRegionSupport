using LogUtils.Enums;
using System;

namespace LogUtils
{
    public class FileActivityStringFormatter
    {
        public static FileActivityStringFormatter Default = new FileActivityStringFormatter();

        public FormattableString GetFormat(FileAction activity)
        {
            return GetFormat(fileContext: null, activity);
        }

        public FormattableString GetFormat(LogID logID, FileAction activity)
        {
            if (logID == null)
                return GetFormat(fileContext: null, activity);

            return GetFormat(fileContext: logID.Properties?.CurrentFilename, activity);
        }

        public FormattableString GetFormat(string fileContext, FileAction activity)
        {
            string appendString = getActivityString(activity);

            if (string.IsNullOrEmpty(fileContext))
                return $"File {appendString}";

            return $"File {fileContext} {appendString}";
        }

        private string getActivityString(FileAction activity)
        {
            switch (activity)
            {
                case FileAction.Write:
                    return "write in process";
                case FileAction.SessionStart:
                    return "started";
                case FileAction.SessionEnd:
                    return "ended";
                case FileAction.Open:
                    return "opened";
                case FileAction.PathUpdate:
                    return "path updated";
                case FileAction.Move:
                    return "moved";
                case FileAction.Copy:
                    return "copied";
                case FileAction.Create:
                    return "created";
                case FileAction.Delete:
                    return "deleted";
                case FileAction.StreamDisposal:
                    return "stream disposed";
                case FileAction.None:
                default:
                    return "accessed";
            }
        }
    }
}
