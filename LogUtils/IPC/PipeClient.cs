using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace LogUtils.IPC
{
    public class PipeClient : NetworkComponentLegacy
    {
        protected override PipeStream BaseStream => Client;

        public NamedPipeClientStream Client = new NamedPipeClientStream(".", "RW-LogUtils");

        /// <summary>
        /// This flag controls whether this client is permitted to conduct LogUtils services (the first client to connect becomes the primary client) 
        /// </summary>
        public bool IsPrimary;

        /// <summary>
        /// The unique identifier for this client
        /// </summary>
        public int ID;

        public void Awake()
        {
            ID = System.Diagnostics.Process.GetCurrentProcess().Id;

            IsPrimary = true;
            enabled = true;
            UtilityLogger.Log("Client activated");
            SendResponse(ResponseCode.NewClient);
        }

        private Mutex mutex = new Mutex(false, "RW-LogUtils");

        protected override bool EstablishConnection()
        {
            if (Client.IsConnected) return true;

            if (WaitTask == null)
            {
                WaitTask = Task.Run(async () =>
                {
                    //mutex.WaitOne();

                    //lock (SyncLock)
                    {
                        try
                        {
                            if (!Client.IsConnected)
                            {
                                UtilityLogger.Log("Attempting to connect to pipe...");
                                try
                                {
                                    Client.Close();// Connect();
                                }
                                finally
                                {
                                    //await Task.Delay(5000);
                                    ResetStream();
                                }
                            }
                        }
                        finally
                        {
                            //mutex.ReleaseMutex();
                        }
                    }
                });
            }

            //if (WaitTask == null)
            //    WaitTask = Task.Run(Client.Connect);

            //Asynchronously wait for a server to become available. This may take awhile...
            if (WaitTask.IsFaulted)
                UtilityLogger.LogError("Connection wait error", WaitTask.Exception.InnerException);

            if (WaitTask.Status >= TaskStatus.RanToCompletion)
                WaitTask = null;
            return Client.IsConnected;
        }

        protected override void Process(byte[] responseData)
        {
            ResponseCode response = (ResponseCode)responseData[0];

            UtilityLogger.Log("Client received response");
            UtilityLogger.Log(response);

            switch (response)
            {
                case ResponseCode.ReceiveID:
                    {
                        ID = responseData[1];
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
                        UtilityCore.ProcessServer = new PipeServerNew.Server(); //placeholder
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

        public void SendResponse(ResponseCode response)
        {
            Buffer.Enqueue(((byte)response, 0));
        }

        public override void Update()
        {
            UtilityLogger.Log("Sending response 1");
            SendResponse("RW-LogUtils", ResponseCode.Connected);
            UtilityLogger.Log("Sending response 2");
            SendResponse("RW-LogUtils", ResponseCode.NewClient);
            return;
            using (Client = new NamedPipeClientStream("RW-LogUtils"))
            {
                UtilityLogger.Log("Client update start");
                bool hasConnected = EstablishConnection();

                UtilityLogger.Log("Connected: " + hasConnected);

                SendResponse(ResponseCode.Connected);
                if (hasConnected)
                {
                    //ReadFromStream();
                    //WriteToStream();
                }
                UtilityLogger.Log("Client update end");
            }
        }

        /// <summary>
        /// Close the current pipe and create a new instance
        /// </summary>
        protected void ResetStream()
        {
            try
            {
                //Client.Dispose();
            }
            finally
            {
                //Client = new NamedPipeClientStream("RW-LogUtils");
            }
        }

        public override string Tag => UtilityConsts.ComponentTags.IPC_CLIENT;
    }
}
