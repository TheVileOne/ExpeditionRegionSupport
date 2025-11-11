using LogUtils.Helpers.FileHandling;
using LogUtils.Properties;

namespace LogUtils.Enums
{
    /// <summary>
    /// A type of <see cref="LogID"/> representing a log file group.
    /// </summary>
    /// <remarks>
    /// Log group properties may be accessed, and changed through the <see cref="Properties"/> field.
    /// </remarks>
    public partial class LogGroupID : LogID
    {
        /// <summary>
        /// This prefix differentiates log group entries from log file entries
        /// </summary>
        internal const string ID_PREFIX = $"{UtilityConsts.PropertyTag.LOG_GROUP}:";

        /// <summary>
        /// Creates a new <see cref="LogGroupID"/> instance.
        /// </summary>
        /// <inheritdoc cref="LogID(string, LogAccess, bool)"/>
        /// <param name="value">The value that identifies the <see cref="LogGroupID"/> instance</param>
        /// <param name="register"/>
        public LogGroupID(string value, bool register = false) : base(getProperties(value), register)
        {
        }

        internal LogGroupID(LogProperties properties, bool register) : base(properties, register)
        {
        }

        /// <inheritdoc/>
        public override bool Equals(LogID idOther, bool doPathCheck)
        {
            if (base.Equals(idOther, doPathCheck))
                return true;

            if (!doPathCheck) //Base check would have handled this case
                return false;

            //Log group comparisons require a direct field comparison to detect a path match
            return Equals(idOther) && PathUtils.PathsAreEqual(Properties.FolderPath, idOther.Properties.FolderPath);
        }

        private static LogProperties getProperties(string value)
        {
            value = CreateIDValue(value, LogIDType.Group); //Expecting an unformatted value here

            //Inherit properties from an existing group ID if one exists, or create new properties
            LogID found = LogID.Find(value, CompareOptions.ID, includeGroupIDs: true);

            if (found != null)
                return found.Properties;

            return new LogGroupProperties(value);
        }
    }
}
