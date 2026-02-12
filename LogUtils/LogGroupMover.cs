using LogUtils.Diagnostics;
using LogUtils.Enums;
using LogUtils.Enums.FileSystem;
using LogUtils.Helpers;
using LogUtils.Helpers.FileHandling;
using LogUtils.Properties;
using LogUtils.Threading;
using System;
using System.IO;
using System.Linq;

namespace LogUtils
{
    public class LogGroupMover
    {
        /// <summary>
        /// The path that files will be moved to
        /// </summary>
        public string TargetPath;

        /// <summary>
        /// The behavior that results from being unable to complete a move operation
        /// </summary>
        public FailProtocol FailProtocol = FailProtocol.LogAndIgnore;

        /// <summary>
        /// The behavior that results from encountering a path that doesn't exist
        /// </summary>
        public FolderCreationProtocol FolderCreationProtocol = FolderCreationProtocol.FailToCreate;

        /// <summary>
        /// When false the mover will avoid creating an empty folder when given no files to move 
        /// </summary>
        public bool AllowEmptyFolders;

        /// <summary>
        /// The structure of the folder hierarchy of a folder group will be kept
        /// </summary>
        public bool PreserveFolderStructure = true;

        /// <summary>
        /// Controls whether or not the move operation applies to group files targetting a path other than the group folder path
        /// </summary>
        public bool IgnoreOutOfFolderFiles = true;

        private MoveCondition _condition;
        /// <summary>
        /// Optional conditions to check before a file is moved
        /// </summary>
        public MoveCondition Conditions
        {
            get => _condition;
            set
            {
                if (value == null)
                {
                    _condition = null;
                    return;
                }
                _condition += value;
            }
        }

        /// <summary>
        /// Create a object for moving groups of log files
        /// </summary>
        public LogGroupMover()
        {
        }

        /// <summary>
        /// Create a object for moving groups of log files
        /// </summary>
        /// <param name="targetPath">The path that files will be moved to</param>
        public LogGroupMover(string targetPath)
        {
            TargetPath = targetPath;
        }

        /// <summary>
        /// Moves log group files to a target path. If required, group path will be updated to reflect the new path.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="target"/> is null</exception>
        /// <exception cref="InvalidOperationException">Target path isn't set, or is invalid</exception>
        /// <exception cref="DirectoryNotFoundException">Directory was unable to be created</exception>
        /// <exception cref="IOException">General IO exceptions (probably directory not found)</exception>
        /// <exception cref="UnauthorizedAccessException">
        /// File is in use within the group folder that LogUtils cannot move
        /// - OR - LogUtils does not have permission to perform this operation
        /// </exception>
        public void Move(LogGroupID target)
        {
            Move(target, target.Properties.IsFolderGroup ? MoveBehavior.FilesAndGroup : MoveBehavior.FilesOnly);
        }

        /// <summary>
        /// Moves log group files to a target path. If specified, group path will be updated to reflect the new path.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="target"/> is null</exception>
        /// <exception cref="InvalidOperationException">Target path isn't set, or is invalid - OR - group does not support specified move operation</exception>
        /// <exception cref="DirectoryNotFoundException">Directory was unable to be created</exception>
        /// <exception cref="IOException">General IO exceptions (probably directory not found)</exception>
        /// <exception cref="UnauthorizedAccessException">
        /// File is in use within the group folder that LogUtils cannot move
        /// - OR - LogUtils does not have permission to perform this operation
        /// </exception>
        public void Move(LogGroupID target, MoveBehavior moveBehavior)
        {
            try
            {
                if (target == null)
                    throw new ArgumentNullException(nameof(target));

                if (PathUtils.IsEmpty(TargetPath))
                    throw new InvalidOperationException("Target path isn't specified");

                if (!target.Properties.IsFolderGroup)
                {
                    if (moveBehavior == MoveBehavior.FilesAndGroup)
                        throw new InvalidOperationException("Group does not support folder operations");
                }

                LogID[] moveTargets = getFilesToMove(target);

                //Prepare the folder - the destination must exist before we can move files there
                prepareDestinationFolder(moveTargets);

                ThreadSafeWorker worker = new ThreadSafeWorker(moveTargets.GetLocks());

                worker.DoWork(() =>
                {
                    int filesMoved = 0;
                    try
                    {
                        if (moveTargets.Length == 0)
                            return;

                        UtilityLogger.Log($"Attempting to move {moveTargets.Length} log file(s)");

                        LogGroupID groupTarget = target;
                        foreach (LogID target in moveTargets)
                        {
                            string newPath = getDestinationPath(target, groupTarget);
                            FileStatus moveResult = LogFile.Move(target, newPath);

                            if (moveResult != FileStatus.MoveComplete && moveResult != FileStatus.NoActionRequired)
                            {
                                UtilityLogger.LogWarning("Move operation failed");
                                continue;
                            }
                            filesMoved++;
                        }
                    }
                    finally
                    {
                        if (moveBehavior == MoveBehavior.FilesAndGroup)
                            target.Properties.ChangePath(TargetPath, applyToMembers: false);

                        target.Properties.SetLastKnownPath(TargetPath); //Required for LogUtils to properly detect last known file locations

                        if (filesMoved == moveTargets.Length)
                            UtilityLogger.Log("All files moved successfully");
                    }
                });
            }
            catch (Exception ex)
            {
                ExceptionHandler handler = new LogGroupMoverExceptionHandler()
                {
                    Protocol = FailProtocol
                };
                handler.OnError(ex);
            }
        }

        /// <summary>
        /// Moves a log group folder, and its contents to a target path
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="target"/> is null</exception>
        /// <exception cref="InvalidOperationException">Target path isn't set, or is invalid - OR - group does not support specified move operation</exception>
        /// <exception cref="DirectoryNotFoundException">Directory was unable to be created</exception>
        /// <exception cref="IOException">General IO exceptions (probably directory not found)</exception>
        /// <exception cref="PermissionDeniedException">
        /// LogUtils does not have permission from the log controller to move this folder
        /// - OR - It is unsafe to move this folder</exception>
        /// <exception cref="UnauthorizedAccessException">
        /// File is in use within the group folder that LogUtils cannot move
        /// - OR - LogUtils does not have permission to perform this operation
        /// </exception>
        public void MoveFolder(LogGroupID target)
        {
            //TODO: This code is not fully thread safe. It is vulnerable to new files being added to the group folder after entries are processed.
            //It is also vulnerable to the paths being retargeted before locks have been applied.
            try
            {
                UtilityLogger.Log("Attempting to move folder");
                if (target == null)
                    throw new ArgumentNullException(nameof(target));

                if (PathUtils.IsEmpty(TargetPath))
                    throw new InvalidOperationException("Target path isn't specified");

                if (PathUtils.IsFilePath(TargetPath))
                    throw new InvalidOperationException("Target path cannot contain a filename");

                if (!target.Properties.IsFolderGroup)
                    throw new ArgumentException("Group is not associated with a folder path");

                LogFile.MoveFolder(target.Properties.CurrentFolderPath, TargetPath);
            }
            catch (Exception ex)
            {
                ExceptionHandler handler = new LogGroupMoverExceptionHandler()
                {
                    Protocol = FailProtocol
                };
                handler.OnError(ex);
            }
        }

        /// <summary>
        /// Applies filter conditions to group members
        /// </summary>
        private LogID[] getFilesToMove(LogGroupID target)
        {
            var members = target.Properties.Members;

            if (members.Count == 0)
                return [];

            //There is no such thing as external members for non-folder groups
            bool ignoreExternalMembers = target.Properties.IsFolderGroup && IgnoreOutOfFolderFiles;

            LogID[] folderMembers = null;
            if (ignoreExternalMembers) //Avoids unnecessary execution
                folderMembers = target.Properties.GetFolderMembers().ToArray();

            return members.Where(logID =>
            {
                if (ignoreExternalMembers && !folderMembers.Contains(logID))
                    return false;

                return Conditions?.Invoke(logID) == true;
            }).ToArray();
        }

        private string getDestinationPath(LogID target, LogGroupID groupTarget)
        {
            if (!PreserveFolderStructure || !groupTarget.Properties.IsFolderGroup)
                return TargetPath;

            string currentBasePath = groupTarget.Properties.CurrentFolderPath;
            return LogProperties.GetNewBasePath(target, currentBasePath, TargetPath);
        }

        private void prepareDestinationFolder(LogID[] moveTargets)
        {
            if (Directory.Exists(TargetPath))
                return;

            FolderCreationProtocol protocol = FolderCreationProtocol;

            if (!AllowEmptyFolders && moveTargets.Length == 0) //Nothing to move, nothing to create
            {
                if (protocol == FolderCreationProtocol.EnsurePathExists)
                    throw new DirectoryNotFoundException("Target path could not be created. Folder would be empty.");
                return;
            }

            //Not allowed to create new folders
            if (protocol == FolderCreationProtocol.FailToCreate)
                throw new DirectoryNotFoundException("Target path must exist before files can be moved");

            //Not allowed to create any new folders other than the parent directory
            if (protocol == FolderCreationProtocol.CreateFolder && !DirectoryUtils.ParentExists(TargetPath))
                throw new DirectoryNotFoundException("Target path must exist before files can be moved");

            Directory.CreateDirectory(TargetPath);
        }

        private LogFileMover createFileMover(string sourceLogPath, string destLogPath)
        {
            return new GroupFileMover(this, sourceLogPath, destLogPath);
        }
    }

    internal sealed class GroupFileMover : LogFileMover
    {
        internal readonly LogGroupMover Owner;

        public GroupFileMover(LogGroupMover owner, string sourceLogPath, string destLogPath) : base(sourceLogPath, destLogPath)
        {
            Owner = owner;
        }

        protected override FileExceptionHandler CreateExceptionHandler(ErrorContext context)
        {
            var handler = base.CreateExceptionHandler(context);

            FailProtocol protocol = Owner.FailProtocol;

            if (protocol != FailProtocol.Throw) //Throwing inside file move process is currently unsupported
                handler.Protocol = protocol;
            return handler;
        }
    }

    internal sealed class LogGroupMoverExceptionHandler : ExceptionHandler
    {
        protected override void LogError(Exception exception)
        {
            UtilityLogger.LogError("Unable to complete move operation", exception);
        }
    }

    public enum MoveBehavior
    {
        /// <summary>The current path of all files, and the group will be changed</summary>
        FilesAndGroup,
        /// <summary>The current path of all files will be changed; group metadata will be unaffected</summary>
        FilesOnly,
    }

    public enum FolderCreationProtocol
    {
        /// <summary>Fail procedure activates when some part of the path doesn't exist</summary>
        FailToCreate,
        /// <summary>Attempt to create missing directories when some part of the path doesn't exist</summary>
        EnsurePathExists,
        /// <summary>Creates the parent directory only. Fail procedure activates when more than one directory specified in the the path doesn't exist</summary>
        CreateFolder,
    }

    public delegate bool MoveCondition(LogID logFile);
}
