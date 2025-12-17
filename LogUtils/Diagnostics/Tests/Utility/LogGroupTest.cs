using LogUtils.Enums;
using LogUtils.Helpers;
using LogUtils.Helpers.FileHandling;
using LogUtils.Properties;
using System;
using System.Collections.Generic;
using System.IO;

namespace LogUtils.Diagnostics.Tests.Utility
{
    internal static class LogGroupTest
    {
        private const string FOLDER_NAME = "LogUtilsTestGroup";

        private static LogGroupID testGroup;
        private static readonly List<Logger> memberLoggers = new List<Logger>();
        private static readonly bool storeMemberLoggers = true;

        internal static void InitializeGroup()
        {
            //Register log group to get properties for it, and to maximize LogUtils support for your group.
            //Registration is not necessary if you plan to fully manage your log files independently of LogUtils, and you do not wish files being moved around.
            //testGroup = LogGroupID.Factory.CreateID(UtilityConsts.UTILITY_NAME, register: true);
            testGroup = LogGroupID.Factory.FromPath(FOLDER_NAME, register: true);
            testGroup.Properties.FolderPermissions = FolderPermissions.Move;

            //Add members to group
            LogAccess defaultAccess = LogAccess.FullAccess;
            _ = new LogID(testGroup, formatLogName(SlugcatStats.Name.White), defaultAccess);
            _ = new LogID(testGroup, formatLogName(SlugcatStats.Name.Yellow), defaultAccess);
            _ = new LogID(testGroup, formatLogName(SlugcatStats.Name.Red), defaultAccess);

            //Put this member in a custom folder
            _ = new LogID(testGroup, Path.Combine("custom", formatLogName(new SlugcatStats.Name("TestSlugcat"))), defaultAccess);

            using (Logger logger = new Logger(LogID.Unity))
            {
                logger.Log("Initialization of test group complete");

                var groupMembers = testGroup.Properties.Members;
                logger.Log("Member Count: " + groupMembers.Count);

                foreach (LogID member in groupMembers)
                {
                    logger.Log("Member: " + member.Value);

                    Logger memberLogger = new Logger(member);
                    if (storeMemberLoggers)
                    {
                        memberLoggers.Add(memberLogger);
                        memberLogger.Log("Hello World");
                    }
                    else
                    {
                        using (memberLogger)
                        {
                            memberLogger.Log("Hello World");
                        }
                    }
                }
            }
        }

        internal static void CreateGroupFolder()
        {
            Directory.CreateDirectory(testGroup.Properties.CurrentFolderPath);
            Directory.CreateDirectory(Path.Combine(testGroup.Properties.CurrentFolderPath, "custom"));
        }

        internal static void CloseGroup()
        {
            testGroup.Unregister();

            string groupPath = testGroup.Properties.CurrentFolderPath;

            //Good code practice
            //RecycleBin.MoveToRecycleBin(groupPath);

            //Bad code practice
            //But since this is a test folder - it is okay to delete 
            DirectoryUtils.DeletePermanently(groupPath, deleteOnlyIfEmpty: false);
        }

        internal static void MoveGroupFolder()
        {
            //The group folder is expected to have this path if the move is successful 
            string newGroupPath = LogProperties.GetContainingPath("MyLogFiles");

            try
            {
                //This method can throw exceptions
                LogGroup.MoveFolder(testGroup, newGroupPath);
            }
            catch (Exception ex)
            {
                UtilityLogger.LogError(ex);
            }
        }

        private static string formatLogName(SlugcatStats.Name slugcatName)
        {
            return UtilityConsts.UTILITY_NAME + "-" + slugcatName;
        }
    }
}
