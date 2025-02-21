namespace LogUtils
{
    internal static class UtilitySetup
    {
        public static InitializationStep CurrentStep;

        public enum InitializationStep
        {
            NOT_STARTED = 0,
            INITALIZE_CORE_LOGGER = 1,
            START_SCHEDULER = 2,
            ESTABLISH_SETUP_PERIOD = 3,
            INITIALIZE_COMPONENTS = 4,
            INITIALIZE_ENUMS = 5,
            PARSE_FILTER_RULES = 6,
            ADAPT_LOGGING_SYSTEM = 7,
            POST_LOGID_PROCESSING = 8,
            APPLY_HOOKS = 9,
            COMPLETE = 10
        }
    }
}
