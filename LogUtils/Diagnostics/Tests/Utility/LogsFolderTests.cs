using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LogUtils.Diagnostics.Tests.Utility
{
    internal static class LogsFolderTests
    {
        internal static void TestFolderMove()
        {
            if (!LogsFolder.Exists)
            {
                UtilityLogger.Log("Cannot run test - folder doesn't exist");
            }

            string tempPath = Path.GetTempPath();

            StringBuilder builder = new StringBuilder();

            builder.AppendLine()
                   .AppendLine("Test - Moving entire folder");

            DirectoryInfo directory = new DirectoryInfo(LogsFolder.CurrentPath);
            DirectoryInfo tempDirectory = new DirectoryInfo(tempPath);

            try
            {
                //This is not expected to pass
                directory.MoveTo(Path.Combine(tempPath, Path.GetFileName(LogsFolder.CurrentPath)));
                directory.MoveTo(LogsFolder.CurrentPath);
                builder.AppendLine("Passed");
            }
            catch (Exception ex)
            {
                builder.AppendLine("Failed")
                       .Append("Exception: ").AppendLine(ex.Message);
            }

            //Create a directory for moving or copying files into
            DirectoryInfo copyDirectory = tempDirectory.CreateSubdirectory(Path.GetFileName(LogsFolder.CurrentPath));

            List<FileInfo> unableToMoveOrCopy = new List<FileInfo>();
            List<string> exceptionMessages = new List<string>();

            builder.AppendLine("Test - Moving contained files to temp location");
            testMoveFiles();

            FileInfo[] filesMovedOrCopied = copyDirectory.GetFiles();

            if (filesMovedOrCopied.Length > 0)
                builder.AppendLine("Files were not properly restored");

            builder.AppendLine("Test - Copying contained files to temp location");
            testCopyFiles();

            //Cleanup temp folder
            copyDirectory.Delete();
            UtilityLogger.Log(builder.ToString());

            void testMoveFiles()
            {
                foreach (FileInfo file in directory.GetFiles())
                {
                    try
                    {
                        file.MoveTo(Path.Combine(copyDirectory.FullName, file.Name));
                    }
                    catch (Exception ex)
                    {
                        unableToMoveOrCopy.Add(file);
                        exceptionMessages.Add(ex.Message);
                    }
                }

                filesMovedOrCopied = copyDirectory.GetFiles();
                appendResults();

                //Move the files back
                foreach (FileInfo file in filesMovedOrCopied)
                {
                    file.MoveTo(Path.Combine(directory.FullName, file.Name));
                }
            }

            void testCopyFiles()
            {
                foreach (FileInfo file in directory.GetFiles())
                {
                    try
                    {
                        file.CopyTo(Path.Combine(copyDirectory.FullName, file.Name));
                    }
                    catch (Exception ex)
                    {
                        unableToMoveOrCopy.Add(file);
                        exceptionMessages.Add(ex.Message);
                    }
                }

                filesMovedOrCopied = copyDirectory.GetFiles();
                appendResults();

                //Delete copied files
                foreach (FileInfo file in filesMovedOrCopied)
                {
                    file.Delete();
                }
            }

            void appendResults()
            {
                builder.AppendLine($"Moved or copied {filesMovedOrCopied.Length} files")
                       .AppendLine($"Unable to move or copy {unableToMoveOrCopy.Count} files");

                //Report the ones that could not be moved, or copied
                for (int i = 0; i < unableToMoveOrCopy.Count; i++)
                {
                    FileInfo file = unableToMoveOrCopy[i];

                    builder.Append("Filename:").AppendLine(file.Name)
                           .Append("Exception: ").AppendLine(exceptionMessages[i]);
                }
            }
        }

        /// <summary>
        /// Moves eligible files to the current log directory and moves them back to their original location 
        /// </summary>
        internal static void TestMoveAndRestore()
        {
            UtilityCore.Scheduler.Schedule(() =>
            {
                try
                {
                    //Alternate between having log files in the current log directory, and having them at their original location
                    if (!LogsFolder.IsManagingFiles)
                    {
                        UtilityLogger.DebugLog("Enabled");
                        LogsFolder.MoveFilesToFolder();
                    }
                    else
                    {
                        UtilityLogger.DebugLog("Disabled");
                        LogsFolder.RestoreFiles();
                    }
                }
                catch (Exception ex)
                {
                    UtilityLogger.DebugLog(ex);
                }
            }, frameInterval: 200);
        }
    }
}
