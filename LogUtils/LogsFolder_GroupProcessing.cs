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
    public partial class LogsFolder
    {
        internal static void ProcessGroup(LogGroupProperties group)
        {
            Processor.Process(group);
        }

        internal static void AddGroupsToFolder()
        {
            if (!Exists) return;

            LogGroupProperties[] untargetedGroups = null;

            ThreadSafeWorker worker = new ThreadSafeWorker(Processor.TopLevelEntries.GetLocks());

            worker.DoWork(() =>
            {
                //When the Logs folder is available, favor that path over the original path to the log file
                foreach (IEnumerable<LogGroupProperties> entryGroup in Processor.GroupTopLevelEntries())
                {
                    //Don't handle untargeted groups until after all folder groups are handled first
                    if (untargetedGroups == null && !entryGroup.First().IsFolderGroup)
                    {
                        untargetedGroups = entryGroup.ToArray();
                        continue;
                    }

                    AddGroupsToFolder(entryGroup);
                }

                //Figure out which log files need to be handled from the untargeted groups here
            });
        }

        internal static void AddGroupsToFolder(IEnumerable<LogGroupProperties> entries)
        {
            entries = entries.ToArray();

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

            void processEntries(Action<LogGroupProperties> moveAction)
            {
                Dictionary<LogID, Exception> errors = null;
                foreach (LogGroupProperties target in entries)
                {
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
                FolderCreationProtocol = FolderCreationProtocol.CreateFolder,
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
            private HashSet<LogGroupProperties> checkedEntries;
            internal List<LogGroupProperties> TopLevelEntries;

            /// <summary>
            /// Identify where this entry belongs in a group folder structure (i.e. is it a toplevel group, or nested within some other group) 
            /// </summary>
            internal void Process(LogGroupProperties entry)
            {
                if (!entry.IsFolderGroup)
                {
                    checkedEntries.Add(entry);
                    TopLevelEntries.Add(entry); //For lack of a better place for it, store it with the other toplevel groups
                    return;
                }

                //Replace toplevel entries contained by the current group path 
                checkedEntries.RemoveWhere(entryBelongsToGroup);

                foreach (var relatedEntry in entry.AllGroupsSharingMyFolder())
                {
                    if (!checkedEntries.Add(relatedEntry))
                        continue;

                    if (relatedEntry.CurrentFolderPath.Length > entry.CurrentFolderPath.Length) //Shared folder paths can only be equal, or greater in length
                    {
                        UtilityLogger.Log("Group entry contained within another group's folder");
                        continue;
                    }

                    //Entry here could be a new group path, or be an existing duplicate toplevel group 
                    TopLevelEntries.Add(entry);
                }

                bool entryBelongsToGroup(LogGroupProperties otherEntry)
                {
                    int currentPathLength = entry.CurrentFilePath.Length;
                    int otherPathLength = otherEntry.CurrentFolderPath.Length;

                    if (currentPathLength <= otherPathLength) //Entry belongs to group when it belongs to a subpath
                        return false;
                    return PathUtils.ContainsOtherPath(entry.CurrentFilePath, otherEntry.CurrentFilePath);
                }
            }

            /// <summary>
            /// Filters out all toplevel entries that qualify as sharing a filepath with another entry
            /// </summary>
            internal IEnumerable<LogGroupProperties> GetUniqueTopLevelEntries()
            {
                List<string> checkedPaths = new List<string>();
                foreach (var entry in TopLevelEntries)
                {
                    if (!entry.IsFolderGroup || checkedPaths.Exists(path => PathUtils.PathsAreEqual(path, entry.CurrentFolderPath)))
                        continue;

                    checkedPaths.Add(entry.CurrentFolderPath);
                    yield return entry;
                }
                yield break;
            }

            internal LogGroupProperties[] GetDuplicateTopLevelEntries(LogGroupProperties entry)
            {
                var duplicateEntries = TopLevelEntries.Where(otherEntry => PathUtils.PathsAreEqual(entry.CurrentFolderPath, otherEntry.CurrentFolderPath));
                return duplicateEntries.ToArray();
            }

            /// <summary>
            /// Groups entries that should be processed together (because they share the same path, or don't have a path)
            /// </summary>
            internal IEnumerable<IEnumerable<LogGroupProperties>> GroupTopLevelEntries()
            {
                return TopLevelEntries.GroupBy(p => p.CurrentFolderPath, ComparerUtils.PathComparer);
            }
        }
    }
}
