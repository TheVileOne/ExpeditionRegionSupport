using LogUtils.Enums;
using LogUtils.Helpers;
using LogUtils.Helpers.FileHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using static LogUtils.UtilityConsts;

namespace LogUtils.Properties
{
    public class LogGroupProperties : LogProperties
    {
        /// <summary>
        /// A set of flags indicating activities that are safe, and permissible to happen to a defined group folder
        /// </summary>
        public FolderPermissions FolderPermissions = FolderPermissions.None;

        /// <summary>
        /// Indicates whether log group is associated with a folder path
        /// </summary>
        public bool IsFolderGroup => !PathUtils.IsEmpty(CurrentFolderPath);

        /// <inheritdoc/>
        public override bool IsMetadataOptional => true;

        /// <summary>
        /// The last known folder path representing this log group
        /// </summary>
        /// <value>Default value is an empty string when not storing a path</value>
        public string LastKnownFolderPath { get; private set; }

        /// <summary>
        /// All members associated with this log group
        /// </summary>
        public readonly List<LogID> Members = new List<LogID>();

        /// <inheritdoc/>
        protected override CompareOptions CompareMask => CompareOptions.ID;

        /// <inheritdoc/>
        protected override LogID CreateID() => new LogGroupID(this, false);

        /// <inheritdoc/>
        protected override int CreateIDHash() => CreateIDHash(GetRawID(), string.Empty);

        public LogGroupProperties(string propertyID) : base(propertyID, metadata: null)
        {
            AddTag(PropertyTag.LOG_GROUP);
            StartupRoutineRequired = false;
        }

        internal LogGroupProperties(string propertyID, LogPropertyMetadata metadata) : base(propertyID, metadata)
        {
            AddTag(PropertyTag.LOG_GROUP);
            StartupRoutineRequired = false;
        }

        /// <inheritdoc/>
        public override void ChangeFilename(string newFilename)
        {
            UtilityLogger.LogWarning($"This log group does not support this operation '{nameof(ChangeFilename)}'");
        }

        /// <inheritdoc/>
        public override void ChangePath(string newPath)
        {
            if (Members.Count > 0 && !IsFolderGroup)
            {
                UtilityLogger.LogWarning("Group path may not be assigned after members are set");
                return;
            }
            ChangePath(GetContainingPath(newPath), true);
        }

        internal void ChangePath(string newPath, bool applyToMembers)
        {
            if (applyToMembers && Members.Count > 0)
                LogGroup.ChangePath(GetFolderMembers(), CurrentFolderPath, newPath);
            CurrentFolderPath = newPath;
        }

        internal void SetInitialPath(string path)
        {
            OriginalFolderPath = GetContainingPath(path);
            CurrentFolderPath = FolderPath = OriginalFolderPath;
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

        /// <summary>
        /// Searches for all group members that are physically located inside the group folder, or otherwise target it
        /// </summary>
        /// <remarks>It is possible for a group member to target a different folder path. The easiest way to encounter this behavior is when a log file is already defined at the time of assignment.</remarks>
        public IEnumerable<LogID> GetFolderMembers()
        {
            if (!IsFolderGroup)
                throw new InvalidOperationException("Group does not support folder operations");

            return Members.Where(member => PathUtils.ContainsOtherPath(member.Properties.CurrentFolderPath, CurrentFolderPath));
        }

        /// <summary>
        /// Verify that group has been given the provided folder permissions
        /// </summary>
        public bool VerifyPermissions(FolderPermissions permissions)
        {
            return permissions == FolderPermissions.None || (FolderPermissions & permissions) == permissions;
        }
    }
}
