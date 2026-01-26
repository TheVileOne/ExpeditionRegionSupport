using LogUtils.Enums;
using LogUtils.Enums.FileSystem;
using LogUtils.Helpers;
using LogUtils.Helpers.FileHandling;
using LogUtils.Threading;
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

        public bool IsAnonymous { get; internal set; }

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

        /// <summary>
        /// Creates a new <see cref="LogGroupProperties"/> instance
        /// </summary>
        public LogGroupProperties(string propertyID) : base(propertyID, metadata: null)
        {
            AddTag(PropertyTag.LOG_GROUP);
        }

        internal LogGroupProperties(string propertyID, LogPropertyMetadata metadata) : base(propertyID, metadata)
        {
            AddTag(PropertyTag.LOG_GROUP);
        }

        internal override void OnStartup()
        {
            if (!StartupRoutineRequired)
            {
                base.OnStartup();
                return;
            }
            StartupRoutineRequired = false;

            if (LogsFolderAware)
                LogsFolder.AddToFolder(this);
        }

        /// <inheritdoc/>
        public override void ChangeFilename(string newFilename)
        {
            UtilityLogger.LogWarning($"This log group does not support this operation '{nameof(ChangeFilename)}'");
        }

        /// <inheritdoc/>
        public override void ChangePath(string newPath)
        {
            UtilityLogger.Log("Assigning new folder to group");
            if (!IsFolderGroup)
            {
                if (Members.Count > 0)
                {
                    UtilityLogger.LogWarning("Group path may not be assigned after members are set");
                    return;
                }
                UtilityLogger.Log("First time assignment");
            }
            ChangePath(GetContainingPath(newPath), applyToMembers: false);
        }

        internal void ChangePath(string newPath, bool applyToMembers)
        {
            if (applyToMembers && Members.Count > 0)
                LogGroup.ChangePath(GetFolderMembers(), CurrentFolderPath, newPath);
            CurrentFolderPath = newPath;
            UpdateLastKnownPath();
        }

        internal void SetInitialPath(string path)
        {
            if (!IsNewInstance) return;

            path = !PathUtils.IsEmpty(path) //For groups, there is little value to having a default path when we can define the group as not having a folder
                   ? ValidatePath(path)
                   : string.Empty;

            OriginalFolderPath = path;
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

        internal override void UpdateLastKnownPath()
        {
            SetLastKnownPath(CurrentFolderPath);
        }

        /// <summary>
        /// Searches for all group members that are physically located inside the group folder, or otherwise target it
        /// </summary>
        /// <remarks>It is possible for a group member to target a different folder path. The easiest way to encounter this behavior is when a log file is already defined at the time of assignment.</remarks>
        /// <exception cref="InvalidOperationException">The log group is not associated with a folder</exception>
        public IEnumerable<LogID> GetFolderMembers()
        {
            if (!IsFolderGroup)
                throw new InvalidOperationException("Group does not support folder operations");

            return Members.Where(member => PathUtils.ContainsOtherPath(member.Properties.CurrentFolderPath, CurrentFolderPath));
        }

        /// <summary>
        /// Searches for all group members that do not share the same path as the group
        /// </summary>
        /// <remarks>It is possible for a group member to target a different folder path. The easiest way to encounter this behavior is when a log file is already defined at the time of assignment.</remarks>
        /// <exception cref="InvalidOperationException">The log group is not associated with a folder</exception>
        public IEnumerable<LogID> GetNonConformingMembers()
        {
            if (!IsFolderGroup)
                throw new InvalidOperationException("Group does not support folder operations");

            return Members.Where(member => !PathUtils.ContainsOtherPath(member.Properties.CurrentFolderPath, CurrentFolderPath));
        }

        /// <summary>
        /// Blocks until current thread has exclusive access to groups sharing the current group path, or a subdirectory of the group path (also includes current instance)
        /// </summary>
        /// <exception cref="InvalidOperationException">The log group is not associated with a folder</exception>
        public IScopedCollection<LogGroupProperties> DemandFolderAccess()
        {
            if (!IsFolderGroup)
                throw new InvalidOperationException("Group does not support folder operations");

            CombinationLock<LogGroupProperties> folderLock = new CombinationLock<LogGroupProperties>(AllGroupsSharingMyFolder());
            return folderLock.Acquire();
        }

        internal IEnumerable<LogGroupProperties> AllGroupsSharingMyFolder()
        {
            IEnumerable<LogGroupProperties> groupsSharingThisFolder = LogGroup.GroupsSharingThisPath(CurrentFolderPath).GetProperties();

            //TODO: This only applies to the current instance - it is possible for other unregistered groups to interfere with a critical operation
            //If unregistered groups get added to PropertiesManager, this line needs to be removed
            if (!ID.Registered)
                return groupsSharingThisFolder.Prepend(this);

            return groupsSharingThisFolder;
        }

        /// <summary>
        /// Verify that group has been given the provided folder permissions
        /// </summary>
        public bool VerifyPermissions(FolderPermissions permissions)
        {
            return permissions == FolderPermissions.None || (FolderPermissions & permissions) == permissions;
        }

        /// <summary>
        /// Verify that group has been given the provided folder permissions
        /// </summary>
        public bool VerifyPermissionsRecursive(FolderPermissions permissions, HashSet<LogGroupProperties> checkedEntries)
        {
            UtilityLogger.Log($"Processing permissions for log group [{ID}]");
            checkedEntries.Add(this);
            if (!VerifyPermissions(permissions))
                return false;

            IEnumerable<LogGroupProperties> groupsSharingThisFolder =
                        PropertyManager.GroupProperties
                        .WithFolder()
                        .HasPath(CurrentFolderPath);

            foreach (LogGroupProperties relatedGroup in groupsSharingThisFolder)
            {
                if (checkedEntries.Contains(relatedGroup)) //Entries should only be processed once
                    continue;

                //A group that is part of a subfolder will contain more characters
                bool isTopLevelGroup = CurrentFolderPath.Length != relatedGroup.CurrentFolderPath.Length;

                //When processing a group at the same level - no subfolders need to be checked
                if (isTopLevelGroup)
                {
                    UtilityLogger.Log($"Processing permissions for log group [{relatedGroup.ID}]");
                    checkedEntries.Add(relatedGroup);
                    if (!relatedGroup.VerifyPermissions(permissions))
                        return false;
                    continue;
                }

                UtilityLogger.Log($"Processing permissions for log subgroup [{relatedGroup.ID}]");
                if (!relatedGroup.VerifyPermissionsRecursive(permissions, checkedEntries))
                    return false;
            }
            return true;
        }
    }
}
