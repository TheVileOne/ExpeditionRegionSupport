using LogUtils.Diagnostics;
using System;
using System.Diagnostics;
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
        private static NamedPipeServerStream monitorServer;

        private static NamedPipeClientStream monitorClient;

        public static bool IsRunning;

        private static bool readyForNewUpdate => updateTask == null || updateTask.Status >= TaskStatus.RanToCompletion;

        /// <summary>
        /// The initialization process either has not run, or there was an error preventing it from completing
        /// </summary>
        public static bool RequiresInitialization = true;

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

        internal static bool TryInit()
        {
            try
            {
                monitorServer = new NamedPipeServerStream("RW-LogUtils");
                monitorClient = new NamedPipeClientStream("RW-LogUtils");
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal static void Update()
        {
            IsRunning = true;

            while (IsRunning)
            {
                //Constructing pipe server instances on secondary Rain World instances throw on construction - we need to keep trying
                //to initalize here until it completes
                if (RequiresInitialization)
                {
                    if (TryInit())
                    {
                        UtilityLogger.Logger.LogMessage($"Init successful [{Process.GetCurrentProcess().Id}]");
                        RequiresInitialization = false;
                    }
                    continue;
                }

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
