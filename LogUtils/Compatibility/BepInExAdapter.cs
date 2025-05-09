﻿using BepInEx.Logging;
using System.Collections.Generic;
using System.Linq;

namespace LogUtils.Compatibility
{
    /// <summary>
    /// Adapter service for converting the BepInEx logging system to the system that LogUtils operates
    /// </summary>
    internal static class BepInExAdapter
    {
        internal static Listeners.DiskLogListener LogListener;

        public static void Run()
        {
            LogListener = new Listeners.DiskLogListener(new TimedLogWriter());

            AdaptLoggingSystem();
            TransferData();
        }

        /// <summary>
        /// Transitions the BepInEx logging system, and data over to the system operated by LogUtils
        /// </summary>
        internal static void AdaptLoggingSystem()
        {
            ICollection<ILogListener> listeners = GetListeners();

            //Find the LogListener that writes to the BepInEx root directory
            ILogListener found = listeners.FirstOrDefault(l => l is BepInEx.Logging.DiskLogListener);

            //This listener is incompatible with LogUtils, and must be replaced
            if (found != null)
            {
                found.Dispose(); //This will flush any messages held by the original listener
                listeners.Remove(found);
            }

            listeners.Add(LogListener);
        }

        /// <summary>
        /// Migrates existing log file over to new file when necessary
        /// </summary>
        internal static void TransferData()
        {
            /*
            LogProperties logProperties = LogID.BepInEx.Properties;

            //This code wont support changes to only the file extension
            bool hasDefaultPath = PathUtils.PathsAreEqual(logProperties.CurrentFolderPath, logProperties.OriginalFolderPath);
            bool hasDefaultFileName = ComparerUtils.FilenameComparer.Equals(logProperties.CurrentFilename, UtilityConsts.LogNames.BepInEx);

            bool fileMoveRequired = !hasDefaultPath || !hasDefaultFileName;

            if (fileMoveRequired)
            {
                string originalLogPath = Path.Combine(logProperties.OriginalFolderPath, UtilityConsts.LogNames.BepInEx + FileExt.LOG);

                //Due to BepInEx log file already existing by the time this assembly is loaded
                LogFile.Move(Path.Combine(originalLogPath, LogID.BepInEx.Properties.CurrentFilePath), logProperties.CurrentFilePath);
            }
            */
        }

        internal static ICollection<ILogListener> GetListeners()
        {
            return BepInEx.Logging.Logger.Listeners;
        }

        internal static void DisposeListeners()
        {
            try
            {
                ICollection<ILogListener> listeners = GetListeners();

                foreach (var listener in listeners.ToArray())
                {
                    UtilityLogger.DebugLog($"Disposing {listener}");
                    listener.Dispose();
                    listeners.Remove(listener);
                }
                UtilityLogger.DebugLog("Dispose successful");
            }
            catch
            {
                UtilityLogger.DebugLog("Dispose process encountered an error");
            }
        }
    }
}
