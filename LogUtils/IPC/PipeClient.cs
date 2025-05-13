using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace LogUtils.IPC
{
    public class PipeClient : NetworkComponent
    {
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


        /// <summary>
        /// Sends a response to IPC server, and waits for server to receive it
        /// </summary>
        /// <returns>Whether response was successful</returns>
        public bool SendResponse(ResponseCode response)
        {
            var task = SendResponseAsync(response);

            SpinWait.SpinUntil(() => task.Status >= TaskStatus.RanToCompletion);

            if (task.IsFaulted)
                return false;

            return task.Result;
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

            NamedPipeClientStream responder = await ConnectToServer();

            if (responder == null)
                return false;

            UtilityLogger.Log("Client connected successfully");

            bool responseSent = false;
            using (responder)
            {
                UtilityLogger.Log("Sending response");

                try
                {
                    byte[] bytes = [(byte)response];
                    await responder.WriteAsync(bytes, 0, 1);

                    //Keep connection open until response is read
                    await Task.Run(responder.WaitForPipeDrain);

                    responseSent = true;
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

        public override string Tag => UtilityConsts.ComponentTags.IPC_CLIENT;
    }
}
