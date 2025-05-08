using System;
using System.Collections.Generic;
using System.IO.Pipes;

namespace LogUtils.IPC
{
    public sealed class PipeServer : NetworkComponent
    {
        protected override PipeStream BaseStream => Server;

        public readonly NamedPipeServerStream Server = new NamedPipeServerStream("RW-LogUtils");

        /// <summary>
        /// The total amount of Rain World clients that have connected (includes disconnected clients)
        /// </summary>
        public byte TotalClients { get; private set; }

        /// <summary>
        /// Contains the ID of the Rain World client that maintains the server
        /// </summary>
        public byte ServerID { get; private set; }

        /// <summary>
        /// A list of Rain World client IDs known to still be active
        /// </summary>
        public List<byte> ActiveClientIDs = new List<byte>();

        /// <summary>
        /// A list of Rain World client IDs known to still be active that have logging privileges
        /// </summary>
        public List<byte> ActivePrimaryClientIDs = new List<byte>();

        public bool IsServerClient(byte clientID) => ServerID == clientID;

        protected override void Process(byte[] messageData)
        {
            ResponseCode response = (ResponseCode)messageData[0];
            byte clientID = messageData[1];

            switch (response)
            {
                case ResponseCode.Connected:
                    break;
                case ResponseCode.ClientLost:
                    {
                        RemoveClient(clientID);
                        break;
                    }
                case ResponseCode.PrimaryClientLost:
                    {
                        RemovePrimaryClient(clientID);
                        break;
                    }
                case ResponseCode.NewClient:
                    {
                        AddClient();
                        break;
                    }
                case ResponseCode.AddPrivileges:
                    {
                        AddPrivileges(clientID);
                        break;
                    }
                case ResponseCode.RevokePrivileges:
                    {
                        RevokePrivileges(clientID);
                        break;
                    }
                case ResponseCode.ReceiveID: //Server doesn't need to handle these requests
                case ResponseCode.RequestNewPrimary:
                case ResponseCode.RequestNewPrimaryServer:
                    break;
                default:
                case ResponseCode.Invalid:
                    throw new InvalidResponseException();
            }
        }

        internal void AddClient()
        {
            int totalActiveClients = ActiveClientIDs.Count;

            if (totalActiveClients == byte.MaxValue)
                throw new ClientOverflowException();

            byte clientID = TotalClients++;

            //The very unlikely situation where we have assigned all available IDs and need to recycle
            if (clientID > byte.MaxValue)
            {
                if (ActiveClientIDs[0] != 0)
                    clientID = 0;
                else if (totalActiveClients == 1)
                    clientID = 1;
                else
                {
                    bool clientFound = false;

                    //Find the earliest available ID
                    for (int i = 1; i < totalActiveClients; i++)
                    {
                        if (ActiveClientIDs[i] - ActiveClientIDs[i - 1] > 1)
                        {
                            clientID = (byte)(ActiveClientIDs[i - 1] + 1);
                            break;
                        }
                    }

                    if (!clientFound)
                    {
                        //Entire client ID list must not contain any number gaps for this to execute
                        clientID = (byte)(ActiveClientIDs[totalActiveClients - 1] + 1);
                    }
                }
            }
            ActiveClientIDs.Add(clientID);
        }

        internal void RemoveClient(byte clientID)
        {
            ActiveClientIDs.Remove(clientID);
        }

        internal void RemovePrimaryClient(byte clientID)
        {
            ActiveClientIDs.Remove(clientID);
            RevokePrivileges(clientID);
        }

        internal void AddPrivileges(byte clientID)
        {
            //The first client to register becomes the default server controller. This is enforced clientside
            if (ActiveClientIDs.Count == 0)
                ServerID = clientID;

            int totalPrimaryClients = ActivePrimaryClientIDs.Count;

            bool requiresInsertion = totalPrimaryClients > 0 && ActivePrimaryClientIDs[totalPrimaryClients - 1] > clientID;

            if (requiresInsertion)
            {
                int index = totalPrimaryClients - 1;
                while (clientID > ActivePrimaryClientIDs[index])
                    index--;
                ActivePrimaryClientIDs.Insert(index + 1, clientID); //index + 1 targets the index after the value that is below clientID
                return;
            }
            ActivePrimaryClientIDs.Add(clientID);
        }

        internal void RevokePrivileges(byte clientID)
        {
            if (!ActivePrimaryClientIDs.Contains(clientID))
            {
                UtilityLogger.Log("Client does not have logging privileges");
                return;
            }

            ActivePrimaryClientIDs.Remove(clientID);

            if (ActiveClientIDs.Count == 0) //Most likely Rain World's only process is shutting down
            {
                UtilityLogger.Log("No clients available to select");
                return;
            }

            int totalPrimaryClients = ActivePrimaryClientIDs.Count;

            ResponseCode request;
            byte requestClient;
            if (IsServerClient(clientID))
            {
                //The client with the lowest ID gets selected to be the new server host with primary clients receiving priority over regular clients
                request = ResponseCode.RequestNewPrimaryServer;
                requestClient = totalPrimaryClients > 0 ? ActivePrimaryClientIDs[0] : ActiveClientIDs[0];

                SendResponse(request, requestClient, NOT_IMPORTANT);
                return;
            }

            //No need to assign a new primary client as long as we have at least one available
            if (totalPrimaryClients > 0)
                return;

            //Request an existing client to receive logging privileges that does not manage the server
            request = ResponseCode.RequestNewPrimary;
            requestClient = ActiveClientIDs[0];
            SendResponse(request, requestClient, NOT_IMPORTANT);
        }

        /// <summary>
        /// The client ID that is currently connected to the server
        /// </summary>
        internal byte ConnectionID;

        internal Dictionary<int, byte[]> OutgoingResponseData;

        internal void SendResponse(ResponseCode response, byte responseTarget, byte responseValue)
        {
            //Check whether we can send this response immediately
            if (ConnectionID == responseTarget)
            {
                Server.WriteByte((byte)response);
                Server.WriteByte(responseValue);
                return;
            }

            //Otherwise, we have to stash it and handle it later - This will overwrite any previously unhandled responses.
            //Try to ensure that a single client doesn't need to receive more than one pending response
            OutgoingResponseData[responseTarget] = [(byte)response, responseValue]; 
        }

        private HeaderData readDataHeader()
        {
            const int HEADER_SIZE = 5;

            byte[] headerData = new byte[HEADER_SIZE];
            int bytesRead = Client.Read(headerData, 0, HEADER_SIZE);

            if (bytesRead == 0)
                return HeaderData.None;

            if (bytesRead < HEADER_SIZE) //Invalid header
            {
                //TODO: throw exception?
                return HeaderData.None;
            }

            var header = new HeaderData
            {
                MessageID = headerData[0],
                MessageSize = BitConverter.ToInt32(headerData, 1)
            };
            return header;
        }

        protected override bool EstablishConnection()
        {
            if (Server.IsConnected) return true;

            if (WaitTask == null)
                WaitTask = Server.WaitForConnectionAsync();

            //Asynchronously wait for a client to signal us. This may take awhile...
            if (!WaitTask.IsCompleted)
            {
                if (WaitTask.IsFaulted)
                    UtilityLogger.LogError("Connection wait error", WaitTask.Exception);
            }
            WaitTask = null;
            return Server.IsConnected;
        }

        public override string Tag => UtilityConsts.ComponentTags.IPC_SERVER;
    }

    public struct HeaderData
    {
        public static HeaderData None = default;

        public byte MessageID;
        public int MessageSize;

        public bool IsValid()
        {
            if (Equals(None))
                return false;

            switch (MessageID)
            {
                case 2: //example
                    return MessageSize == 6;
            }
            return false;
        }
    }
}
