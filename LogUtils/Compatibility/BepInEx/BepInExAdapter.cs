using BepInEx.Logging;
using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Formatting;
using LogUtils.Helpers.FileHandling;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using BepInExPath = LogUtils.Helpers.Paths.BepInEx;

namespace LogUtils.Compatibility.BepInEx
{
    /// <summary>
    /// Adapter service for converting the BepInEx logging system to the system that LogUtils operates
    /// </summary>
    internal static class BepInExAdapter
    {
        public static void Run()
        {
            BepInExInfo.BuildInfoCache();
            AdaptLoggingSystem();
        }

        /// <summary>
        /// Transitions the BepInEx logging system, and data over to the system operated by LogUtils
        /// </summary>
        internal static void AdaptLoggingSystem()
        {
            //These listener instances are incompatible with LogUtils and must be replaced
            ILogListener closeTarget = BepInExInfo.Find<DiskLogListener>();
            BepInExInfo.Close(closeTarget);

            closeTarget = BepInExInfo.Find<ConsoleLogListener>();
            BepInExInfo.Close(closeTarget);

            BepInExInfo.InitializeListener();

            //It is not possible to intercept logged messages to BepInEx made before Chainloader is initialized
            //This code determines what should happen to the existing BepInEx log file
            if (UtilityCore.IsControllingAssembly)
            {
                string bepInExLogPath = Path.Combine(BepInExPath.RootPath, UtilityConsts.LogNames.BepInEx + FileExt.LOG);

                if (!File.Exists(bepInExLogPath)) //Perhaps someone is using a newer, or modified BepInEx version
                {
                    UtilityLogger.LogWarning("Unable to locate BepInEx log file");
                    return;
                }

                bool shouldHaveCategories = LogID.BepInEx.Properties.ShowCategories.IsEnabled;
                bool shouldHaveLineCount = LogID.BepInEx.Properties.ShowLineCount.IsEnabled;

                //TODO: Can we check for other rules present? Rules could be fetched by reflection
                bool shouldApplyRules =
                    (!shouldHaveCategories || shouldHaveLineCount) //Will the active rules affect the log formatting for this file
                  && File.GetLastWriteTime(bepInExLogPath) > Process.GetCurrentProcess().StartTime; //Was the file modified by BepInEx or another Rain World process

                if (shouldApplyRules)
                {
                    UtilityLogger.DebugLog("Applying log rules retroactively");
                    try
                    {
                        FileUtils.TryWrite(bepInExLogPath, RetroactivelyApplyRules(bepInExLogPath));
                    }
                    catch (IOException)
                    {
                        UtilityLogger.LogWarning("Unable to read BepInEx log file");
                    }
                }
                return;
            }

            CleanBepInExFolder();
        }

        /// <summary>
        /// Detects, and removes log file copies in the original BepInEx logs folder directory
        /// </summary>
        internal static void CleanBepInExFolder()
        {
            string[] strayLogFiles = Directory.GetFiles(BepInExPath.RootPath, UtilityConsts.LogNames.BepInEx + "*");

            foreach (string file in strayLogFiles)
            {
                try
                {
                    File.Delete(file);
                }
                catch (IOException)
                {
                    //File may be opened by another process
                }
            }
        }

        /// <summary>
        /// Applies log rules to already logged messages, overwriting the existing file with the new changes
        /// </summary>
        internal static string RetroactivelyApplyRules(string logPath)
        {
            short totalLinesProcessed = 0;

            LogCategory category = null;
            ManualLogSource source = null;

            string message;
            StringBuilder messageBuilder = new StringBuilder(),
                          newFileContent = new StringBuilder();
            foreach (string line in File.ReadAllLines(logPath))
            {
                //BepInEx probably doesn't have any multiline strings, but check anyways just to be safe
                if (line.StartsWith("[")) //Starting with a bracket indicates a line start
                {
                    int headerStart = 1,
                        headerEnd = line.IndexOf(']');

                    string[] headerData = line.Substring(headerStart, headerEnd - 1).Split(':');

                    if (totalLinesProcessed > 0) //For each line after the first, we append a new line to the content buffer
                    {
                        message = buildMessage(category, source);
                        newFileContent.AppendLine().Append(message);
                    }

                    category = new LogCategory(headerData[0].Trim());
                    source = new ManualLogSource(headerData[1].Trim());

                    //Slight chance this could fail - if a false positive line start is handled. It shouldn't happen under typical run conditions
                    int messageStartIndex = headerEnd + 1;

                    if (messageStartIndex != line.Length) //Append if string is not empty
                        messageBuilder.Append(line, messageStartIndex, line.Length - messageStartIndex);

                    totalLinesProcessed++;
                }
                else //While not starting with a bracket, indicates a continuation of a line
                {
                    messageBuilder.AppendLine().Append(line); //Instead of appending the new line to the content buffer, we include it in the line buffer as part of the multiline string
                }
            }

            //The final message does not get built inside the loop
            message = buildMessage(category, source);
            newFileContent.AppendLine().Append(message);

            return newFileContent.ToString();

            string buildMessage(LogCategory category, ManualLogSource source)
            {
                var messageData = new LogRequestEventArgs(LogID.BepInEx, messageBuilder, category)
                {
                    LogSource = source
                };

                string message = LogMessageFormatter.Default.Format(messageData);

                LogID.BepInEx.Properties.MessagesHandledThisSession++; //Count logged messages as if it were just logged
                messageBuilder.Clear(); //Prepare for the next line
                return message;
            }
        }

        internal static void DisposeListeners()
        {
            foreach (var listener in BepInExInfo.Listeners.ToArray())
            {
                UtilityLogger.DebugLog($"Disposing {listener}");
                BepInExInfo.Close(listener);
            }
            UtilityLogger.DebugLog("Dispose successful");
        }
    }
}
