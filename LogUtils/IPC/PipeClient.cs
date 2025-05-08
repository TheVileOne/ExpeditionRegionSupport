using System.IO.Pipes;

namespace LogUtils.IPC
{
    public class PipeClient : NetworkComponent
    {
        protected override PipeStream BaseStream => Client;

        public readonly NamedPipeClientStream Client = new NamedPipeClientStream("RW-LogUtils");

        /// <summary>
        /// This flag controls whether this client is permitted to conduct LogUtils services (the first client to connect becomes the primary client) 
        /// </summary>
        public bool IsPrimary;

        /// <summary>
        /// The unique identifier for this client
        /// </summary>
        public byte ID;

        protected override bool EstablishConnection()
        {
            if (Client.IsConnected) return true;

            if (WaitTask == null)
                WaitTask = Client.ConnectAsync();

            //Asynchronously wait for a server to become available. This may take awhile...
            if (!WaitTask.IsCompleted)
            {
                if (WaitTask.IsFaulted)
                    UtilityLogger.LogError("Connection wait error", WaitTask.Exception);
            }
            WaitTask = null;
            return Client.IsConnected;
        }

        protected override void Process(byte[] messageData)
        {
            ResponseCode response = (ResponseCode)messageData[0];

            switch (response)
            {
                case ResponseCode.ReceiveID:
                    {
                        ID = messageData[1];
                        break;
                    }
                case ResponseCode.RequestNewPrimary:
                    {
                        IsPrimary = true;
                        break;
                    }
                case ResponseCode.RequestNewPrimaryServer:
                    {
                        IsPrimary = true;
                        UtilityCore.ProcessServer = new PipeServer(); //placeholder
                        break;
                    }
                case ResponseCode.ClientLost: //Client doesn't need to handle these requests
                case ResponseCode.PrimaryClientLost:
                case ResponseCode.NewClient:
                case ResponseCode.AddPrivileges:
                case ResponseCode.RevokePrivileges:
                    break;
                default:
                case ResponseCode.Invalid:
                    throw new InvalidResponseException();
            }
        }

        public void SendResponse(ResponseCode response, byte responseValue)
        {
            Buffer.Enqueue(((byte)response, responseValue));
        }

        public override string Tag => UtilityConsts.ComponentTags.IPC_CLIENT;
    }
}
