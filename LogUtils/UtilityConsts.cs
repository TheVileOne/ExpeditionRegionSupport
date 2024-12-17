using LogUtils.Helpers;
using LogUtils.Helpers.Comparers;

namespace LogUtils
{
    public static class UtilityConsts
    {
        public const int CUSTOM_LOGTYPE_LIMIT = 1000;
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
            public const string PERSISTENCE_MANAGER = "PersistenceManager";
            public const string PROPERTY_DATA = "LogProperties";
            public const string REQUEST_DATA = "RequestData";
            public const string SCHEDULER = "Scheduler";
            public const string SHARED_DATA = "SharedData";
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
                return name.MatchAny(ComparerUtils.StringComparerIgnoreCase, BepInEx, Exception, Expedition, JollyCoop, Unity);
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

        public static class AssertMessages
        {
            public const string CONDITION_TRUE = "{0} is true";
            public const string CONDITION_FALSE = "{0} is false";
            public const string VALUES_EQUAL = "{0} are equal";
            public const string VALUES_NOT_EQUAL = "{0} are not equal";
            public const string VALUE_NULL = "{0} is null";
            public const string VALUE_NOT_NULL = "{0} is not null";
            public const string VALUE_ABOVE = "{0} is above {1}";
            public const string VALUE_ABOVE_OR_EQUAL = "{0} is above or equal to {1}";
            public const string VALUE_NOT_ABOVE = "{0} is not above {1}";
            public const string VALUE_BELOW = "{0} is below {1}";
            public const string VALUE_BELOW_OR_EQUAL = "{0} is below or equal to {1}";
            public const string VALUE_NOT_BELOW = "{0} is not below {1}";
            public const string VALUE_IN_RANGE = "{0} in range";
            public const string VALUE_OUT_OF_RANGE = "{0} not in range";
            public const string VALUE_ZERO = "{0} is zero";
            public const string VALUE_NOT_ZERO = "{0} is not zero";
            public const string VALUE_NEGATIVE = "{0} is negative";
            public const string VALUE_NOT_NEGATIVE = "{0} is not negative";
            public const string VALUE_POSITIVE = "{0} is positive";
            public const string VALUE_NOT_POSITIVE = "{0} is not positive";
            public const string VALUE_NOT_A_NUMBER = "{0} is not a number";
            public const string EXPECTED_VALUE = "{0} is expected";
            public const string UNEXPECTED_VALUE = "{0} is not expected";
            public const string COLLECTION_EMPTY = "{0} is empty";
            public const string COLLECTION_HAS_ITEMS = "{0} has items";
        }
    }
}
