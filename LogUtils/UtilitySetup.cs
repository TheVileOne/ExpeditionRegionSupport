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
            INITIALIZE_PATCHER = 6,
            INITIALIZE_ENUMS = 7,
            PARSE_FILTER_RULES = 8,
            ADAPT_LOGGING_SYSTEM = 9,
            POST_LOGID_PROCESSING = 10,
            APPLY_HOOKS = 11,
            COMPLETE = 12
        }

        public enum Build
        {
            RELEASE = 0,
            DEVELOPMENT = 1,
        }
    }
}
