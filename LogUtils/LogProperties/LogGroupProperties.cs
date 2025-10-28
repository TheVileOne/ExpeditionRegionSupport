using LogUtils.Enums;
using System;

namespace LogUtils.Properties
{
    public class LogGroupProperties : LogProperties
    {
        /// <inheritdoc/>
        protected override LogID CreateID() => new LogGroupID(this, false);

        public LogGroupProperties(string propertyID) : base(propertyID, string.Empty, string.Empty)
        {
            AddTag(UtilityConsts.PropertyTag.LOG_GROUP);
        }

        internal override void InitializeMetadata(string filename, string path)
        {
            Filename = new LogFilename(filename);
            FolderPath = path;

            CurrentFilename = ReserveFilename = Filename;
            CurrentFolderPath = OriginalFolderPath = FolderPath;
            LastKnownFilePath = CurrentFilePath;
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
    }
}
