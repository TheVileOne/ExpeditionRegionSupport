using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace LogUtils.IPC
{
    public abstract class NetworkComponent : UtilityComponent
    {
    }

    public abstract class NetworkComponentLegacy : UtilityComponent
    {
        public static object SyncLock = new object(); 

        protected abstract PipeStream BaseStream { get; }

        public Queue<(byte, byte)> Buffer = new Queue<(byte, byte)>();

        protected Task WaitTask;

        protected abstract bool EstablishConnection();

        protected abstract void Process(byte[] responseData);

        private FileAccess busyState;

        protected void SendResponse(string pipeName, ResponseCode response)
        {
            var responder = new NamedPipeClientStream(pipeName);

            using (responder)
            {
            retry:
                try
                {
                    if (!responder.IsConnected)
                    {
                        responder.Connect();
                        UtilityLogger.Log("Client connected");
                        responder.WriteByte((byte)response);
                        responder.WaitForPipeDrain();
                    }
                }
                catch(Exception ex) when (ex is InvalidOperationException || ex is Win32Exception)
                {
                    UtilityLogger.LogError(ex);
                    goto retry;
                }
            }

            //UtilityLogger.Log("Closing client");
            //responder.Close();
            //UtilityCore.ProcessServer.Server.Disconnect();
            //UtilityCore.ProcessServer.Update();
        }

        public abstract void Update();

        public const byte NOT_IMPORTANT = 255;
    }

    public class ClientOverflowException() : Exception("Number of active clients has exceeded maximum allowed value")
    {
    }

    public class InvalidResponseException() : Exception("Response code is not recognized")
    {
    }

    public enum ResponseCode : byte
    {
        Invalid = 0,
        Connected = 1,
        ClientLost = 2,
        PrimaryClientLost = 3,
        NewClient = 4,
        AddPrivileges = 5,
        RevokePrivileges = 6, //Remove primary status (clientside)
        ReceiveID = 7,
        RequestNewPrimary = 8,
        RequestNewPrimaryServer = 9,
        Ack = 10
    }
}
