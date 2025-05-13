using System;

namespace LogUtils.IPC
{
    public abstract class NetworkComponent : UtilityComponent
    {
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
