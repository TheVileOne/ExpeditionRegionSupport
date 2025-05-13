using System;
using System.IO.Pipes;
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

        public async Task SendResponse(ResponseCode response)
        {
            UtilityLogger.Log("Sending client response: " + response);

            NamedPipeClientStream responder = await ConnectToServer();

            if (responder == null) return;

            UtilityLogger.Log("Client connected successfully");

            try
            {
                using (responder)
                {
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

                UtilityLogger.Log("Is client connected? " + responder.IsConnected);
                UtilityLogger.Log("Releasing pipe stream");
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
}
