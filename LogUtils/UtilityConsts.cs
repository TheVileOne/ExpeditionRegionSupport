using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogUtils
{
    public static class UtilityConsts
    {
        public static class DataFields
        {
            public const int EXPECTED_FIELD_COUNT = 18;

            public const string LOGID = "logid";
            public const string FILENAME = "filename";
            public const string ALTFILENAME = "altfilename";
            public const string TAGS = "tags";
            public const string VERSION = "version";
            public const string PATH = "path";
            public const string ORIGINAL_PATH = "origpath";
            public const string LAST_KNOWN_PATH = "lastknownpath";
            public const string LOGS_FOLDER_AWARE = "logsfolderaware";
            public const string LOGS_FOLDER_ELIGIBLE = "logsfoldereligible";
            public const string SHOW_LOGS_AWARE = "showlogsaware";
            public const string CUSTOM = "custom";

            public static class Intro
            {
                public const string MESSAGE = "intro_message";
                public const string TIMESTAMP = "intro_timestamp";
            }

            public static class Outro
            {
                public const string MESSAGE = "outro_message";
                public const string TIMESTAMP = "outro_timestamp";
            }

            public static class Rules
            {
                public const string HEADER = "logrules";
                public const string SHOW_LINE_COUNT = "showlinecount";
                public const string SHOW_CATEGORIES = "showcategories";
            }
        }

        public static class ComponentTags
        {
            public const string PROPERTY_DATA = "LogProperties";
            public const string SCHEDULER = "Scheduler";
            public const string SHARED_DATA = "SharedData";
            public const string REQUEST_DATA = "RequestData";
        }
    }
}
