using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ExpeditionRegionSupport.Data.Logging
{
    public class PropertyDataController : MonoBehaviour, DataController
    {
        public static PropertyDataController PropertyManager;

        public string Name => "LogProperties";

        public List<LogProperties> Properties = new List<LogProperties>();

        static PropertyDataController()
        {
            Initialize();
        }

        public List<LogProperties> GetProperties(LogID logID)
        {
            return Properties.FindAll(p => p.Filename == logID.value);
        }

        /// <summary>
        /// Finds the first detected LogProperties instance associated with the given LogID, and relative filepath
        /// </summary>
        /// <param name="logID">The LogID to search for</param>
        /// <param name="relativePathNoFile">The filepath to search for. When set to null, the first LogID match will be returned</param>
        public LogProperties GetProperties(LogID logID, string relativePathNoFile)
        {
            if (relativePathNoFile == null)
                return Properties.Find(p => p.Filename == logID.value);

            return Properties.Find(p => p.Filename == logID.value && p.HasPath(relativePathNoFile));
        }

        public LogProperties SetProperties(LogID logID, string relativePathNoFile)
        {
            LogProperties properties = new LogProperties(logID, relativePathNoFile);

            Properties.Add(properties);
            return properties;
        }

        public bool TryGetData<T>(string dataAccessString, out T dataValue)
        {
            try
            {
                dataValue = GetData<T>(dataAccessString);
                return true;
            }
            catch
            {
                dataValue = default;
            }
            return false;
        }

        public T GetData<T>(string dataAccessString)
        {
            string[] splitData = dataAccessString.Split(','); //Expected format: <Log Identifier>,<Data Identifier>

            if (splitData.Length < 2)
                throw new FormatException("Data string is in an unexpected format");

            string logName = splitData[0];
            string dataName = splitData[1];

            LogProperties properties = Properties.Find(p => p.LogID.value == logName || p.Aliases.Contains(logName));

            //Search for the requested property field and store it into a temporary object
            //Note: This will box value types, but LogProperties currently doesn't have any of those
            object dataObject = null;
            switch (dataName)
            {
                case nameof(properties.Filename):
                    dataObject = properties.Filename;
                    break;
                case nameof(properties.AltFilename):
                    dataObject = properties.AltFilename;
                    break;
                case nameof(properties.Aliases):
                    dataObject = properties.Aliases;
                    break;
            }

            //Convert the object to the specified type
            return (T)Convert.ChangeType(dataObject, typeof(T));
        }

        public void SendData<T>(string dataAccessString, T dataValue)
        {
            string[] splitData = dataAccessString.Split(','); //Expected format: <Log Identifier>,<Data Identifier>

            if (splitData.Length < 2)
                throw new FormatException("Data string is in an unexpected format");

            string logName = splitData[0];
            string dataName = splitData[1];

            LogProperties properties = Properties.Find(p => p.LogID.value == logName || p.Aliases.Contains(logName));

            //Store value in the specified field location
            switch (dataName)
            {
                case nameof(properties.Filename):
                    properties.Filename = Convert.ToString(dataValue);
                    break;
                case nameof(properties.AltFilename):
                    properties.AltFilename = Convert.ToString(dataValue);
                    break;
                case nameof(properties.Aliases):
                    properties.Aliases = (List<string>)Convert.ChangeType(dataValue, typeof(List<string>));
                    break;
            }
        }

        public void ReadFromFile()
        {
            string propertiesFile = AssetManager.ResolveFilePath("logs.txt");

            if (File.Exists(propertiesFile))
            {
                string[] propertyStrings = File.ReadAllLines(propertiesFile);

                //Read all lines and serialize them into LogProperties
                for (int i = 0; i < propertyStrings.Count(); i++)
                {
                    string propertyString = propertyStrings[i];

                    if (string.IsNullOrWhiteSpace(propertyString) || propertyString.StartsWith("//")) continue;

                    try
                    {
                        LogProperties properties = LogProperties.Deserialize(propertyString);

                        if (properties != null)
                            Properties.Add(properties);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("An error occured while processing log property line " + i);
                        Debug.LogError(ex);
                    }
                }
            }
        }

        public static void Initialize()
        {
            PropertyManager = GetOrCreate(out bool wasCreated);

            if (wasCreated)
                PropertyManager.ReadFromFile();
        }

        /// <summary>
        /// Searches for a PropertyDataController component, and creates one if it does not exist
        /// </summary>
        public static PropertyDataController GetOrCreate(out bool didCreate)
        {
            GameObject managerObject = BepInEx.Bootstrap.Chainloader.ManagerObject;

            didCreate = false;
            if (managerObject != null)
            {
                //TODO: This might create an instance
                var propertyController = managerObject.GetComponent<PropertyDataController>();

                if (propertyController != null)
                    return propertyController;

                didCreate = true;
                return managerObject.AddComponent<PropertyDataController>();
            }
            return null;
        }

        public static string FormatAccessString(string logName, string propertyName)
        {
            return logName + ',' + propertyName;
        }
    }

    public interface DataController
    {
        /// <summary>
        /// This is used to differentiate between multiple DataController instances used for different purposes
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a value associated with a specific key. Throws exception if not found
        /// </summary>
        T GetData<T>(string dataKey);

        /// <summary>
        /// Gets a value associated with a specific key. Returns false if not found
        /// </summary>
        bool TryGetData<T>(string dataKey, out T dataValue);

        /// <summary>
        /// Give data to be stored by, or handled by the DataController
        /// </summary>
        void SendData<T>(string dataKey, T dataValue);

        /// <summary>
        /// Use for handling data from file
        /// </summary>
        void ReadFromFile();
    }
}
