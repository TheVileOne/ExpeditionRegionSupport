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

        internal LogGroupID(LogProperties properties, bool register) : base(properties, register)
        {
        }

        private static LogProperties getProperties(string value)
        {
            value = createIDValue(value); //Expecting an unformatted value here

            //Inherit properties from an existing group ID if one exists, or create new properties
            LogID found = LogID.Find(value, CompareOptions.ID, includeGroupIDs: true);

            if (found != null)
                return found.Properties;

            return new LogGroupProperties(value);
        }

        private static string createIDValue(string valueBase) => ID_PREFIX + valueBase;
    }
}
