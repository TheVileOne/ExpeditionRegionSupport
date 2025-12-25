using LogUtils.Diagnostics;
using LogUtils.Enums;
using LogUtils.Helpers;
using LogUtils.Helpers.FileHandling;
using LogUtils.Threading;
using System;
using System.IO;
using System.Linq;
using static LogUtils.Diagnostics.ExceptionHandler;

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
        /// The behavior when attempting to move a group without members
        /// </summary>
        public bool AllowEmptyFolders;

        /// <summary>
        /// Optional conditions to check before a file is moved
        /// </summary>
        public MoveCondition Conditions;

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
        /// Move a log group to a target path
        /// </summary>
        public void Move(LogGroupID target, MoveBehavior moveBehavior)
        {
            try
            {
                if (PathUtils.IsEmpty(TargetPath))
                    throw new InvalidOperationException("Target path isn't specified");

                if (!target.Properties.IsFolderGroup)
                    throw new InvalidOperationException("Group does not support folder operations");

                LogID[] moveTargets = getFilesToMove(target);

                if (moveTargets.Length == 0)
                    return;

                UtilityLogger.Log($"Attempting to move {moveTargets.Length} log file(s)");

                //Prepare the folder - the destination must exist before we can move files there
                prepareDestinationFolder(moveTargets);

                ThreadSafeWorker worker = new ThreadSafeWorker(moveTargets.GetLocks());

                worker.DoWork(() =>
                {
                    bool allFilesMoved = true;
                    foreach (LogID target in moveTargets)
                    {
                        FileStatus moveResult = LogFile.Move(target, TargetPath);

                        if (moveResult != FileStatus.MoveComplete && moveResult != FileStatus.NoActionRequired)
                            allFilesMoved = false;
                    }

                    if (!allFilesMoved)
                    {
                        UtilityLogger.LogWarning("Unable to move all group files");
                    }
                    else
                    {
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

        private LogID[] getFilesToMove(LogGroupID target)
        {
            if (Conditions == null)
                return target.Properties.Members.ToArray();

            return target.Properties.Members.Where(new Func<LogID, bool>(Conditions)).ToArray();
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

        protected override ExceptionHandler CreateExceptionHandler()
        {
            ExceptionHandler handler = base.CreateExceptionHandler();

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
