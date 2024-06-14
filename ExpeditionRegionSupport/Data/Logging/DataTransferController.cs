using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ExpeditionRegionSupport.Data.Logging
{
    public delegate void DataHandleDelegate(DataStorage dataPacketObject);

    /// <summary>
    /// A static class used to assist in the transfer of data strings from one mod to another
    /// </summary>
    public static class DataTransferController
    {
        private static DataTransferHandler dataController;

        /// <summary>
        /// The primary method of handling data strings shared between one or more mods
        /// </summary>
        public static event DataHandleDelegate DataHandler;

        public static void HandleData(DataStorage dataPacket)
        {
            DataHandler(dataPacket);
        }

        public static List<DataStorage> UnhandledDataPackets => dataController.UnhandledDataPackets;

        static DataTransferController()
        {
            dataController = DataTransferHandler.CreateHandler();
            dataController.Handler = DataHandler;
        }

        /// <summary>
        /// Sends data to other remote logger instances
        /// </summary>
        public static void SendData(DataPacketType dataPacketHeader, string dataID, string dataString)
        {
            DataPacket dataPacket = new DataPacket()
            {
                Data = dataString,
                DataID = dataID,
                HeaderID = (int)dataPacketHeader,
                Handled = false
            };

            //Plugin.Logger.LogInfo("Sending data");

            if (dataController == null)
            {
                Debug.Log("Data was unable to be delivered");
                return;
            }

            Plugin.Logger.LogInfo("Sending data");
            dataController.BroadcastMessage(nameof(DataTransferHandler.ReceiveData), dataPacket, SendMessageOptions.DontRequireReceiver);
        }
    }

    internal class DataTransferHandler : MonoBehaviour
    {
        internal DataHandleDelegate Handler;

        /// <summary>
        /// A list for storing data strings that are received, but are unable to be handled by the mod
        /// </summary>
        internal List<DataStorage> UnhandledDataPackets = new List<DataStorage>();

        /// <summary>
        /// Receives data sent by other mods
        /// </summary>
        public void ReceiveData(DataStorage dataPacketObject)
        {
            Plugin.Logger.LogInfo("Receiving data");

            DataStorage dataPacket = dataPacketObject;

            try
            {
                Plugin.Logger.LogInfo(dataPacket.Data);
                Plugin.Logger.LogInfo((DataPacketType)dataPacket.HeaderID);
                Plugin.Logger.LogInfo(dataPacket.DataID);
                Plugin.Logger.LogInfo(dataPacket.Handled);
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError(e);
            }

            if (dataPacket.Handled) return;

            if (Handler == null) //This data cannot be handled by this mod yet
            {
                UnhandledDataPackets.Add(dataPacket);
                return;
            }

            Plugin.Logger.LogInfo("Receiving data");
            try
            {
                Plugin.Logger.LogInfo(dataPacket.Data);
                Plugin.Logger.LogInfo((DataPacketType)dataPacket.HeaderID);
                Plugin.Logger.LogInfo(dataPacket.DataID);
                Plugin.Logger.LogInfo(dataPacket.Handled);
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError(e);
            }
            Handler(dataPacket);
        }

        internal static DataTransferHandler CreateHandler()
        {
            GameObject managerObject = BepInEx.Bootstrap.Chainloader.ManagerObject;

            if (managerObject != null)
                return managerObject.AddComponent<DataTransferHandler>();
            return null;
        }
    }

    public enum DataPacketType
    {
        Unknown = -1,
        ObjectData,
        Request,
        Signal
    }
}
