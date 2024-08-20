using LogUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExpeditionRegionSupport.Regions.Restrictions
{
    public static class RestrictionProcessor
    {
        public const string HEADER_WORLDSTATE = @"WORLDSTATE";
        public const string HEADER_SLUGCATS = @"SLUGCATS";
        public const string HEADER_ALLOW = @"ALLOW";
        public const string HEADER_NOTALLOW = @"NOTALLOW";
        public const string HEADER_ROOMS = @"ROOMS";
        public const string HEADER_PROGRESSION = @"ProgressionRestriction";

        private static RegionList regionsRestricted;

        private delegate void ProcessDelegate(string data, RegionRestrictions regionRestrictions);

        private static ProcessDelegate[] processField;
        private enum FieldType
        {
            Unknown = -1,
            WorldState = 0,
            Slugcats = 1,
            ProcessRestrictions = 2,
            RoomRestrictions = 3
        }

        private enum Modifier
        {
            None = 0,
            Allow = 1,
            Disallow = 2
        }

        private static FieldType activeField;
        private static Modifier activeModifierField;

        private static RegionRestrictions regionRestrictions = null;

        /// <summary>
        /// Returns the RegionRestrictions object used to store restriction values actively being processed from file
        /// A new RegionRestrictions object is created for each region, or set of regions that share restrictions are processed,
        /// or each room, or set of rooms that share restrictions are processed.
        /// </summary>
        private static RegionRestrictions activeRegionRestrictions => processingRooms ? activeRoomRestrictions.Restrictions : regionRestrictions;

        private static RoomRestrictions activeRoomRestrictions;
        private static List<RoomRestrictions> roomRestrictions = new List<RoomRestrictions>();

        private static bool processingInProgress;

        /// <summary>
        /// A flag that manages, and returns the null status of activeRoomRestrictions
        /// </summary>
        private static bool processingRooms
        {
            get => activeRoomRestrictions != null;
            set
            {
                if (value)
                {
                    if (activeRoomRestrictions == null)
                        activeRoomRestrictions = new RoomRestrictions();
                }
                else
                {
                    activeRoomRestrictions = null;
                }
            }
        }

        public static string LogHeader => processingRooms ? "[ROOMS] " : string.Empty;

        public static RegionList Process()
        {
            if (processField == null)
            {
                //Define field handling logic in an array for easy access
                processField = new ProcessDelegate[4];

                processField[(int)FieldType.WorldState] = parseField_WorldState;
                processField[(int)FieldType.Slugcats] = parseField_Slugcats;
                processField[(int)FieldType.ProcessRestrictions] = parseField_ProgressionRestrictions;
                processField[(int)FieldType.RoomRestrictions] = parseField_RoomRestrictions;
            }

            //Handle any test cases that may be present
            processTestCases();

            string path = AssetManager.ResolveFilePath("restricted-regions.txt");

            processFile(path);

            return regionsRestricted;
        }

        private static void processTestCases()
        {
            string debugFolder = AssetManager.ResolveDirectory("debug");

            if (Directory.Exists(debugFolder))
            {
                IEnumerable<string> testCases = Directory.EnumerateFiles(debugFolder, "test*.txt", SearchOption.TopDirectoryOnly); //Fetch test cases

                if (testCases.Count() > 0)
                {
                    string resultFolder = debugFolder + Path.DirectorySeparatorChar + "results";

                    //Start each test case with fresh results
                    foreach (string file in Directory.GetFiles(resultFolder, "result-*.log"))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch
                        {
                        }
                    }

                    //Create folder for logging
                    Directory.CreateDirectory(resultFolder);

                    Plugin.Logger.LogDebug("LOGGING RESTRICTION TEST CASES");

                    foreach (string testCase in testCases)
                    {
                        LogID testCaseID = null;
                        string testCaseName = Path.GetFileNameWithoutExtension(testCase);

                        Plugin.Logger.LogInfo("Test Case: " + testCaseName);

                        try
                        {
                            processFile(testCase);

                            //Log test case results
                            testCaseID = LogID.CreateTemporaryID(testCaseName.Replace("test", "result"), resultFolder);

                            Plugin.Logger.LogTargets.Add(testCaseID);

                            Plugin.Logger.LogInfo("Case Results: " + testCaseName);
                            LogRestrictions();
                            Plugin.Logger.LogInfo("Case Results: END");
                        }
                        catch (Exception ex)
                        {
                            Plugin.Logger.LogError(ex);
                        }
                        finally
                        {
                            if (testCaseID != null)
                                Plugin.Logger.LogTargets.Remove(testCaseID);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called before processing is started to make sure all required fields are reset back to default values
        /// </summary>
        private static void initializeProcessingValues()
        {
            processingInProgress = true;

            regionsRestricted = new RegionList();

            activeField = FieldType.Unknown;
            activeModifierField = Modifier.None;

            regionRestrictions = null;
            activeRoomRestrictions = null;
        }

        private static void processFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Plugin.Logger.LogInfo("Restriction file not found");
                return;
            }

            initializeProcessingValues();

            Plugin.Logger.LogInfo("Restriction file found");

            try
            {
                string[] regionCodes = null;
                foreach (string text in File.ReadAllLines(filePath))
                {
                    string textLine = text.Trim();

                    Plugin.Logger.LogDebug("Active line " + textLine);
                    try
                    {
                        if (textLine.StartsWith("//")) continue; //Comment line

                        if (textLine.StartsWith("$")) //$ indicates a header line. Region codes go on these lines separated by commas.
                        {
                            processingRooms = false;

                            //Apply restrictions before processing the next region section
                            if (regionRestrictions != null)
                                applyRestrictions(regionCodes, regionRestrictions);

                            regionCodes = Regex.Split(textLine, ",");
                            regionRestrictions = new RegionRestrictions();

                            //New region block has started. Clear field flags.
                            activeField = FieldType.Unknown;
                            activeModifierField = Modifier.None;

                            continue;
                        }

                        FieldType field = checkFieldType(textLine);

                        //When the field is unknown, do not override the last active field
                        //FieldTypes will be unknown when data is stored on new lines instead of in-line
                        if (field != FieldType.Unknown)
                        {
                            //A new room set should be defined whenever the switching from a non-rooms field to a rooms field
                            if (field == FieldType.RoomRestrictions && field != activeField)
                                activeRoomRestrictions = new RoomRestrictions();

                            checkForHeader = true; //Assume inline field on first process
                            activeField = field;
                            activeModifierField = Modifier.None;
                        }
                        else if (processingRooms && RegionUtils.ContainsRoomData(textLine)) //Is this a headerless new row of rooms to process?
                        {
                            activeField = FieldType.RoomRestrictions;
                            activeModifierField = Modifier.None;

                            //We do not need to concern ourselves with what is stored here. The content is already stored in roomRestrictions
                            activeRoomRestrictions = new RoomRestrictions();
                        }

                        if (activeField == FieldType.Unknown) //Unrecognized field, or header has not been established properly
                        {
                            Plugin.Logger.LogWarning("Abnormal data detected: " + textLine);
                            continue;
                        }
                        else if (activeField == FieldType.Slugcats)
                        {
                            //Slugcats can be allowed or disallowed. This logic determines which list we are processing
                            Modifier modifier = checkModifierField(textLine);

                            bool modifierChanged = false;
                            if (modifier == Modifier.None)
                            {
                                if (activeModifierField == Modifier.None)
                                {
                                    activeModifierField = Modifier.Allow; //Allow is the default
                                    modifierChanged = true;
                                }
                            }
                            else if (modifier != activeModifierField)
                            {
                                activeModifierField = modifier;
                                modifierChanged = true;
                            }

                            if (modifierChanged)
                                Plugin.Logger.LogInfo(LogHeader + "MODIFIER: " + activeModifierField.ToString());

                            if (modifier != Modifier.None)
                            {
                                int valueIndex;
                                switch (activeModifierField)
                                {
                                    case Modifier.Allow:
                                        valueIndex = HEADER_ALLOW.Length;
                                        break;
                                    case Modifier.Disallow:
                                        valueIndex = HEADER_NOTALLOW.Length;
                                        break;
                                    default: //Only recognized modifier fields support value processing on the modifier line
                                        continue;
                                }

                                if (textLine.Length <= valueIndex + 1) //Check whether it is only the header
                                    continue;

                                textLine = textLine.Substring(valueIndex).TrimStart(':').TrimStart();
                            }
                        }

                        processField[(int)activeField](textLine, activeRegionRestrictions);
                    }
                    catch (Exception ex)
                    {
                        Plugin.Logger.LogError("Error during restriction processing. Check restricted-regions.txt for correct syntax.");
                        Plugin.Logger.LogError("File: " + Path.GetFileName(filePath));
                        Plugin.Logger.LogError("Last field processed: " + activeField);
                        Plugin.Logger.LogError(ex);
                    }

                    checkForHeader = false; //This flag is only needed for the current line.
                }

                //Post processing logic
                //Make sure that the last data read from file is handled
                if (regionCodes != null)
                {
                    if (processingRooms)
                    {
                        applyRestrictions(regionCodes, activeRoomRestrictions.Restrictions);
                        processingRooms = false;
                    }

                    applyRestrictions(regionCodes, activeRegionRestrictions);
                }

                roomRestrictions.Clear();
            }
            catch (Exception ex)
            {
                processingInProgress = false;
                Plugin.Logger.LogError(ex);
            }
        }

        /// <summary>
        /// Processes the restrictions that were processed from file
        /// </summary>
        private static void applyRestrictions(string[] regionCodes, RegionRestrictions regionRestrictions)
        {
            //All region codes share the same restrictions, but room restrictions may be different
            foreach (string text in regionCodes)
            {
                try
                {
                    //Supports: $XX $XX $XX and $XX XX XX
                    string regionCode = parseRegionHeader(text.Trim()); //Get rid of $ symbol and any whitespace

                    Plugin.Logger.LogInfo("Processing restrictions for region " + regionCode);

                    //Fetch RoomRestrictions that apply to this region code
                    regionRestrictions.RoomRestrictions = getRoomRestrictions(regionCode, regionRestrictions);

                    if (regionRestrictions.RoomRestrictions.Count > 0)
                    {
                        Plugin.Logger.LogInfo("Rooms detected");

                        int count = 0;
                        foreach (var res in regionRestrictions.RoomRestrictions)
                        {
                            Plugin.Logger.LogInfo($"[{count}]");
                            foreach (string room in res.Rooms)
                                Plugin.Logger.LogInfo(room);
                            count++;
                        }
                    }
                    else
                    {
                        Plugin.Logger.LogInfo("No room restrictions detected");
                    }

                    if (!regionRestrictions.HasEntries)
                    {
                        Plugin.Logger.LogInfo("No entries detected");
                        continue; //These isn't a point in processing something without entries
                    }

                    Plugin.Logger.LogDebug("Rooms inherit from me: " + regionRestrictions.IsRoomSpecific);

                    //Fetch existing RegionRestrictions if one exists
                    //Merge recently processed restrictions with it if it does, create a new one otherwise
                    if (regionsRestricted.TryFind(regionCode, out RegionKey found))
                    {
                        Plugin.Logger.LogInfo("Existing restrictions found");
                        found.Restrictions.MergeValues(regionRestrictions);
                    }
                    else
                    {
                        Plugin.Logger.LogInfo("Existing restrictions not found. Creating restrictions");

                        //We need to sanity check internal rooms for duplicate, and inconsistent data
                        if (regionRestrictions.RoomRestrictions.Count > 0)
                        {
                            Plugin.Logger.LogInfo("More than one restrictions set detected. Checking for consistency");
                            regionRestrictions.ProcessDuplicateRestrictions();
                        }

                        regionsRestricted.Add(regionCode);
                        regionsRestricted.RegionCache.Restrictions.InheritValues(regionRestrictions);

                    }
                }
                catch (Exception ex)
                {
                    Plugin.Logger.LogError("Error during region code processing");
                    Plugin.Logger.LogError(ex);
                }
            }

            try
            {
                //Handle unprocessed room restrictions that could not be associated with any region code (no region header was provided)
                if (roomRestrictions.Count > 0)
                {
                    //Save unprocessed restrictions from officially being added to regionsRestricted until all region headers are processed 
                    if (processingInProgress)
                    {
                        //The region restrictions source is going to change. Unprocessed restrictions can no longer inherit restrictions
                        roomRestrictions.ForEach(r => r.ReplaceInheritence(regionRestrictions));
                    }
                    else
                    {
                        Plugin.Logger.LogInfo("Handling post processing room restrictions");

                        RegionKey currentRegion = default;
                        RoomRestrictions existingRestrictions = null;
                        foreach (RoomRestrictions current in roomRestrictions)
                        {
                            foreach (string room in current.Rooms)
                            {
                                //Retrieve new, or existing RegionKey
                                if (currentRegion.IsEmpty || !room.StartsWith(currentRegion.RegionCode))
                                {
                                    RegionUtils.ParseRoomName(room, out string regionCode, out _);

                                    if (!regionsRestricted.Contains(regionCode))
                                        regionsRestricted.Add(regionCode);

                                    currentRegion = regionsRestricted.RegionCache;
                                    existingRestrictions = currentRegion.Restrictions.FindRestrictionMatch(current.Restrictions);

                                    //Create a new RoomRestrictions object if one doesn't exist
                                    if (existingRestrictions == null)
                                        existingRestrictions = new RoomRestrictions(regionCode);
                                }

                                existingRestrictions.AddRoom(room);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError("Error during room restrictions cleanup");
                Plugin.Logger.LogError(ex);
            }
        }

        /// <summary>
        /// Gets all of the RoomRestrictions associated with the given region code
        /// </summary>
        private static List<RoomRestrictions> getRoomRestrictions(string regionCode, RegionRestrictions source)
        {
            if (roomRestrictions.Count == 0) return new List<RoomRestrictions>();

            try
            {
                Plugin.Logger.LogInfo("Processing room restrictions");
                Plugin.Logger.LogInfo($"Found {roomRestrictions.Count} sets to process");

                List<RoomRestrictions> restrictions = new List<RoomRestrictions>();
                List<RoomRestrictions> restrictionsToRemove = new List<RoomRestrictions>(); //A list of restrictions that do not need to be processed anymore 

                int index = -1;
                bool restrictionsAdded = false;
                foreach (RoomRestrictions current in roomRestrictions)
                {
                    //We only need to create new RoomRestrictions when the restriction fields change
                    RoomRestrictions r;

                    if (index < 0 || !current.CompareFields(restrictions[index], source))
                    {
                        r = new RoomRestrictions(regionCode);
                        restrictionsAdded = false;
                    }
                    else
                        r = restrictions[index];

                    int addedCount = 0;
                    List<string> roomsToRemove = new List<string>(); //A list of rooms that do not need to be processed anymore 
                                                                     //Find all rooms associated with a region code, and add them
                    foreach (string room in current.Rooms)
                    {
                        Plugin.Logger.LogInfo("Processing Room: " + room);

                        if (!room.StartsWith(regionCode)) continue; //Not the same region

                        if (r.IsEmpty) //First pass will be empty
                            r.Restrictions.InheritValues(RoomRestrictions.GetRestrictions(current, source));

                        r.AddRoom(room);
                        roomsToRemove.Add(room);
                        addedCount++;
                    }

                    if (addedCount > 0)
                    {
                        Plugin.Logger.LogInfo($"Processed {addedCount} rooms with restrictions");

                        current.Rooms.RemoveAll(roomsToRemove);

                        if (current.IsEmpty)
                            restrictionsToRemove.Add(current);

                        //Add to list non-empty RoomRestrictions
                        if (!restrictionsAdded)
                        {
                            index++;
                            restrictions.Add(r);
                            restrictionsAdded = true;
                        }
                    }
                }

                roomRestrictions.RemoveAll(restrictionsToRemove);
                return restrictions;
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError("Error occured during room processing");
                Plugin.Logger.LogError(ex);

                return new List<RoomRestrictions>();
            }
        }

        private static string parseRegionHeader(string text)
        {
            return text.StartsWith("$") ? text.Substring(1) : text;
        }

        private static bool checkForHeader;

        /// <summary>
        /// Returns the index position of the field specified in data
        /// </summary>
        private static FieldType checkFieldType(string data)
        {
            FieldType field = FieldType.Unknown;

            if (data.StartsWith(HEADER_WORLDSTATE, StringComparison.InvariantCultureIgnoreCase))
            {
                field = FieldType.WorldState;
            }
            else if (data.StartsWith(HEADER_SLUGCATS, StringComparison.InvariantCultureIgnoreCase))
            {
                field = FieldType.Slugcats;
            }
            else if (data.StartsWith(HEADER_ROOMS, StringComparison.InvariantCultureIgnoreCase))
            {
                field = FieldType.RoomRestrictions;
            }
            else if (data.StartsWith(HEADER_PROGRESSION, StringComparison.InvariantCultureIgnoreCase))
            {
                field = FieldType.ProcessRestrictions;
            }

            if (field != FieldType.Unknown)
                Plugin.Logger.LogInfo(LogHeader + "FIELD: " + field.ToString());
            return field;
        }

        private static Modifier checkModifierField(string data)
        {
            Modifier modifierField = Modifier.None;

            if (data.StartsWith(HEADER_ALLOW, StringComparison.InvariantCultureIgnoreCase))
                modifierField = Modifier.Allow;
            else if (data.StartsWith(HEADER_NOTALLOW, StringComparison.InvariantCultureIgnoreCase))
                modifierField = Modifier.Disallow;

            /*
             * We only want the modifier to change when the header changes
             * FIELDNAME
             * ALLOW
             * Val1, Val2, Val3
             * NOTALLOW
             * Val1, Val2, Val3
            */
            return modifierField;
        }

        private static void parseField_WorldState(string data, RegionRestrictions regionRestrictions)
        {
            string[] array = Regex.Split(data, ":");

            if (array.Length > 1)
            {
                array = Regex.Split(array[1], ",");

                bool excludeAllWorldStates = false; //Only allowed through user input
                WorldState pendingWorldStateChanges = WorldState.None;

                foreach (string state in array)
                {
                    WorldState worldStateToInclude = parseWorldState(state.Trim()); //WORLD STATE: Saint

                    int worldStateValue = (int)worldStateToInclude;
                    if (worldStateValue < 0 || worldStateValue > 64)
                    {
                        Plugin.Logger.LogWarning($"Unrecognized WorldState detected [{state.Trim()}]");

                        if (worldStateValue < -1)
                            worldStateToInclude = WorldState.Invalid;

                        if (worldStateToInclude == WorldState.Invalid) //Unrecognized is probably fine, but invalid should be ignored
                            continue;
                    }

                    //The WorldState.None flag does not take priority over any other WorldState flag
                    if (pendingWorldStateChanges != WorldState.None || worldStateToInclude != WorldState.None)
                    {
                        pendingWorldStateChanges |= worldStateToInclude; //Combine the flag with any flags already processed
                        excludeAllWorldStates = false;
                    }
                    else
                        excludeAllWorldStates = true;
                }

                if (excludeAllWorldStates)
                {
                    Plugin.Logger.LogInfo("Restriction will prevent access to this region/room unless overwritten by other restrictions");
                    regionRestrictions.WorldState = WorldState.None;
                }
                else if (pendingWorldStateChanges != WorldState.None) //This will be None if no changes, or only invalid changes were processed
                {
                    if (regionRestrictions.WorldState == WorldState.Any) //Flag must not be set to Any before restricted flags can be applied
                        regionRestrictions.WorldState = WorldState.None;

                    regionRestrictions.WorldState |= pendingWorldStateChanges;
                    Plugin.Logger.LogInfo(regionRestrictions.WorldState);
                }
            }
        }

        private static void parseField_Slugcats(string data, RegionRestrictions regionRestrictions)
        {
            if (checkForHeader) //Remove header information if it exists
            {
                if (data.Length <= HEADER_SLUGCATS.Length + 1) return; //Only contains the header, or header + delimiter

                data = removeFieldHeader(data, HEADER_SLUGCATS.Length);
            }

            //Assign process delegate to either allow or disallow this slugcat
            Action<string> processSlugcat;
            if (activeModifierField == Modifier.Allow)
                processSlugcat = regionRestrictions.Slugcats.Allow;
            else if (activeModifierField == Modifier.Disallow)
                processSlugcat = regionRestrictions.Slugcats.Disallow;
            else
            {
                Plugin.Logger.LogWarning("Unusual modifier detected");
                return;
            }

            parseSlugcatData(data, processSlugcat);
        }

        /// <summary>
        /// How this field works
        /// The processor expects room data to be given on the field header line, or the following line.
        /// If no room data is provided, it will treat the next line it can process as room data that doesn't qualify as a header, or field header.
        /// This will cause unexpected handling of data.
        /// </summary>
        private static void parseField_RoomRestrictions(string data, RegionRestrictions regionRestrictions)
        {
            processingRooms = true;

            if (checkForHeader) //Remove header information if it exists
            {
                if (data.Length <= HEADER_ROOMS.Length + 1) return; //Only contains the header, or header + delimiter

                data = removeFieldHeader(data, HEADER_ROOMS.Length);
            }

            //Rooms may be formatted inline style, one per line, or inlines across multiple new lines
            string[] array = Regex.Split(data, ","); //ROOMS: SU_15, TR_20

            if (array.Length > 0)
            {
                //Only store restrictions for future processing if we know there will be rooms to process.
                if (activeRoomRestrictions.IsEmpty)
                    roomRestrictions.Add(activeRoomRestrictions);

                activeRoomRestrictions.AddRooms(array);
            }
        }

        private static void parseField_ProgressionRestrictions(string data, RegionRestrictions regionRestrictions)
        {
            //Check whether we are processing a ProgressionRestriction flag, or associated data values
            if (data.StartsWith("ProgressionRestriction", StringComparison.InvariantCultureIgnoreCase))
            {
                bool fieldProcessed = false;
                if (data.EndsWith("OnVisit", StringComparison.InvariantCultureIgnoreCase))
                {
                    regionRestrictions.ProgressionRestriction |= ProgressionRequirements.OnVisit;
                    fieldProcessed = true;
                }
                else if (data.EndsWith("OnSlugcatUnlocked", StringComparison.InvariantCultureIgnoreCase))
                {
                    regionRestrictions.ProgressionRestriction |= ProgressionRequirements.OnSlugcatUnlocked;
                    fieldProcessed = true;
                }

                if (!fieldProcessed)
                    Plugin.Logger.LogWarning("Unrecognized ProgressionRestriction detected");
            }
            else if ((regionRestrictions.ProgressionRestriction & ProgressionRequirements.OnSlugcatUnlocked) != 0)
            {
                parseSlugcatData(data, regionRestrictions.Slugcats.SetUnlockRequirement);
            }
        }

        private static WorldState parseWorldState(string data)
        {
            switch (data.ToLower())
            {
                case "vanilla":
                case "standard":
                case "white":
                case "yellow":
                case "red":
                case "monk":
                case "survivor":
                case "hunter":
                    {
                        return WorldState.Vanilla;
                    }
                case "gourmand":
                    {
                        return WorldState.Gourmand;
                    }
                case "rivulet":
                    {
                        return WorldState.Rivulet;
                    }
                case "spearmaster":
                    {
                        return WorldState.SpearMaster;
                    }
                case "artificer":
                    {
                        return WorldState.Artificer;
                    }
                case "saint":
                    {
                        return WorldState.Saint;
                    }
                case "custom":
                    {
                        return WorldState.Other;
                    }
                case "oldworld":
                    {
                        return WorldState.OldWorld;
                    }
                case "msc":
                    {
                        return WorldState.MSC;
                    }
                case "any":
                    {
                        return WorldState.Any;
                    }
                case "none":
                    {
                        return WorldState.None;
                    }
            }

            if (Enum.TryParse(data, true, out WorldState value))
                return value;

            return WorldState.Invalid; //Do not let bad data parse to a normal enum value
        }

        private static void parseSlugcatData(string data, Action<string> slugcatHandler)
        {
            //Are there multiple entries on this line?
            if (data.Contains(','))
            {
                string[] array = Regex.Split(data, ",");

                foreach (string name in array)
                    slugcatHandler.Invoke(name);
            }
            else //Field data is being processed per line
                slugcatHandler.Invoke(data);
        }

        private static void LogRestrictions()
        {
            foreach (RegionKey region in regionsRestricted)
            {
                Plugin.Logger.LogDebug("Region: " + region.RegionCode);

                RegionRestrictions restrictions = region.Restrictions;

                bool roomSpecific = restrictions.IsRoomSpecific;
                bool hasRoomEntries = restrictions.HasRoomEntries;

                if (restrictions.NoRestrictedFields)
                {
                    if (roomSpecific || !hasRoomEntries)
                    {
                        Plugin.Logger.LogDebug("NO RESTRICTIONS");

                        if (roomSpecific)
                            Plugin.Logger.LogDebug("HAS ROOM INHERITENCE");
                    }
                    else
                        Plugin.Logger.LogDebug("ROOMS ONLY");

                }
                else if (roomSpecific)
                {
                    Plugin.Logger.LogDebug("ROOMS ONLY (WITH INHERITENCE)");
                }
                else if (hasRoomEntries)
                {
                    Plugin.Logger.LogDebug("REGION-RESTRICTED (WITH ROOMS)");
                }
                else
                    Plugin.Logger.LogDebug("REGION ONLY");

                region.LogRestrictions();
            }
        }

        private static string removeFieldHeader(string field, int headerLength)
        {
            return field.Substring(headerLength).Trim(':');
        }
    }
}
