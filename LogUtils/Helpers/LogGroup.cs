using LogUtils.Diagnostics;
using LogUtils.Enums;
using LogUtils.Helpers.FileHandling;
using System.IO;

namespace LogUtils.Helpers
{
    public static class LogGroup
    {
        public static void DeleteFolder(LogGroupID group)
        {
        }

        public static void MoveFolder(LogGroupID group, string newPath)
        {
        }

        public static void MoveFiles(LogGroupID group, string newPath)
        {
        }

        /// <summary>
        /// Example showing how API can be used by a mod to move their group folder, or its contents around
        /// </summary>
        public static void MoveFolderExample()
        {
            LogGroupID myGroupID = null;
            bool hasTriedToMoveFolder = false;
        retry:
            try
            {
                //Define a new group path
                string folderName = Path.GetFileName(myGroupID.Properties.CurrentFolderPath);
                string newFolderPath = Path.Combine("new path", folderName);

                //Attempt to move entire folder, and if it fails, attempt to move only the files instead
                if (!hasTriedToMoveFolder)
                {
                    MoveFolder(myGroupID, newFolderPath);
                }
                else
                {
                    MoveFiles(myGroupID, newFolderPath);
                }
                //Confirm that we have a new path
                Assert.That(PathUtils.PathsAreEqual(newFolderPath, myGroupID.Properties.CurrentFolderPath));
            }
            catch (IOException)
            {
                if (!hasTriedToMoveFolder) //Ignore if this fails twice, but actual handling procesures may differ
                {
                    hasTriedToMoveFolder = true;
                    goto retry;
                }
            }
        }
    }
}
