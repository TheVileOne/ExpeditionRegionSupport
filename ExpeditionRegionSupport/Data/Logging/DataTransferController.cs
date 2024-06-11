using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Data.Logging
{
    /// <summary>
    /// A static class used to assist in the transfer of data strings from one mod to another
    /// </summary>
    public static class DataTransferController
    {
        public static ExtEnumType DataHandler;

        public static List<string> UnhandledMessages => DataHandler.entries;

        /// <summary>
        /// Handle activities that are required for proper functionality of the class
        /// </summary>
        public static void Initialize()
        {
            foreach (KeyValuePair<Type, ExtEnumType> entry in ExtEnumBase.valueDictionary)
            {
                if (entry.Key.Name.Contains(nameof(DataMessage)))
                {
                    DataHandler = entry.Value;
                    break;
                }
            }

            if (DataHandler == null)
            {
                DataMessage dataMessage = new DataMessage("data", false);
                DataHandler = ExtEnumBase.valueDictionary[dataMessage.GetType()];
            }
        }

        public static void SendMessage(string message)
        {
            DataHandler.AddEntry(message);
        }
    }

    /// <summary>
    /// A class used to store data strings for other mods to recognize and process
    /// </summary>
    public class DataMessage : ExtEnum<string>
    {
        public DataMessage(string value, bool register = false) : base(value, register)
        {
        }
    }
}
