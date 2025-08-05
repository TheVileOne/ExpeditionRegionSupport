using LogUtils.Helpers.Comparers;
using LogUtils.Helpers.Extensions;
using LogUtils.Policy;

#pragma warning disable CS1591 //Missing XML comment for publicly visible type or member
namespace LogUtils
{
    public static class UtilityConsts
    {
        public const int CUSTOM_LOGTYPE_LIMIT = int.MaxValue;
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
                public const string LOG_DUMP = "logdump";
                public const string SHOW_CATEGORIES = "showcategories";
                public const string SHOW_LINE_COUNT = "showlinecount";
            }

            /// <summary>
            /// Case sensitive comparison of a string against known utility DataFields
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0066:Convert switch statement to expression", Justification = "Switch expression is worse")]
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
                }
            }

            static DataFields()
            {
                OrderedFields = new string[]
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

        public static class ConfigCategory
        {
            public const string Debug = "Debug";
            public const string Logging = "Logging";
            public const string LogRequests = "Logging.Requests";
            public const string Patcher = "Patcher";
            public const string Testing = "Testing";
            public const string Asserts = "Testing.Asserts";
        }

        public static class PolicyNames
        {
            public static class Debug
            {
                public const string Mode = nameof(DebugPolicy.DebugMode);
                public const string ShowDebugLog = nameof(DebugPolicy.ShowDebugLog);
                public const string ShowActivityLog = nameof(DebugPolicy.ShowActivityLog);
            }

            public static class Patcher
            {
                public const string HasAskedForPermission = nameof(PatcherPolicy.HasAskedForPermission);
                public const string ShouldDeploy = nameof(PatcherPolicy.ShouldDeploy);
                public const string ShowPatcherLog = nameof(PatcherPolicy.ShowPatcherLog);
            }

            public static class Testing
            {
                public const string PreferExpectationsAsFailures = nameof(TestCasePolicy.PreferExpectationsAsFailures);
                public const string FailuresAreAlwaysReported = nameof(TestCasePolicy.FailuresAreAlwaysReported);
                public const string ReportVerbosity = nameof(TestCasePolicy.ReportVerbosity);
                public const string AssertsEnabled = "Enabled";
            }

            public static class LogRequests
            {
                public const string ShowRejectionReasons = nameof(LogRequestPolicy.ShowRejectionReasons);
            }
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

        public static class ResourceNames
        {
            public const string PATCHER = "LogUtils.VersionLoader";
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

        public static class AssertResponse
        {
            public const string FAIL = "Assertion failed";
            public const string PASS = "Assertion passed";

            //Boolean data
            public const string MUST_BE_TRUE = "{0} must be true";
            public const string MUST_BE_FALSE = "{0} must be false";

            //Object and struct data
            public const string MUST_BE_EQUAL = "{0} must be equivalent";
            public const string MUST_NOT_BE_EQUAL = "{0} must not be equivalent";
            public const string MUST_BE_SAME_INSTANCE = "{0} must match the specified instance";
            public const string MUST_NOT_BE_SAME_INSTANCE = "{0} must be a different instance";
            public const string MUST_BE_NULL = "{0} must be null";
            public const string MUST_NOT_BE_NULL = "{0} must not be null";

            //Collection data
            public const string NOT_ENOUGH_ENTRIES = "{0} does not have enough entries";
            public const string TOO_MANY_ENTRIES = "{0} has too many entries";

            public const string MUST_CONTAIN = "{0} must contain {1}";
            public const string MUST_NOT_CONTAIN = "{0} must not contain {1}";
            public const string MUST_ONLY_CONTAIN = "{0} must only contain {1}";
            public const string MUST_BE_EMPTY = "{0} must be empty";
            public const string MUST_HAVE_ITEMS = "{0} must not be null or empty";

            //Numeric data
            public const string TOO_LOW = "{0} is too low";
            public const string TOO_HIGH = "{0} is too high";

            //public const string MUST_BE_HIGHER = "{0} must be higher than {1}";
            //public const string MUST_BE_LOWER = "{0} must be lower than {1}";

            //public const string MUST_NOT_BE_HIGHER = "{0} must not be higher than {1}";
            //public const string MUST_NOT_BE_LOWER = "{0} must not be lower than {1}";
            public const string MUST_NOT_BE_ZERO = "{0} must not be zero";

            public const string MUST_BE_IN_RANGE = "{0} must be between {1} and {2}";
            public const string MUST_BE_NEGATIVE = "{0} must be negative";
            public const string MUST_BE_POSITIVE = "{0} must be positive";
            public const string MUST_NOT_BE_NEGATIVE = "{0} must not be negative";

            public const string NO_COMPARISON = "{0} could not be compared";
        }

        public static class PropertyTag
        {
            public const string CONFLICT = "conflict";
        }

        public static class MessageTag
        {
            public const string EXPECTED = "Expected";
            public const string UNEXPECTED = "Unexpected";
            public const string EMPTY = "No details";
        }
    }
}
#pragma warning restore CS1591 //Missing XML comment for publicly visible type or member