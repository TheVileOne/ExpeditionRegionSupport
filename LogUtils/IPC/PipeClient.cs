using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace LogUtils.IPC
{
    public class PipeClient : NetworkComponent
    {
        public int ID;

        internal NamedPipeServerStream Receiver;

        internal Task ReadTask;

        internal static bool ListenForResponses = true; //Static to avoid impacting disposal

        public void Awake()
        {
            enabled = true;

            ID = System.Diagnostics.Process.GetCurrentProcess().Id;
            Receiver = new NamedPipeServerStream($"RW-LogUtils_{ID}", PipeDirection.InOut);

            ReadTask = Task.Run(async () =>
            {
                Receiver.WaitForConnection();

                while (ListenForResponses)
                {
                    try
                    {
                        byte[] receivedBytes = new byte[1];
                        await Receiver.ReadAsync(receivedBytes, 0, 1);

                        ResponseCode response = (ResponseCode)receivedBytes[0];

                        if (response == ResponseCode.Ack)
                            OnConfirmation();
                    }
                    catch (Exception ex)
                    {
                        UtilityLogger.LogError("Client error", ex);
                    }
                }
            });
        }

        private bool confirmationFlag;

        internal void OnConfirmation()
        {
            confirmationFlag = true;
        }

        internal bool ConsumeConfirmation()
        {
            if (confirmationFlag)
            {
                confirmationFlag = false;
                return true;
            }
            return false;
        }

        public async Task<bool> WaitForConfirmation(int timeout = -1)
        {
            bool confirmed = false;

            CancellationTokenSource cancelToken = new CancellationTokenSource(timeout);

            try
            {
                await Task.Run(checkForConfirmation, cancelToken.Token);
            }
            catch (TaskCanceledException)
            {
            }
            return confirmed;

            void checkForConfirmation()
            {
                while (!confirmed)
                {
                    confirmed = ConsumeConfirmation();

                    if (cancelToken.IsCancellationRequested)
                        throw new TaskCanceledException();
                }
            }
        }

        internal static async Task<NamedPipeClientStream> ConnectToServer(string portName)
        {
            NamedPipeClientStream responder = new NamedPipeClientStream(".", portName, PipeDirection.InOut);

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
                        UtilityLogger.LogError("Client error", ex);

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

            UtilityLogger.Log("Connection successful");
            return responder;
        }


        internal void Register()
        {
            UtilityLogger.Log("Client registration");
            var task = SendResponseAsync(ResponseCode.NewClient);

            while (task.Status < TaskStatus.RanToCompletion)
            {
                //busy wait
            }

            if (task.Result)
                UtilityLogger.LogError("Client registered");
            else
            {
                UtilityLogger.LogWarning("Client failed to register. Retrying...");
                Register();
            }
        }

        /// <summary>
        /// Sends a response to IPC server, and waits for server to receive it
        /// </summary>
        /// <returns>Whether response was successful</returns>
        public bool SendResponse(ResponseCode response)
        {
            try
            {
                var task = SendResponseAsync(response);

                SpinWait.SpinUntil(() => task.Status >= TaskStatus.RanToCompletion);

                if (task.IsFaulted)
                    return false;

                return task.Result;
            }
            catch (Exception ex)
            {
                UtilityLogger.LogError(ex);
                return false;
            }
        }

        /// <summary>
        /// Sends a response to IPC server, and waits for server to receive it
        /// </summary>
        /// <param name="timeout">A time period to wait for server before failing to respond</param>
        /// <returns>Whether response was successful</returns>
        public bool SendResponse(ResponseCode response, TimeSpan timeout)
        {
            var task = SendResponseAsync(response);

            bool taskAchieved = SpinWait.SpinUntil(() => task.Status >= TaskStatus.RanToCompletion, (int)timeout.TotalMilliseconds);

            if (!taskAchieved || task.IsFaulted)
                return false;

            return task.Result;
        }

        /// <summary>
        /// Sends a response to IPC server, and waits for server to receive it
        /// </summary>
        /// <returns>A task representing the response</returns>
        public async Task<bool> SendResponseAsync(ResponseCode response)
        {
            //TODO: Cancel support
            UtilityLogger.Log("Sending client response: " + response);

            NamedPipeClientStream responder = await ConnectToServer("RW-LogUtils");

            if (responder == null)
                return false;

            UtilityLogger.Log("Client connected successfully");

            bool responseSent = false;
            using (responder)
            {
                UtilityLogger.Log("Sending response");

                try
                {
                    await responder.Send(response);

                    responseSent = await WaitForConfirmation(20);
                }
                catch (Exception ex)
                {
                    UtilityLogger.LogError("Client error", ex);
                    responseSent = false;
                }
            }

            UtilityLogger.Log("Is client connected? " + responder.IsConnected);
            UtilityLogger.Log("Releasing pipe stream");
            return responseSent;
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
            //TODO: This can be removed if it is not needed
            await Task.CompletedTask;
        }

        ~PipeClient()
        {
            ListenForResponses = false;
        }

        public override string Tag => UtilityConsts.ComponentTags.IPC_CLIENT;
    }

    internal static class ClientExtensions
    {
        public static async Task Send(this NamedPipeClientStream responder,  ResponseCode response)
        {
            await responder.WriteAsync([(byte)response], 0, 1);

            //Keep connection open until response is read
            await Task.Run(responder.WaitForPipeDrain);
        }
    }
}
