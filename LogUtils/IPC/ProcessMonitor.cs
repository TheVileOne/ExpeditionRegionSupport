using LogUtils.Threading;
using System.Diagnostics;
using System.IO.Pipes;

namespace LogUtils.IPC
{
    internal static class ProcessMonitor
    {
        private static NamedPipeServerStream connection;

        /// <summary>
        /// This process has control of the PipeServer that all LogUtils instances attempt to establish a connection with
        /// </summary>
        public static bool IsConnected => connection != null;

        private static bool isWaitingOnFirstUpdate = true;

        private static Task connectTask;

        public static void Connect()
        {
            connectTask = new Task(() =>
            {
                if (RainWorldInfo.IsShuttingDown)
                {
                    UtilityLogger.Logger.LogMessage($"Connection attempts canceled due to process shutdown [{Process.GetCurrentProcess().Id}]");
                    Disconnect();
                    return;
                }

                establishConnection();

                isWaitingOnFirstUpdate = false;

                if (IsConnected)
                {
                    UtilityCore.OnProcessConnected();

                    UtilityLogger.Logger.LogMessage($"Connection established [{Process.GetCurrentProcess().Id}]");
                    connectTask.Complete();
                    connectTask = null;
                }
            }, 1);
            connectTask.Name = "ProcessMonitor";
            connectTask.IsContinuous = true;

            LogTasker.Schedule(connectTask);
        }

        public static void Disconnect()
        {
            connectTask?.Cancel();

            try
            {
                connection?.Dispose();
            }
            catch
            {
                //Errors are unimportant here
            }
            connection = null;
        }

        private static void establishConnection()
        {
            try
            {
                connection = new NamedPipeServerStream("RW-LogUtils");
            }
            catch
            {
                //Will fail if connection is established by another process
            }
        }

        /// <summary>
        /// Block thread until connection status has been verified
        /// </summary>
        public static void WaitOnConnectionStatus()
        {
            Task.WaitUntil(() => !isWaitingOnFirstUpdate).Wait();
        }
    }
}
