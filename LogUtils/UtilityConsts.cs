using LogUtils.Helpers;

namespace LogUtils
{
    public static class UtilityConsts
    {
        public const int FILTER_MAX = 1000;
        public const string UTILITY_NAME = "LogUtils";

        public static class DataFields
        {
            public const int EXPECTED_FIELD_COUNT = 18;

            public readonly static string[] OrderedFields;

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

            /// <summary>
            /// Case sensitive comparison of a string against known utility DataFields
            /// </summary>
            public static bool IsRecognizedField(string match)
            {
                //CUSTOM intentionally left out of this check - it is an optional line
                switch (match)
                {
                    case LOGID:
                    case FILENAME:
                    case ALTFILENAME:
                    case TAGS:
                    case VERSION:
                    case PATH:
                    case ORIGINAL_PATH:
                    case LAST_KNOWN_PATH:
                    case Intro.MESSAGE:
                    case Intro.TIMESTAMP:
                    case Outro.MESSAGE:
                    case Outro.TIMESTAMP:
                    case LOGS_FOLDER_AWARE:
                    case LOGS_FOLDER_ELIGIBLE:
                    case SHOW_LOGS_AWARE:
                    case Rules.HEADER:
                    case Rules.SHOW_LINE_COUNT:
                    case Rules.SHOW_CATEGORIES:
                        return true;
                    default:
                        return false;
                };
            }

            static DataFields()
            {
                OrderedFields = new[]
                {
                    LOGID,
                    FILENAME,
                    ALTFILENAME,
                    TAGS,
                    VERSION,
                    PATH,
                    ORIGINAL_PATH,
                    LAST_KNOWN_PATH,
                    LOGS_FOLDER_AWARE,
                    LOGS_FOLDER_ELIGIBLE,
                    SHOW_LOGS_AWARE,
                    Intro.MESSAGE,
                    Intro.TIMESTAMP,
                    Outro.MESSAGE,
                    Outro.TIMESTAMP,
                    Rules.HEADER,
                    Rules.SHOW_LINE_COUNT,
                    Rules.SHOW_CATEGORIES,
                    CUSTOM
                };
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
            public const string Unknown = "unknown"; //Fallback log name - log file doesn't exist

            public const string BepInExAlt = "mods";
            public const string ExceptionAlt = "exception";
            public const string ExpeditionAlt = "expedition";
            public const string JollyCoopAlt = "jolly";
            public const string UnityAlt = "console";

            /// <summary>
            /// Evaluates whether given name is a match to a game log filename
            /// </summary>
            public static bool NameMatch(string name)
            {
                return name.MatchAny(EqualityComparer.StringComparerIgnoreCase, BepInEx, Exception, Expedition, JollyCoop, Unity);
            }
        }

        public static class FilterKeywords
        {
            public const string KEYWORD_PREFIX = "fk:"; //Used by the parser
            public const string REGEX = "regex";
            public const string ACTIVATION_PERIOD_STARTUP = "time_startup";
        }

        public static class PathKeywords
        {
            public const string ROOT = "root";
            public const string STREAMING_ASSETS = "customroot";
        }
    }
}
