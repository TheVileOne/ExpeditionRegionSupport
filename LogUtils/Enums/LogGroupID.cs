using LogUtils.Helpers.Comparers;
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

        /// <inheritdoc/>
        public override string Tag
        {
            get
            {
                if (RegistrationStage == RegistrationStatus.Completed && !ReferenceEquals(ManagedReference, this))
                    return ManagedReference.Tag;

                return Value; //Unlike typical LogID types, groups do not use the path as an identifier. There can be multiple groups with the same specified path.
            }
        }

        /// <inheritdoc cref="LogID.Properties"/>
        public new LogGroupProperties Properties
        {
            get => (LogGroupProperties)base.Properties;
            protected set => base.Properties = value;
        }

        /// <inheritdoc cref="LogGroupID(string, string, bool)"/>
        public LogGroupID(string value, bool register = false) : base(getProperties(value), register)
        {
        }

        /// <summary>
        /// Creates a new <see cref="LogGroupID"/> instance.
        /// </summary>
        /// <inheritdoc cref="LogID(string, LogAccess, bool)"/>
        /// <param name="value">The value that identifies the <see cref="LogGroupID"/> instance</param>
        /// <param name="path">An optional path that all group members will have in common</param>
        /// <param name="register"></param>
        public LogGroupID(string value, string path, bool register = false) : base(getProperties(value), register)
        {
            Properties.FolderPath =
            Properties.OriginalFolderPath = LogProperties.GetContainingPath(path);
        }

        internal LogGroupID(LogProperties properties, bool register) : base(properties, register)
        {
        }

        internal void Assign(LogID target)
        {
            target.Properties.Group = this;
            Properties.Members.Add(target);
        }

        /// <inheritdoc/>
        public override bool CheckTag(string tag)
        {
            //Adding a file extension is required by the helper
            if (PathUtils.IsFilePath(tag + ".txt")) //LogGroupIDs do not store path information in the tags - this should never be a match
                return false;

            return ComparerUtils.StringComparerIgnoreCase.Equals(Tag, tag);
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
