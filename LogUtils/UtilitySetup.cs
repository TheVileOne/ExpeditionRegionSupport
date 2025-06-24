namespace LogUtils
{
    internal static class UtilitySetup
    {
        public static InitializationStep CurrentStep;

        public enum InitializationStep
        {
            NOT_STARTED = 0,
            SETUP_ENVIRONMENT = 1,
            START_SCHEDULER = 2,
            ESTABLISH_MONITOR_CONNECTION = 3,
            ESTABLISH_SETUP_PERIOD = 4,
            INITIALIZE_COMPONENTS = 5,
            INITIALIZE_ENUMS = 6,
            PARSE_FILTER_RULES = 7,
            ADAPT_LOGGING_SYSTEM = 8,
            POST_LOGID_PROCESSING = 9,
            APPLY_HOOKS = 10,
            COMPLETE = 11
        }

        public enum Build
        {
            RELEASE = 0,
            DEVELOPMENT = 1,
        }
    }
}
