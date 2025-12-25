using LogUtils.Diagnostics;
using LogUtils.Enums;
using LogUtils.Helpers;
using LogUtils.Helpers.Comparers;
using LogUtils.Helpers.FileHandling;
using LogUtils.Properties;
using LogUtils.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PermissionSet = (bool CanMove, bool IsAware, bool IsEligible);

namespace LogUtils
{
    public static partial class LogsFolder
    {
        /// <summary>
        /// Manages subfolders and their associated log groups within the log directory
        /// </summary>
        internal static FolderProcessor Processor = new FolderProcessor();

        /// <summary>
        /// Attempts to move eligible log groups to Logs folder
        /// </summary>
        internal static void AddGroupsToFolder()
        {
            if (!Exists) return;

            AddGroupsToFolder(Processor.GroupTopLevelEntries());
        }

        internal static void AddGroupsToFolder(IEnumerable<IEnumerable<LogGroupProperties>> entryGroups)
        {
            LogGroupProperties[] untargetedGroups = null;

            //When the Logs folder is available, favor that path over the original path to the log file
            foreach (IEnumerable<LogGroupProperties> entryGroup in entryGroups)
            {
                //Don't handle untargeted groups until after all folder groups are handled first
                if (isUntargetedGroup(entryGroup))
                {
                    untargetedGroups = entryGroup.ToArray();
                    continue;
                }

                ThreadSafeWorker worker = new ThreadSafeWorker(entryGroup.GetLocks());

                worker.DoWork(() =>
                {
                    AddGroupsToFolder(entryGroup.ToArray());
                });
            }

            //Figure out which log files need to be handled from the untargeted groups here
            if (untargetedGroups != null)
            {
                UtilityLogger.Log("Processing untargeted log groups"); //This should only log once
                UtilityLogger.Log($"Detected {untargetedGroups.Length} group(s)");

                foreach (LogGroupProperties group in untargetedGroups)
                {
                    

                }
            }

            bool isUntargetedGroup(IEnumerable<LogGroupProperties> entryGroup)
            {
                if (untargetedGroups != null) //Only one of the groups can be the untargeted group
                    return false;
                return !entryGroup.First().IsFolderGroup;
            }
        }

        internal static void AddGroupsToFolder(LogGroupProperties[] entries)
        {
            LogGroupProperties firstEntry = entries.First();

            if (!firstEntry.IsFolderGroup)
            {
                UtilityLogger.Log("Checking eligibility requirements");
                processEntries(moveGroupFilesOnly);
                return;
            }

            //The first pass checks for eligibility requirements for moving the entire folder, and all associated log groups and log files associated with it
            if (tryAddFolderGroupFirstPass(firstEntry))
            {
                UtilityLogger.Log("Log group moved successfully");
                return;
            }

            processEntries(moveGroupIntoSubFolder);

            //Since we could not move the entire folder - we need to check each subgroup
            AddGroupsToFolder(Processor.GroupTopLevelEntries(firstEntry));

            void processEntries(Action<LogGroupProperties> moveAction)
            {
                Dictionary<LogID, Exception> errors = null;
                foreach (LogGroupProperties target in entries)
                {
                    UtilityLogger.Log($"Processing {target.ID} with {target.Members.Count} entries");

                    //The first obtained result involves potential embedded subgroups, and other independent log files. This check is exclusive to this
                    //group's members in particular. An unsuccessful result indicates we should not move any members in this log group.
                    EligibilityResult result = checkEligibilityRequirementsThisEntry(target);

                    if (result == EligibilityResult.InsufficientPermissions)
                    {
                        UtilityLogger.Log($"{target.ID} is currently ineligible to be moved to Logs folder");
                        continue;
                    }

                    try
                    {
                        moveAction.Invoke(target);
                    }
                    catch (IOException ex)
                    {
                        if (errors != null)
                            errors = new Dictionary<LogID, Exception>();
                        errors[target.ID] = ex;
                    }
                }

                if (errors != null)
                {
                    UtilityLogger.LogWarning("Groups unable to be moved");

                    foreach (var entry in errors)
                    {
                        UtilityLogger.Log("Group : " + entry.Key);
                        UtilityLogger.Log("Reason: " + entry.Value.Message);
                    }
                }
            }
        }

        /// <summary>
        /// This method is only responsible with moving the existing group folder to the Logs folder directory, or a subfolder within it
        /// </summary>
        private static bool tryAddFolderGroupFirstPass(LogGroupProperties target)
        {
            UtilityLogger.Log("Checking eligibility requirements");
            EligibilityResult result = checkEligibilityRequirements(target);

            if (result == EligibilityResult.Success)
            {
                UtilityLogger.Log("Attempting to move group folder to Logs folder");
                return tryMoveGroup(target);
            }
            return false;
        }

        private static bool tryMoveGroup(LogGroupProperties target)
        {
            try
            {
                LogGroup.MoveFolder((LogGroupID)target.ID, Path.Combine(CurrentPath, Path.GetFileName(target.CurrentFolderPath)));
                return true;
            }
            catch (Exception ex)
            {
                UtilityLogger.LogWarning("Failed to move folder");
                UtilityLogger.LogError(ex);
            }
            return false;
        }

        private static void moveGroupIntoSubFolder(LogGroupProperties target)
        {
            string newGroupPath = getDestinationPath(target);

            LogGroupMover groupMover = new LogGroupMover(newGroupPath)
            {
                FailProtocol = ExceptionHandler.FailProtocol.Throw,
                FolderCreationProtocol = FolderCreationProtocol.EnsurePathExists,
                Conditions = requiresMoveCheck
            };
            groupMover.Move((LogGroupID)target.ID, MoveBehavior.FilesOnly);
        }

        private static void moveGroupFilesOnly(LogGroupProperties target)
        {
            string newGroupPath = getDestinationPath(target);

            LogGroupMover groupMover = new LogGroupMover(newGroupPath)
            {
                FailProtocol = ExceptionHandler.FailProtocol.Throw,
                FolderCreationProtocol = target.IsFolderGroup ? FolderCreationProtocol.CreateFolder : FolderCreationProtocol.FailToCreate,
                Conditions = requiresMoveCheck
            };
            groupMover.Move((LogGroupID)target.ID, MoveBehavior.FilesOnly);
        }

        static bool requiresMoveCheck(LogID logID)
        {
            return !logID.Registered && !PathUtils.ContainsOtherPath(logID.Properties.CurrentFolderPath, CurrentPath);
        }

        private static string getDestinationPath(LogGroupProperties target)
        {
            if (!target.IsFolderGroup)
                return CurrentPath;

            string targetDirName = Path.GetFileName(target.CurrentFilePath);

            //Take the parent directory of the group, and make it the new destination inside Logs folder
            return Path.Combine(CurrentPath, targetDirName);
        }

        private static EligibilityResult checkEligibilityRequirementsThisEntry(LogGroupProperties target)
        {
            PermissionSet permissionSet =
            (
                CanMove: true,
                IsAware: true,
                IsEligible: true
            );

            Lock groupLock = target.GetLock();
            using (groupLock.Acquire())
            {
                permissionSet.CanMove = target.VerifyPermissions(FolderPermissions.Move);
                permissionSet.IsAware = target.LogsFolderAware;
                permissionSet.IsEligible = target.LogsFolderEligible;
            }

            bool hasAllPermissions = permissionSet.CanMove && permissionSet.IsAware && permissionSet.IsEligible;

            if (!hasAllPermissions)
                return EligibilityResult.InsufficientPermissions;
            return EligibilityResult.Success;
        }

        private static EligibilityResult checkEligibilityRequirements(LogGroupProperties target)
        {
            if (!target.IsFolderGroup)
                return checkEligibilityRequirementsThisEntry(target);

            PermissionSet permissionSet =
            (
                CanMove: true,
                IsAware: true,
                IsEligible: true
            );

            string targetPath = target.CurrentFolderPath;

            using (var scope = target.DemandFolderAccess())
            {
                if (!DirectoryUtils.IsSafeToMove(targetPath))
                    return EligibilityResult.PathIneligible;

                IReadOnlyCollection<LogGroupProperties> groupsSharingThisFolder = scope.Items;

                //Step 1: Check all shared groups for eligibility requirements
                checkEligibilityRequirements(groupsSharingThisFolder, ref permissionSet);

                bool canMoveEntireFolder = permissionSet.CanMove && permissionSet.IsAware && permissionSet.IsEligible;

                if (!canMoveEntireFolder)
                    return EligibilityResult.InsufficientPermissions;

                LogGroupProperties[] allGroups = LogProperties.PropertyManager.GroupProperties.ToArray();
                IEnumerable<LogGroupProperties> unsharedGroups = allGroups.Except(groupsSharingThisFolder);

                //Step 2: Check entries from other groups for eligibility requirements
                foreach (LogGroupProperties entry in unsharedGroups)
                {
                    if (entry.IsFolderGroup)
                        checkEligibilityRequirements(entry.GetNonConformingMembers(), targetPath, ref permissionSet);
                    else
                        checkEligibilityRequirements(entry.Members, targetPath, ref permissionSet);
                }

                canMoveEntireFolder = permissionSet.IsAware && permissionSet.IsEligible;

                if (!canMoveEntireFolder)
                    return EligibilityResult.InsufficientPermissions;

                //Step 3: Check for LogIDs not part of any group for eligibility requirements
                checkEligibilityRequirements(LogID.GetEntries(), targetPath, ref permissionSet);

                if (!canMoveEntireFolder)
                    return EligibilityResult.InsufficientPermissions;
            }
            return EligibilityResult.Success;
        }

        private static void checkEligibilityRequirements(IEnumerable<LogGroupProperties> uncheckedGroups, ref PermissionSet permissionSet)
        {
            foreach (LogGroupProperties entry in uncheckedGroups)
            {
                if (!entry.VerifyPermissions(FolderPermissions.Move))
                    permissionSet.CanMove = false;

                if (!entry.LogsFolderAware)
                    permissionSet.IsAware = false;

                if (!entry.LogsFolderEligible)
                    permissionSet.IsEligible = false;
            }
        }

        private static void checkEligibilityRequirements(IEnumerable<LogID> uncheckedMembers, string groupPath, ref PermissionSet permissionSet)
        {
            foreach (LogID member in uncheckedMembers)
            {
                string memberPath = member.Properties.CurrentFolderPath;

                //Check for rogue members from other groups
                if (PathUtils.ContainsOtherPath(memberPath, groupPath))
                {
                    if (!member.Properties.LogsFolderAware)
                        permissionSet.IsAware = false;

                    if (!member.Properties.LogsFolderEligible)
                        permissionSet.IsEligible = false;
                }
            }
        }

        internal class FolderProcessor
        {
            /// <summary>
            /// Groups entries that should be processed together (because they share the same path, or don't have a path)
            /// </summary>
            internal IEnumerable<IEnumerable<LogGroupProperties>> GroupTopLevelEntries()
            {
                List<LogGroupProperties> topLevelEntries = getTopLevelEntries();

                return topLevelEntries.GroupBy(p => p.CurrentFolderPath, ComparerUtils.PathComparer);
            }

            internal IEnumerable<IEnumerable<LogGroupProperties>> GroupTopLevelEntries(LogGroupProperties target)
            {
                List<LogGroupProperties> topLevelEntries = getTopLevelEntries(target);

                return topLevelEntries.GroupBy(p => p.CurrentFolderPath, ComparerUtils.PathComparer);
            }

            private List<LogGroupProperties> getTopLevelEntries()
            {
                return getTopLevelEntries(LogProperties.PropertyManager.GroupProperties);
            }

            private List<LogGroupProperties> getTopLevelEntries(LogGroupProperties target)
            {
                return getTopLevelEntries(target.AllGroupsSharingMyFolder());
            }

            private List<LogGroupProperties> getTopLevelEntries(IEnumerable<LogGroupProperties> entriesToCheck)
            {
                List<LogGroupProperties> topLevelEntries = new List<LogGroupProperties>();
                foreach (LogGroupProperties uncheckedEntry in entriesToCheck)
                {
                    if (!uncheckedEntry.IsFolderGroup)
                    {
                        //For lack of a better place for them, consider untargeted groups as toplevel
                        topLevelEntries.Add(uncheckedEntry);
                        continue;
                    }

                    if (isTopLevel(uncheckedEntry))
                    {
                        //Remove any entries that belong to the unchecked entry
                        topLevelEntries.RemoveAll(entry => entryBelongsToGroup(uncheckedEntry, entry));
                        topLevelEntries.Add(uncheckedEntry);
                    }
                }
                return topLevelEntries;

                bool isTopLevel(LogGroupProperties uncheckedEntry)
                {
                    return topLevelEntries.All(entry => !entryBelongsToGroup(entry, uncheckedEntry));
                }
            }

            internal bool entryBelongsToGroup(LogGroupProperties entry, LogGroupProperties entryOther)
            {
                if (!entry.IsFolderGroup || !entryOther.IsFolderGroup)
                    return false;

                int currentPathLength = entry.CurrentFolderPath.Length;
                int otherPathLength = entryOther.CurrentFolderPath.Length;

                if (currentPathLength <= otherPathLength) //Entry belongs to group when it belongs to a subpath
                    return false;
                return PathUtils.ContainsOtherPath(entry.CurrentFolderPath, entryOther.CurrentFolderPath);
            }
        }
    }
}
