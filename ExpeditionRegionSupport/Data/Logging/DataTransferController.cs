using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ExpeditionRegionSupport.Data.Logging
{
    public delegate void DataHandleDelegate(object dataPacketObject);

    /// <summary>
    /// A static class used to assist in the transfer of data strings from one mod to another
    /// </summary>
    public static class DataTransferController
    {
        private static DataTransferHandler dataController;

        /// <summary>
        /// The primary method of handling data strings shared between one or more mods
        /// </summary>
        public static DataHandleDelegate DataHandler;

        public static List<object> UnhandledDataPackets => dataController.UnhandledDataPackets;

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
            dataController.BroadcastMessage(nameof(DataTransferHandler.ReceiveData), dataPacket, SendMessageOptions.DontRequireReceiver);
        }
    }

    internal class DataTransferHandler : MonoBehaviour
    {
        internal DataHandleDelegate Handler;

        /// <summary>
        /// A list for storing data strings that are received, but are unable to be handled by the mod
        /// </summary>
        internal List<object> UnhandledDataPackets = new List<object>();

        /// <summary>
        /// Receives data sent by other mods
        /// </summary>
        public void ReceiveData(object dataPacketObject)
        {
            Plugin.Logger.LogInfo("Receiving data");
            dynamic dataPacket = dataPacketObject;

            if ((bool)dataPacket.Handled) return;

            if (Handler == null) //This data cannot be handled by this mod yet
            {
                UnhandledDataPackets.Add(dataPacket);
                return;
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
