using LogUtils.Threading;
using System.Diagnostics;
using System.IO.Pipes;

namespace LogUtils.IPC
{
    public static class ProcessMonitor
    {
        private static NamedPipeServerStream connection;

        /// <summary>
        /// This process has control of the PipeServer that all LogUtils instances attempt to establish a connection with
        /// </summary>
        internal static bool IsConnected => connection != null;

        private static bool isWaitingOnFirstUpdate = true;

        private static Task connectTask;

        internal static void Connect()
        {
            connectTask = new Task(() =>
            {
                establishConnection();

                isWaitingOnFirstUpdate = false;

                if (RWInfo.IsShuttingDown)
                {
                    UtilityLogger.Logger.LogMessage($"Connection attempts canceled due to process shutdown [{Process.GetCurrentProcess().Id}]");
                    Disconnect();
                    return;
                }

                if (IsConnected)
                {
                    UtilityLogger.Logger.LogMessage($"Connection established [{Process.GetCurrentProcess().Id}]");
                    connectTask.Complete();
                    connectTask = null;
                }
            }, 1);
            connectTask.Name = "ProcessMonitor";
            connectTask.IsContinuous = true;

            LogTasker.Schedule(connectTask);
        }

        internal static void Disconnect()
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
        internal static void WaitOnConnectionStatus()
        {
            Task.WaitUntil(() => !isWaitingOnFirstUpdate).Wait();
        }
    }
}
