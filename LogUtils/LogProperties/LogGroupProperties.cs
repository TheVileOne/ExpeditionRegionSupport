using LogUtils.Enums;
using LogUtils.Helpers.FileHandling;

namespace LogUtils.Properties
{
    public class LogGroupProperties : LogProperties
    {
        /// <inheritdoc/>
        public override bool IsMetadataOptional => true;

        /// <summary>
        /// The last known folder path representing this log group
        /// </summary>
        /// <value>Default value is an empty string when not storing a path</value>
        public string LastKnownFolderPath { get; private set; }

        /// <inheritdoc/>
        protected override CompareOptions CompareMask => CompareOptions.ID;

        /// <inheritdoc/>
        protected override LogID CreateID() => new LogGroupID(this, false);

        public LogGroupProperties(string propertyID) : base(propertyID, string.Empty, string.Empty)
        {
            AddTag(UtilityConsts.PropertyTag.LOG_GROUP);
        }

        internal LogGroupProperties(string propertyID, LogPropertyStringDictionary optionalFields) : base(propertyID, optionalFields[UtilityConsts.DataFields.FILENAME] ?? string.Empty, optionalFields[UtilityConsts.DataFields.PATH] ?? string.Empty)
        {
            AddTag(UtilityConsts.PropertyTag.LOG_GROUP);
        }

        /// <inheritdoc/>
        public override void ChangeFilename(string newFilename)
        {
            UtilityLogger.LogWarning($"This log group does not support this operation '{nameof(ChangeFilename)}'");
        }

        /// <inheritdoc/>
        public override void ChangePath(string newPath)
        {
            UtilityLogger.LogWarning($"This log group does not support this operation '{nameof(ChangePath)}'");
        }

        internal override string GetLastKnownPath()
        {
            return LastKnownFolderPath;
        }

        internal override void SetLastKnownPath(string path = null)
        {
            base.SetLastKnownPath(path);
            if (!PathUtils.IsEmpty(LastKnownFilePath)) //When this field is not empty, we know it contains valid directory information
            {
                LastKnownFolderPath = PathUtils.PathWithoutFilename(LastKnownFilePath);
                return;
            }

            //An unset base value may indicate that we have no directory information to store, but it is equally likely that there is directory information, but no filename information.
            //This class is designed for log groups after all - there is no expectation for there to be filename information stored. What we CAN conclude here is that there can not be
            //filename information as part of the path.
            if (PathUtils.IsEmpty(path))
                path = CurrentFolderPath; //The last known path can be extracted from the current path

            LastKnownFolderPath = path;
        }
    }
}
