using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace LogUtils.IPC
{
    public abstract class NetworkComponent : UtilityComponent
    {
        protected abstract PipeStream BaseStream { get; }

        public Queue<(byte, byte)> Buffer = new Queue<(byte, byte)>();

        protected Task WaitTask;

        protected abstract bool EstablishConnection();

        protected abstract void Process(byte[] responseData);

        private void readFromStream()
        {
            PipeStream stream = BaseStream;

            byte[] responseData = new byte[2];

            const byte response_flag = 0;
            const byte response_value = 1;

            short attemptsAllowed = 20;
            while (stream.IsConnected && attemptsAllowed > 0)
            {
                int rawInput = stream.ReadByte();

                if (rawInput == -1)
                {
                    attemptsAllowed--;
                    continue;
                }

                byte readByte = (byte)rawInput;

                //Server will read a flag followed by a value. All data transmitted through the server needs to follow this pattern
                if (responseData[response_flag] == 0)
                    responseData[response_flag] = readByte;
                else
                {
                    responseData[response_value] = readByte;
                    Process(responseData);

                    //Since a flag, and a value has been read, the buffer can be reset to accept a new flag and value
                    responseData[response_flag] = 0;
                }
                attemptsAllowed = 20; //As long as the client is sending new data, we shouldn't pressure it that harshly
            }
        }

        private void writeFromStream()
        {
            PipeStream stream = BaseStream;

            while (Buffer.Count > 0)
            {
                var messageData = Buffer.Dequeue();

                stream.WriteByte(messageData.Item1);
                stream.WriteByte(messageData.Item2);
            }
        }

        public void Update()
        {
            bool hasConnected = EstablishConnection();

            if (hasConnected)
            {
                readFromStream();
                writeFromStream();
            }
        }

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
    }
}
