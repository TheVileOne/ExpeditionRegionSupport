using LogUtils.Enums;
using LogUtils.Enums.FileSystem;
using System;

namespace LogUtils.Formatting
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

#pragma warning disable IDE0055 //Fix formatting
        private string getActivityString(FileAction activity)
        {
            return activity switch
            {
                FileAction.Write          => "write in process",
                FileAction.Buffering      => "is buffering",
                FileAction.SessionStart   => "started",
                FileAction.SessionEnd     => "ended",
                FileAction.Open           => "opened",
                FileAction.PathUpdate     => "path updated",
                FileAction.Move           => "moved",
                FileAction.Copy           => "copied",
                FileAction.Create         => "created",
                FileAction.Delete         => "deleted",
                FileAction.StreamDisposal => "stream disposed",
                _                         => "accessed",
            };
        }
#pragma warning restore IDE0055 //Fix formatting
    }
}
