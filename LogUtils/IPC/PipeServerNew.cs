using LogUtils.Diagnostics;
using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace LogUtils.IPC
{
    public class PipeServerNew
    {
        public class Client : NetworkComponent
        {
            public static NamedPipeClientStream Instance;

            public void Awake()
            {
                enabled = true;
                UtilityLogger.Log($"Client registered [ID {System.Diagnostics.Process.GetCurrentProcess().Id}]");
            }

            internal async Task<NamedPipeClientStream> ConnectToServer()
            {
                NamedPipeClientStream responder = new NamedPipeClientStream(".", "RW-LogUtils", PipeDirection.InOut);

                int connectionAttempts = 0;
                await Task.Run(() =>
                {
                    while (connectionAttempts < 10)
                    {
                        try
                        {
                            responder.Connect();
                            break;
                        }
                        catch (Exception ex)
                        {
                            UtilityLogger.LogError(ex);

                            //Server may be busy
                            connectionAttempts++;
                        }
                    }
                });

                //Check that we were able to connect, and if we did return the connected client, otherwise clean up resources
                if (connectionAttempts == 10)
                {
                    UtilityLogger.LogWarning("Connection timed out");
                    responder.Dispose();
                    return null;
                }
                return responder;
            }

            public async Task SendResponse(ResponseCode response)
            {
                UtilityLogger.Log("Sending client response: " + response);

                NamedPipeClientStream responder = await ConnectToServer();

                if (responder != null)
                    Instance = responder;

                if (responder == null) return;

                UtilityLogger.Log("Client connected successfully");

                try
                {
                    UtilityCore.ProcessServer.Update();
                    using (responder)
                    {
                        //for (int i = 0; i < 100; i++)
                        {
                            //Send response to server
                            //responder.WriteByte((byte)response);

                            UtilityLogger.Log("Sending response");

                            byte[] bytes = [(byte)response];
                            await responder.WriteAsync(bytes, 0, 1);

                            int serverAck = responder.ReadByte();

                            if ((ResponseCode)serverAck == ResponseCode.Ack)
                            {
                                UtilityLogger.Log("Acknowledged");
                            }
                        }

                        //Keep connection open until response is read
                        await Task.Run(responder.WaitForPipeDrain);
                        //await Task.Delay(100000);

                        UtilityLogger.Log("Is client connected? " + responder.IsConnected);
                        UtilityLogger.Log("Releasing pipe stream");
                    }
                }
                catch (Exception ex)
                {
                    UtilityLogger.LogError("Client error", ex);
                }
            }

            private Task updateTask;

            public void Update()
            {
                if (updateTask == null)
                {
                    updateTask = UpdateAsync();
                    return;
                }

                if (updateTask.Status >= TaskStatus.RanToCompletion)
                {
                    if (updateTask.IsFaulted)
                        UtilityLogger.LogError(updateTask.Exception.InnerException);

                    updateTask = null;
                }
            }

            internal async Task UpdateAsync()
            {
                UtilityLogger.Log("Sending response 1");
                await SendResponse(ResponseCode.Connected);
                //UtilityLogger.Log("Sending response 2");
                //await SendResponse(ResponseCode.NewClient);
            }

            public override string Tag => UtilityConsts.ComponentTags.IPC_CLIENT;
        }

        public class Server : NetworkComponent
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

            private Task updateTask;

            public void Update()
            {
                UpdateOld();
                return;

                UtilityLogger.Log("Server update");

                if (updateTask != null)
                {
                    if (updateTask.Status == TaskStatus.WaitingForActivation)
                        updateTask = null;
                    else
                        UtilityLogger.Log(updateTask.Status);
                }

                if (updateTask == null)
                {
                    updateTask = UpdateAsync();
                    return;
                }

                if (updateTask.Status >= TaskStatus.RanToCompletion)
                {
                    if (updateTask.IsFaulted)
                        UtilityLogger.LogError("Server error", updateTask.Exception.InnerException);

                    updateTask = null;
                }
            }

            public void UpdateOld()
            {
                UtilityLogger.Log("Server update start");

                if (Receiver.SafePipeHandle.IsClosed)
                {
                    UtilityLogger.Log("Server disconnected");
                    Receiver.Disconnect();
                    return;
                }

                try
                {
                    UtilityLogger.Log("Server read");

                    if (updateTask == null)
                    {
                        updateTask = Task.Run(() =>
                        {
                            try
                            {
                                Receiver.WaitForConnection();
                            }
                            catch
                            {
                            }
                        });
                    }

                    //UtilityLogger.Log("Update state:" + updateTask.Status);

                    if (updateTask.Status < TaskStatus.RanToCompletion)
                        return;

                    updateTask = null;

                    UtilityLogger.Log("Listening...");
                    var result = ListenForResponseOld().Result;//ListenForResponseAsync().Result;

                    UtilityLogger.LogWarning(result);

                    try
                    {
                        UtilityLogger.LogWarning(result + " " + 1);
                        if (result != ResponseCode.Invalid)
                        {
                            uint clientProcessId;
                            if (GetNamedPipeClientProcessId(Receiver.SafePipeHandle.DangerousGetHandle(), out clientProcessId))
                            {
                                UtilityLogger.Log($"Received: {result} from PID {clientProcessId}");
                            }
                            else
                            {
                                UtilityLogger.Log("Failed to get client process ID.");
                            }
                        }
                        UtilityLogger.LogWarning(result + " " + 2);
                    }
                    catch
                    {
                    }

                    try
                    {
                        Receiver.Disconnect();
                        UtilityLogger.LogWarning(result + " " + 3);
                        Receiver.WaitForConnection();
                        UtilityLogger.LogWarning(result + " " + 4);
                    }
                    catch
                    {
                        UtilityLogger.LogWarning(result + " " + "error");
                    }
                    UtilityLogger.LogWarning(result + " " + 5);
                    UtilityLogger.Log("Server write");
                }
                catch (Exception ex)
                {
                    UtilityLogger.LogError(ex);
                }

                UtilityLogger.Log("Server update end");
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

                UtilityLogger.Log("Has client: " + HasClient);

                if (!HasClient) return;

                ResponseCode response = ResponseCode.Invalid;

                UtilityLogger.Log("Listening for client response");

                while (response == ResponseCode.Invalid)
                {
                    response = await ListenForResponseAsync();
                    //await Task.Yield();
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

            bool waitingForResults = false;
            internal ResponseCode ListenForResponse()
            {
                byte[] receivedBytes = new byte[1];

                waitingForResults = true;
                try
                {
                    var result = Receiver.BeginRead(receivedBytes, 0, 1, (IAsyncResult result) =>
                    {
                        try
                        {
                            if (receivedBytes[0] != 0)
                            {
                                UtilityLogger.Log("Server received a response");
                                Receiver.WriteByte((byte)ResponseCode.Ack);
                                Receiver.EndRead(result);
                            }
                        }
                        catch (Exception ex)
                        {
                            UtilityLogger.LogError("Server error", ex);
                        }
                        waitingForResults = false;
                    }, null);
                }
                catch (Exception ex)
                {
                    UtilityLogger.LogError("Server error", ex);
                }
                
                while (waitingForResults)
                { }

                return (ResponseCode)receivedBytes[0];
            }

            Task ReadTask;

            internal async Task<ResponseCode> ListenForResponseOld()
            {
                byte[] receivedBytes = new byte[1];
                if (ReadTask == null)
                    ReadTask = Receiver.ReadAsync(receivedBytes, 0, 1);

                var task = ReadTask;
                var taskStatus = task.Status;

                if (taskStatus != TaskStatus.RanToCompletion)
                {
                    if (taskStatus > TaskStatus.RanToCompletion) //Task wont ever complete
                    {
                        if (taskStatus == TaskStatus.Faulted)
                            UtilityLogger.LogWarning("Read fault");
                        ReadTask = null;
                    }

                    return ResponseCode.Invalid;
                }

                ReadTask = null;
                return (ResponseCode)receivedBytes[0];
            }

            internal async Task<ResponseCode> ListenForResponseAsync()
            {
                byte[] receivedBytes = new byte[1];

                //Keep listening until we receive a response
                Task readTask = Receiver.ReadAsync(receivedBytes, 0, 1);

                await readTask;
                return (ResponseCode)receivedBytes[0];

                bool waitingForResults = true;
                var result = Receiver.BeginRead(receivedBytes, 0, 1, (IAsyncResult result) =>
                {
                    if (receivedBytes[0] != 0)
                    {
                        UtilityLogger.Log("Server received a response");
                        Receiver.WriteByte((byte)ResponseCode.Ack);
                        Receiver.EndRead(result);
                    }
                    waitingForResults = false;
                }, null);

                //Wait until we receive a response
                while (waitingForResults)//readTask.Status < TaskStatus.RanToCompletion)
                {
                    UtilityLogger.Log("Yielding");
                    //await Task.Yield();
                }

                //UtilityLogger.Log("Task status: " + readTask.Status);

                //if (readTask.IsFaulted)
                //{
                //    UtilityLogger.LogError("Server error", readTask.Exception.InnerException);
                //    return ResponseCode.Invalid;
                //}

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
}
