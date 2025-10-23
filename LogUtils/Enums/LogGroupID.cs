using LogUtils.Properties;
using System.Collections.Generic;
using System.Linq;

namespace LogUtils.Enums
{
    /// <summary>
    /// Represents an identifier for related <see cref="LogID"/> instances
    /// </summary>
    public class LogGroupID : LogID
    {
        /// <summary>
        /// This prefix differentiates log group entries from log file entries
        /// </summary>
        internal const string ID_PREFIX = $"{UtilityConsts.PropertyTag.LOG_GROUP}:";

        public LogGroupID(string value, bool register = false) : base(getProperties(value), register)
        {
        }

        private static LogProperties getProperties(string value)
        {
            value = createIDValue(value); //Expecting an unformatted value here

            IEnumerable<LogID> logGroups = FindByTag(UtilityConsts.PropertyTag.LOG_GROUP);

            //Inherit properties from an existing group ID if one exists, or create new properties
            LogID found = logGroups.FindAll(value, CompareOptions.ID)
                                   .FirstOrDefault();

            if (found != null)
                return found.Properties;

            return new LogProperties(value, "logutils-group", null)
            {
                Tags = [UtilityConsts.PropertyTag.LOG_GROUP]
            };
        }

        private static string createIDValue(string valueBase) => ID_PREFIX + valueBase;
    }
}
