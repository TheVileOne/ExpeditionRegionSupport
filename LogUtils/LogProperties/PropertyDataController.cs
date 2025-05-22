using LogUtils.Enums;
using LogUtils.Helpers;
using LogUtils.Helpers.FileHandling;
using LogUtils.Properties.Custom;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LogUtils.Properties
{
    public class PropertyDataController : UtilityComponent
    {
        private List<LogProperties> _properties = new List<LogProperties>();

        public IEnumerable<LogProperties> Properties => _properties.ToArray();
        public CustomLogPropertyCollection CustomLogProperties = new CustomLogPropertyCollection();
        public Dictionary<LogProperties, LogPropertyStringDictionary> UnrecognizedFields = new Dictionary<LogProperties, LogPropertyStringDictionary>();

        public LogPropertyFile PropertyFile = new LogPropertyFile();

        public override string Tag => UtilityConsts.ComponentTags.PROPERTY_DATA;

        /// <summary>
        /// A flag that indicates that there are duplicate entries in the LogProperties file
        /// </summary>
        public bool HasDuplicateFileEntries;

        /// <summary>
        /// A flag that forces all properties instances to write to file on the next save attempt
        /// </summary>
        public bool ForceWriteAll;

        /// <summary>
        /// The game is within a period of time when ReadOnly can be toggled off for the duration of the period before turning back on at the end of the period
        /// </summary>
        internal bool IsEditGracePeriod;

        /// <summary>
        /// A flag that indicates the log replacement process has started and has yet to finish
        /// </summary>
        internal bool StartupRoutineActive;

        static PropertyDataController()
        {
            UtilityCore.EnsureInitializedState();
        }

        public PropertyDataController()
        {
            IsEditGracePeriod = RWInfo.LatestSetupPeriodReached < SetupPeriod.PostMods;

            CustomLogProperties.OnPropertyAdded += onCustomPropertyAdded;
            CustomLogProperties.OnPropertyRemoved += onCustomPropertyRemoved;
        }

        internal void ProcessLogFiles()
        {
            LogsFolder.UpdatePath();

            bool shouldRunStartupRoutine = RWInfo.LatestSetupPeriodReached < RWInfo.STARTUP_CUTOFF_PERIOD;

            if (shouldRunStartupRoutine)
                StartupRoutineActive = true; //Notify that startup process might be happening early

            UtilityLogger.Log("Creating temporary log files");
            ProcessLateInitializedLogFile(LogID.BepInEx);

            //It is important for normal function of the utility for it to initialize before the game does. The following code handles the situation when
            //the utility is initialized too late, and the game has been allowed to intialize the log files without the necessary utility hooks active
            if (RWInfo.LatestSetupPeriodReached > SetupPeriod.Pregame)
            {
                ProcessLateInitializedLogFile(LogID.Unity);
                ProcessLateInitializedLogFile(LogID.Exception);

                if (RWInfo.LatestSetupPeriodReached >= SetupPeriod.ModsInit) //Expedition, and JollyCoop
                {
                    ProcessLateInitializedLogFile(LogID.Expedition);
                    ProcessLateInitializedLogFile(LogID.JollyCoop);
                }
            }

            if (shouldRunStartupRoutine) //Sanity check in case we are initializing extra late
                BeginStartupRoutine();
        }

        internal void ProcessLateInitializedLogFile(LogID logFile)
        {
            LogProperties properties = logFile.Properties;

            //The original filename is stored without its original file extension in the value field of the LogID
            string originalFilePath = LogFile.FindPathWithoutFileExtension(properties.OriginalFolderPath, logFile.value);

            if (originalFilePath != null) //This shouldn't be null under typical circumstances
            {
                bool moveFileFromOriginalPath = !PathUtils.PathsAreEqual(originalFilePath, properties.CurrentFilePath);
                bool lastKnownFileOverwritten = PathUtils.PathsAreEqual(originalFilePath, properties.LastKnownFilePath);

                if (moveFileFromOriginalPath)
                {
                    //Set up the temp file for this log file if it isn't too late to do so
                    if (!lastKnownFileOverwritten && StartupRoutineActive)
                    {
                        properties.CreateTempFile();
                        properties.SkipStartupRoutine = true;
                    }

                    using (properties.FileLock.Acquire())
                    {
                        properties.FileLock.SetActivity(properties.ID, FileAction.Move);

                        //Move the file, and if it fails, change the path. Either way, log file exists
                        if (LogFile.Move(originalFilePath, properties.CurrentFilePath) == FileStatus.MoveComplete)
                            properties.ChangePath(properties.CurrentFilePath);
                        else
                            properties.ChangePath(originalFilePath);

                        properties.FileExists = true;
                        properties.LogSessionActive = true;
                    }
                }

                properties.SkipStartupRoutine |= lastKnownFileOverwritten;
            }
        }

        internal void BeginStartupRoutine()
        {
            StartupRoutineActive = true;
            foreach (LogProperties properties in Properties)
            {
                if (!properties.SkipStartupRoutine && !properties.LogSessionActive)
                    properties.CreateTempFile();

                //When the Logs folder is available, favor that path over the original path to the log file
                if (properties.LogsFolderAware && properties.LogsFolderEligible)
                    LogsFolder.AddToFolder(properties);
            }
        }

        internal void CompleteStartupRoutine()
        {
            if (!StartupRoutineActive) return;

            //All created temp files are removed on game start
            foreach (LogProperties properties in Properties)
                properties.RemoveTempFile();

            StartupRoutineActive = false;
        }

        private void onCustomPropertyAdded(CustomLogProperty property)
        {
            foreach (LogProperties properties in Properties)
            {
                CustomLogProperty customProperty = property.Clone(); //Create an instance of the custom property for each item in the property list

                //Search for unrecognized fields that match the custom property
                if (UnrecognizedFields.TryGetValue(properties, out LogPropertyStringDictionary fieldDictionary))
                {
                    if (fieldDictionary.ContainsKey(customProperty.Name))
                    {
                        customProperty.Value = fieldDictionary[customProperty.Name]; //Overwrites default value with the value taken from file
                        fieldDictionary.Remove(customProperty.Name); //Field is no longer unrecognized

                        if (fieldDictionary.Count == 0)
                            UnrecognizedFields.Remove(properties); //Remove the dictionary after it is empty
                    }
                }

                //Register the custom property with the associated properties instance
                properties.CustomProperties.AddProperty(customProperty);
            }
        }

        private void onCustomPropertyRemoved(CustomLogProperty property)
        {
            //Remove the custom property reference from each properties instance
            foreach (LogProperties properties in Properties)
                properties.CustomProperties.RemoveProperty(p => p.Name == property.Name);
        }

        public IEnumerable<LogProperties> GetProperties(LogID logID)
        {
            return Properties.Where(p => p.ID.BaseEquals(logID));
        }

        /// <summary>
        /// Finds the first detected LogProperties instance associated with the given LogID, and relative filepath
        /// </summary>
        /// <param name="logID">The LogID to search for</param>
        /// <param name="relativePathNoFile">The filepath to search for. When set to null, any LogID match will be returned with custom root being prioritized</param>
        public LogProperties GetProperties(LogID logID, string relativePathNoFile)
        {
            bool searchForAnyMatch = LogProperties.IsPathWildcard(relativePathNoFile); //This flag prioritizes the custom root over any other match

            LogProperties bestCandidate = null;
            foreach (LogProperties properties in GetProperties(logID))
            {
                if (PathUtils.PathsAreEqual(properties.OriginalFolderPath, LogProperties.GetContainingPath(relativePathNoFile)))
                {
                    bestCandidate = properties;
                    break;
                }

                if (searchForAnyMatch && bestCandidate == null)
                    bestCandidate = properties;
            }
            return bestCandidate;
        }

        public LogProperties SetProperties(LogID logID, string relativePathNoFile)
        {
            LogProperties properties = new LogProperties(logID.value, relativePathNoFile);

            _properties.Add(properties);
            return properties;
        }

        /// <summary>
        /// Reads properties data from file and creates LogProperties instances from the data
        /// </summary>
        public void SetPropertiesFromFile()
        {
            var propertyComparer = Comparer<LogProperties>.Create(compareByIDHash);

            foreach (LogPropertyData data in PropertyFile.Reader.ReadData())
            {
                if (data.FieldOrderMismatch)
                    ForceWriteAll = true;

                data.ProcessFields();
                LogProperties properties = data.Processor.Results;

                if (properties != null)
                {
                    bool propertiesAlreadyExists = Properties.Any(p => propertyComparer.Compare(p, properties) == 0);

                    if (propertiesAlreadyExists)
                    {
                        ForceWriteAll = true;
                        HasDuplicateFileEntries = true;
                        continue;
                    }

                    if (data.UnrecognizedFields.Count > 0)
                        UnrecognizedFields[properties] = data.UnrecognizedFields;

                    properties.UpdateWriteHash();
                    properties.ReadOnly = true;

                    _properties.Add(properties);
                }
            }
        }

        internal void ReloadFromProcessSwitch()
        {
            var propertyComparer = Comparer<LogProperties>.Create(compareByIDHash);

            foreach (LogPropertyData data in PropertyFile.Reader.ReadData())
            {
                data.ProcessFields();
                LogProperties properties = data.Processor.Results;

                if (properties != null)
                {
                    LogProperties existingProperties = Properties.FirstOrDefault(p => propertyComparer.Compare(p, properties) == 0);

                    //When a main process closes, it's state is written to file - in particular the current path for the file gets stored as the last known path.
                    //We need to restore this metadata so that the incoming process knows where to log new messages
                    if (existingProperties != null)
                    {
                        existingProperties.ChangePath(properties.LastKnownFilePath);
                        continue;
                    }

                    properties.ChangePath(properties.LastKnownFilePath);

                    //This log file is unrecognized by this process and must be new
                    if (data.UnrecognizedFields.Count > 0)
                        UnrecognizedFields[properties] = data.UnrecognizedFields;

                    properties.UpdateWriteHash();
                    properties.ReadOnly = true;

                    _properties.Add(properties);
                }
            }
        }

        private static int compareByIDHash(LogProperties properties, LogProperties propertiesOther)
        {
            return properties.IDHash.CompareTo(propertiesOther.IDHash);
        }

        public void SaveToFile()
        {
            if (!PropertyFile.Stream.CanWrite)
            {
                UtilityLogger.LogWarning("Property file data could not be saved");
                return;
            }

            bool success = false;
            try
            {
                PropertyFile.Writer.Write(GetUpdateEntries());
                success = true;
            }
            catch (IOException ex)
            {
                UtilityLogger.LogError(ex);
            }
            finally
            {
                if (success)
                    UtilityLogger.Log("Properties file saved");
                else
                    UtilityLogger.LogWarning("Properties file could not be saved");
            }
        }

        /// <summary>
        /// Returns an array of property instances that have data that needs to be written to file
        /// </summary>
        internal LogProperties[] GetUpdateEntries()
        {
            if (ForceWriteAll || !File.Exists(PropertyFile.FilePath))
                return (LogProperties[])Properties;

            //Reasons to update include new log file data, incomplete data read from file, or modifications made to property data 
            return Properties.Where(p => p.ProcessedWithErrors || p.HasModifiedData()).ToArray();
        }

        public override Dictionary<string, object> GetFields()
        {
            Dictionary<string, object> fields = base.GetFields();

            fields[nameof(Properties)] = Properties;
            fields[nameof(CustomLogProperties)] = CustomLogProperties;
            fields[nameof(UnrecognizedFields)] = UnrecognizedFields;
            return fields;
        }

        internal void NotifyWriteCompleted()
        {
            //After a file update, associated flags need to be set back to default values
            ForceWriteAll = false;
            HasDuplicateFileEntries = false;
        }

        public static string FormatAccessString(string logName, string propertyName)
        {
            return logName + ',' + propertyName;
        }
    }
}
