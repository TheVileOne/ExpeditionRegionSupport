using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ExpeditionRegionSupport.Data.Logging
{
    /// <summary>
    /// A static class used to assist in the transfer of data strings from one mod to another
    /// </summary>
    public static class DataTransferController
    {
        public delegate void DataHandleDelegate(object dataPacketObject);

        /// <summary>
        /// This is the GameObject that contains the game, and its plugins
        /// </summary>
        private static GameObject dataController => BepInEx.Bootstrap.Chainloader.ManagerObject;

        /// <summary>
        /// A list for storing data strings that are received, but are unable to be handled by the mod
        /// </summary>
        public static List<object> UnhandledDataPackets = new List<object>();

        /// <summary>
        /// The primary method of handling data strings shared between one or more mods
        /// </summary>
        public static DataHandleDelegate DataHandler;

        /// <summary>
        /// Sends data to other remote logger instances
        /// </summary>
        public static void SendData(DataPacketType dataPacketHeader, string dataID, string dataString)
        {
            object dataPacket = new
            {
                Data = dataString,
                DataID = dataID,
                DataHeader = dataPacketHeader,
                Handled = false
            };

            if (dataController == null)
            {
                Debug.Log("Data was unable to be delivered");
                return;
            }

            Plugin.Logger.LogInfo("Sending data");
            dataController.BroadcastMessage(nameof(ReceiveData), dataPacket, SendMessageOptions.RequireReceiver);
        }

        /// <summary>
        /// Receives data sent by other mods
        /// </summary>
        public static void ReceiveData(dynamic dataPacketObject)
        {
            Plugin.Logger.LogInfo("Receiving data");
            dynamic dataPacket = dataPacketObject;

            //if (dataPacket.Handled) return;

            if (DataHandler == null) //This data cannot be handled by this mod yet
            {
                UnhandledDataPackets.Add(dataPacket);
                return;
            }

            DataHandler(dataPacket);

            /*
            //Strips that data header from the message string and stores its value as an enum
            DataProcessType processMessage()
            {
                DataProcessType processType = DataProcessType.Unknown;
                string handleSignal = null;
                if (message.StartsWith(DATA_HEADER_OBJECT_DATA))
                {
                    processType = DataProcessType.ObjectData;
                    handleSignal = DATA_HEADER_OBJECT_DATA;
                }
                else if (message.StartsWith(DATA_HEADER_REQUEST))
                {
                    processType = DataProcessType.Request;
                    handleSignal = DATA_HEADER_REQUEST;
                }
                else if (message.StartsWith(DATA_HEADER_SIGNAL))
                {
                    processType = DataProcessType.Signal;
                    handleSignal = DATA_HEADER_SIGNAL;
                }

                if (processType != DataProcessType.Unknown)
                    message = message.Substring(handleSignal.Length + 1); //signal + delimiter
                else
                    Debug.Log("Unrecognized message received");
                return processType;
            }
            */
        }

        //public const string DATA_HEADER_OBJECT_DATA = "object";
        //public const string DATA_HEADER_REQUEST = "request";
        //public const string DATA_HEADER_SIGNAL = "signal";
    }

    public enum DataPacketType
    {
        Unknown = -1,
        ObjectData,
        Request,
        Signal
    }
}
