using LogUtils.Diagnostics;
using System;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace LogUtils.IPC
{
    public static class ProcessMonitor
    {
        /// <summary>
        /// Only one Rain World process can connect to this at a time
        /// </summary>
        private static NamedPipeServerStream monitorServer = new NamedPipeServerStream("RW-LogUtils");

        private static NamedPipeClientStream monitorClient = new NamedPipeClientStream("RW-LogUtils");

        public static bool IsRunning;

        private static bool readyForNewUpdate => updateTask == null || updateTask.Status >= TaskStatus.RanToCompletion;

        private static Task updateTask;

        internal static void Connect()
        {
            if (!IsRunning)
                Task.Run(Update);
        }

        internal static void Disconnect()
        {
            IsRunning = false;
        }

        internal static void Update()
        {
            IsRunning = true;

            while (IsRunning)
            {
                if (readyForNewUpdate)
                {
                    Task task = updateTask;

                    if (task != null)
                    {
                        if (task.IsFaulted)
                            UtilityLogger.LogError(task.Exception.InnerException);
                    }

                    try
                    {
                        updateTask = UpdateAsync();
                    }
                    catch (Exception ex)
                    {
                        UtilityLogger.LogError(ex);
                    }
                }
            }
        }

        internal static async Task UpdateAsync()
        {
            if (monitorClient.IsConnected)
                return;

            //Wait for a client to connect to the server
            await EstablishConnection();

            if (monitorClient.IsConnected)
                UtilityLogger.Log("Connection established");
        }

        internal static async Task EstablishConnection()
        {
            UtilityLogger.Log("Connecting to IPC monitor");

            Task connectClientTask = Task.Run(monitorClient.Connect);
            Task waitForClientTask = Task.Run(waitForClient);

            Task checkTask = Task.WhenAll(connectClientTask, waitForClientTask);

            await checkTask;

            //Both tasks must complete successfully to establish a local connection
            if (checkTask.IsFaulted)
                UtilityLogger.LogError("Server error", checkTask.Exception.InnerException);
        }

        private static async Task waitForClient()
        {
            UtilityLogger.Log("Waiting for monitor client");

            bool waitingForResult = true;

            await Task.Run(() =>
            {
                monitorServer.BeginWaitForConnection((IAsyncResult result) =>
                {
                    waitingForResult = false;
                }, null);
            });

            while (waitingForResult)
            {
                //UtilityLogger.Log("Yielding");
                await Task.Yield();
            }

            Condition.Assert(monitorClient.IsConnected, new AssertHandler(UtilityLogger.Logger));
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetNamedPipeClientProcessId(IntPtr pipe, out uint clientProcessId);
    }
}
