namespace LogUtils
{
    public static class UtilityConsts
    {
        public const string UTILITY_NAME = "LogUtils";

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

        public static class LogNames
        {
            public const string BepInEx = "LogOutput";
            public const string Exception = "exceptionLog";
            public const string Expedition = "ExpLog";
            public const string JollyCoop = "jollyLog";
            public const string Unity = "consoleLog";

            public const string BepInExAlt = "mods";
            public const string ExceptionAlt = "exception";
            public const string ExpeditionAlt = "expedition";
            public const string JollyCoopAlt = "jolly";
            public const string UnityAlt = "console";
        }

        public static class PathKeywords
        {
            public const string ROOT = "root";
            public const string STREAMING_ASSETS = "customroot";
        }
    }
}
