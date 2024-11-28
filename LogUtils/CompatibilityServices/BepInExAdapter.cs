using BepInEx.Logging;
using System.Collections.Generic;

namespace LogUtils.CompatibilityServices
{
    /// <summary>
    /// Adapter service for converting the BepInEx logging system to the system that LogUtils operates
    /// </summary>
    internal static class BepInExAdapter
    {
        private static BepInExDiskLogListener _listener;

        public static void Run()
        {
            _listener = new BepInExDiskLogListener(new TimedLogWriter());

            AdaptLoggingSystem();
            TransferData();
        }

        /// <summary>
        /// Transitions the BepInEx logging system, and data over to the system operated by LogUtils
        /// </summary>
        internal static void AdaptLoggingSystem()
        {
            List<ILogListener> listeners = GetListeners();

            //The first DiskLogListener should be the one that writes to the BepInEx root directory
            int listenerIndex = listeners.FindIndex(l => l is DiskLogListener);

            if (listenerIndex >= 0)
            {
                listeners[listenerIndex].Dispose(); //This will flush any messages held by the original listener
                listeners[listenerIndex] = _listener;
            }
            else
                listeners.Add(_listener);
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

        internal static List<ILogListener> GetListeners()
        {
            return (List<ILogListener>)BepInEx.Logging.Logger.Listeners;
        }
    }
}
