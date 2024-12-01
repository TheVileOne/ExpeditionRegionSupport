namespace LogUtils
{
    internal static class UtilitySetup
    {
        public enum InitializationStep
        {
            NOT_STARTED,
            INITALIZE_CORE_LOGGER,
            START_SCHEDULER,
            ESTABLISH_SETUP_PERIOD,
            INITIALIZE_COMPONENTS,
            INITIALIZE_LOGIDS,
            PARSE_FILTER_RULES,
            ADAPT_LOGGING_SYSTEM,
            POST_LOGID_PROCESSING,
            APPLY_HOOKS,
            COMPLETE
        }
    }
}
