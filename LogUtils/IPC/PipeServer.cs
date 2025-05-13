using LogUtils.Diagnostics;
using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace LogUtils.IPC
{
    public class PipeServer : NetworkComponent
    {
        public NamedPipeServerStream Receiver = new NamedPipeServerStream(pipeName: "RW-LogUtils");
                                                                        /*direction: PipeDirection.InOut,
                                                                          maxNumberOfServerInstances: 1,
                                                                          transmissionMode: PipeTransmissionMode.Byte,
                                                                          options: PipeOptions.Asynchronous | PipeOptions.WriteThrough,
                                                                          inBufferSize: 1,
                                                                          outBufferSize: 1);*/

        /// <summary>
        /// Used to establish the first server to connect as the primary server - only one primary server is allowed
        /// </summary>
        private NamedPipeServerStream localServer = new NamedPipeServerStream("RW-LogUtils_local");

        /// <summary>
        /// Used to establish the first server to connect as the primary server - only one primary server is allowed
        /// </summary>
        private NamedPipeClientStream localClient = new NamedPipeClientStream("RW-LogUtils_local");

        public bool IsRunning;

        private bool readyForNewUpdate => updateTask == null || updateTask.Status >= TaskStatus.RanToCompletion;

        private Task updateTask;

        internal void Start()
        {
            IsRunning = true;
            Task.Run(Update);
        }

        public void Update()
        {
            UtilityLogger.Log("Server started");

            while (IsRunning)
            {
                if (readyForNewUpdate)
                {
                    UtilityLogger.Log("Server update");

                    Task task = updateTask;

                    if (task != null)
                    {
                        UtilityLogger.Log("Server update complete");

                        if (task.IsFaulted)
                            UtilityLogger.LogError("Server error", task.Exception.InnerException);
                    }

                    try
                    {
                        updateTask = UpdateAsync();
                    }
                    catch (Exception ex)
                    {
                        UtilityLogger.LogError("Server error", ex);
                    }
                }
            }
        }

        internal async Task EstablishLocalConnection()
        {
            UtilityLogger.Log("Establishing local connection");

            Task connectClient = Task.Run(connectLocalClient);
            Task waitForClient = Task.Run(waitForLocalClient);

            Task checkTask = Task.WhenAll(connectClient, waitForClient);

            //Schedule check client to connect, to try to establish a local connection
            //This is expected to fail if another server instance has a local connection by design
            await checkTask;

            //Both tasks must complete successfully to establish a local connection
            if (checkTask.IsFaulted)
                UtilityLogger.LogError("Server error", checkTask.Exception.InnerException);
        }

        internal bool HasClient;

        internal async Task WaitForClientConnection()
        {
            UtilityLogger.Log("Waiting for client to connect");

            while (!HasClient)
            {
                try
                {
                    Receiver.WaitForConnection();
                    HasClient = true;
                }
                catch (Exception ex)
                {
                    if (ErrorCodeProvider.GetCode(ex) == ErrorCode.ProcessAlreadyExists)
                    {
                        HasClient = true;
                        break;
                    }

                    UtilityLogger.LogError("Server error", ex);
                    await Task.Yield();
                }
            }
        }

        internal void DisconnectClient()
        {
            HasClient = false;

            try
            {
                Receiver.Disconnect();
            }
            catch
            {
                //Untrustworthy process
            }

        }

        internal async Task UpdateAsync()
        {
            UtilityLogger.Log("Server update async");

            //Wait for a client to connect to the server
            if (!localClient.IsConnected)
            {
                await EstablishLocalConnection();

                if (localClient.IsConnected)
                {
                    UtilityLogger.Log("Local connection established");

                    bool processFileExists = File.Exists("processes.txt");

                    if (!processFileExists)
                        UtilityLogger.Log("Process file does not exist");

                    //When a new local server has been established, we need to check for the LogUtils created processes.txt. It will provide
                    //information on Rain World processes collected by a once-existing, but now terminated primary server
                    if (processFileExists)
                    {
                        //Read file

                    }
                }
            }

            //Only one server is allowed to wait on client requests at a time
            if (!localClient.IsConnected) return;

            if (!HasClient)
                await WaitForClientConnection();

            ResponseCode response = ResponseCode.Invalid;

            UtilityLogger.Log("Listening for client response");

            //Wait until we receive a response from the client
            while (response == ResponseCode.Invalid)
            {
                response = await ListenForResponse();
            }

            UtilityLogger.Log("Receiving client response: " + response);

            try
            {
                Process(response);
            }
            finally
            {
                DisconnectClient();
                UtilityLogger.Log("Client disconnected");
            }
        }

        internal async Task<ResponseCode> ListenForResponse()
        {
            byte[] receivedBytes = new byte[1];

            //Keep listening until we receive a response
            Task readTask = Receiver.ReadAsync(receivedBytes, 0, 1);

            await readTask;
            return (ResponseCode)receivedBytes[0];
        }

        private void connectLocalClient()
        {
            localClient.Connect();
        }

        private async Task waitForLocalClient()
        {
            UtilityLogger.Log("Waiting for local client");
            //localServer.WaitForConnection();

            bool waitingForResult = true;

            await Task.Run(() =>
            {
                localServer.BeginWaitForConnection((IAsyncResult result) =>
                {
                    waitingForResult = false;
                }, null);
            });

            while (waitingForResult)
            {
                //UtilityLogger.Log("Yielding");
                await Task.Yield();
            }

            Condition.Assert(localClient.IsConnected, new AssertHandler(UtilityLogger.Logger));
        }

        internal void Process(ResponseCode response)
        {
            //TODO: Handle logic
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetNamedPipeClientProcessId(IntPtr pipe, out uint clientProcessId);

        public override string Tag => UtilityConsts.ComponentTags.IPC_SERVER;
    }
}
