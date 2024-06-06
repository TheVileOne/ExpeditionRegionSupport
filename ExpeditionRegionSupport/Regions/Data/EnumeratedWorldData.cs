using ExpeditionRegionSupport.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Regions.Data
{
    public class EnumeratedWorldData : CachedEnumerable<string>, IHaveEnumeratedSections
    {
        /// <summary>
        /// The region of which the world data belongs
        /// </summary>
        public string RegionCode { get; }

        private List<GateInfo> _gates;

        /// <summary>
        /// Returns processed gate information for the region (Does not yet store karma requirements)
        /// This will return data based on sections retrieved by the data miner. Conditional links and rooms sections are required
        /// </summary>
        public List<GateInfo> Gates
        {
            get
            {
                if (_gates == null)
                    _gates = GetGateData();
                return _gates;
            }
        }

        private List<ShelterInfo> _shelters;

        /// <summary>
        /// Returns processed shelter information for the region
        /// This will return data based on sections retrieved by the data miner. Rooms section is required
        /// </summary>
        public List<ShelterInfo> Shelters
        {
            get
            {
                if (_shelters == null)
                    _shelters = GetShelterData();
                return _shelters;
            }
        }

        /// <summary>
        /// Associates a section with its index position in the enumerated list
        /// </summary>
        public Dictionary<string, Range> SectionMap { get; }

        /// <summary>
        /// The last accessed start and end point in the EnumeratedValues list
        /// </summary>
        public Range CurrentRange { get; private set; }

        /// <summary>
        /// The range that follows immediately after CurrentRange. Null if it doesn't exist
        /// </summary>
        public Range? NextRange
        {
            get
            {
                //Search for dictionary entries with ranges that are ahead of the current range, and pick the earliest range
                Range? earliestRange = null;
                foreach (Range range in SectionMap.Values.Where(range => range.CompareTo(CurrentRange) > 0))
                {
                    if (earliestRange == null || range.Start < earliestRange?.Start)
                        earliestRange = range;
                }
                return earliestRange;
            }
        }

        /// <summary>
        /// Advance CurrentRange to the next available section range if one exists
        /// </summary>
        public bool AdvanceRange()
        {
            //Search for dictionary entries with ranges that are ahead of the current range, and pick the earliest range
            Range? earliestRange = NextRange;

            //Update the current range
            if (earliestRange != null)
            {
                CurrentRange = earliestRange.Value;
                return true;
            }
            return false;
        }

        public bool EndOfRange => ProcessingComplete && CurrentRange.End == EnumeratedValues.Count - 1;

        /// <summary>
        /// Returns whether or not CurrentRange is in a state that can retrieve data
        /// </summary>
        public bool CanRetrieveSectionLines(Range range) => range.Start != -1 && range.End != -1;

        //These are used for managing the file stream
        private IEnumerator<string> dataEnumerator;
        private RegionDataMiner.ReadLinesIterator readLinesIterator;

        public EnumeratedWorldData(RegionDataMiner.ReadLinesIterator iterator, string regionCode) : base(iterator.GetEnumerable())
        {
            CurrentRange = Range.NegativeOne;
            RegionCode = regionCode;

            readLinesIterator = iterator;
            readLinesIterator.OnSectionStart += onSectionStart;
            readLinesIterator.OnSectionEnd += onSectionEnd;

            SectionMap = new Dictionary<string, Range>();
            InitializeSections();

            dataEnumerator = base.GetEnumerator();
        }

        protected void InitializeSections()
        {
            //Initiate a section for each major world file section
            foreach (string section in RegionDataMiner.WORLD_FILE_SECTIONS)
                SectionMap[section] = Range.NegativeOne;
        }

        public override IEnumerator<string> GetEnumerator()
        {
            return dataEnumerator;
        }

        /// <summary>
        /// Gets conditional link data from the enumerated list. If conditional link data was not collected, list will be empty.
        /// </summary>
        public List<string> GetConditionalLinkData()
        {
            //TODO: Process inlined CRS-styled conditional links
            return GetSectionData(RegionDataMiner.SECTION_CONDITIONAL_LINKS);
        }

        /// <summary>
        /// Gets room data from the enumerated list. If room data was not collected, list will be empty.
        /// </summary>
        public List<string> GetRoomData()
        {
            return GetSectionData(RegionDataMiner.SECTION_ROOMS);
        }

        /// <summary>
        /// Gets creature data from the enumerated list. If creature data was not collected, list will be empty.
        /// </summary>
        public List<string> GetCreatureData()
        {
            return GetSectionData(RegionDataMiner.SECTION_CREATURES);
        }

        /// <summary>
        /// Gets bat migration blockage data from the enumerated list. If bat migration blockage data was not collected, list will be empty.
        /// </summary>
        public List<string> GetBatMigrationBlockageData()
        {
            return GetSectionData(RegionDataMiner.SECTION_BAT_MIGRATION_BLOCKAGES);
        }

        /// <summary>
        /// Gets section data from the enumerated list. If the section was not collected, list will be empty.
        /// </summary>
        public List<string> GetSectionData(string sectionName)
        {
            try
            {
                Range sectionRange = GetSectionRange(sectionName);

                //Plugin.Logger.LogInfo("Searching for section " + sectionName);
                //Plugin.Logger.LogInfo("Range " + sectionRange.ToString());

                if (CanRetrieveSectionLines(sectionRange))
                {
                    return GetSectionLines(sectionRange);
                }
                else if (!ProcessingComplete) //Check that there is still data to process, the section may not be read yet
                {
                    //Plugin.Logger.LogInfo("Data to process");

                    if (sectionRange.Start != -1) //Indicates that section has not been read fully, read the rest of the data
                    {
                        ReadUntilSectionEnds();

                        sectionRange = GetSectionRange(sectionName); //Get the range a second time
                        return GetSectionLines(sectionRange);
                    }
                    else
                    {
                        readLinesIterator.OnSectionEnd += onSectionEndEvent;
                        bool sectionFound = false;
                        while (!sectionFound && dataEnumerator.MoveNext()) { } //Read lines until we find the section, or run out of lines

                        //Plugin.Logger.LogInfo("Data processing finished");
                        //Plugin.Logger.LogInfo("Section found " + sectionFound);

                        readLinesIterator.OnSectionEnd -= onSectionEndEvent;

                        if (sectionFound)
                            return GetSectionLines(sectionRange);

                        void onSectionEndEvent(string sectionRead, bool isSectionWanted)
                        {
                            sectionFound = sectionRead == sectionName;

                            //The section range is set here, because CurrentRange will store the range for the next section after this event
                            if (sectionFound)
                                sectionRange = CurrentRange;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError("There was an error while reading world data");
                Plugin.Logger.LogError(ex);
                Plugin.Logger.LogError(ex.StackTrace);
            }

            //Plugin.Logger.LogInfo("Data set empty");
            return new List<string>();
        }

        public List<string> ReadNextSection()
        {
            if (EndOfRange) return new List<string>(); //Nothing left to return

            if (ProcessingComplete)
            {
                //No more values to process, but we're not at the end of the range
                if (AdvanceRange() && CanRetrieveSectionLines(CurrentRange))
                    return GetSectionLines(CurrentRange);
                return new List<string>();
            }

            if (CurrentRange.End == -1)
            {
                /*
                // Failed attempt to handle incomplete processed sections
                if (CurrentRange != Range.NegativeOne)
                {
                    //Last section has not been fully processed. Process it first, and then process the next section fully
                    ReadUntilSectionEnds();
                }
                */

                //Retrieve the section we want to access
                //This code gives priority to unread data over the unlikely event that the enumeration has been reset back to the
                //first section
                return ReadUntilSectionEnds();
            }

            /*
             * CurrentRange is guaranteed to be positive, but it may be positioned earlier than expected. Advance the range, and check
             * that the new range is still valid. If it is, retrieve the data, and if it is not, we need to read the rest of the section
             * to complete it.
             */
            if (AdvanceRange())
            {
                if (CanRetrieveSectionLines(CurrentRange))
                    return GetSectionLines(CurrentRange);

                List<string> sectionLines = new List<string>();
                if (CurrentRange.Start < EnumeratedValues.Count)
                {
                    //Get section lines that have already been processed
                    sectionLines = GetSectionLines(new Range(CurrentRange.Start, EnumeratedValues.Count - 1));
                }

                //The remaining lines will be added here
                sectionLines.AddRange(ReadUntilSectionEnds());
                return sectionLines;
            }

            return new List<string>();
        }

        internal List<string> ReadUntilSectionEnds()
        {
            readLinesIterator.OnSectionEnd += onSectionEndEvent;

            List<string> sectionLines = new List<string>();
            bool sectionEnded = false;
            do
            {
                if (!dataEnumerator.MoveNext())
                    sectionEnded = true;

                if (!sectionEnded)
                    sectionLines.Add(dataEnumerator.Current); //Add lines to list until end of section
            }
            while (!sectionEnded);

            readLinesIterator.OnSectionEnd -= onSectionEndEvent;
            return sectionLines;

            void onSectionEndEvent(string sectionName, bool isSectionWanted)
            {
                sectionEnded = true;
            }
        }

        /// <summary>
        /// Allows enumeration to begin at the start of the enumerated values cache
        /// </summary>
        public void Reset()
        {
            CurrentRange = Range.NegativeOne;
        }

        public Range GetSectionRange(string sectionName)
        {
            try
            {
                return SectionMap[sectionName];
            }
            catch (KeyNotFoundException)
            {
                return Range.NegativeOne;
            }
        }

        private void onSectionStart(string sectionName, bool isSectionWanted)
        {
            if (isSectionWanted)
            {
                CurrentRange = new Range(EnumeratedValues.Count, -1);
                SectionMap[sectionName] = CurrentRange; //Store the index start for the section
            }
        }

        private void onSectionEnd(string sectionName, bool isSectionWanted)
        {
            if (isSectionWanted)
            {
                Range currentRange = SectionMap[sectionName];
                Range newRange = new Range(currentRange.Start, EnumeratedValues.Count - 1);

                //if (Plugin.DebugMode)
                //    Plugin.Logger.LogInfo($"Section '{sectionName}' has {newRange.ValueRangeInclusive} entries");

                if (newRange.ValueRange <= 0) //Section indexes are not in an expected state
                    newRange = Range.NegativeOne;

                CurrentRange = newRange;
                SectionMap[sectionName] = newRange;
            }
        }

        /// <summary>
        /// A tracker for failed data access attempts
        /// </summary>
        private int dataAccessAttempts = 0;

        /// <summary>
        /// Returns the section lines between the given range (indexes inclusive)
        /// </summary>
        internal List<string> GetSectionLines(Range range)
        {
            bool errorHandled = false;
            int sectionLineCount = range.ValueRangeInclusive;

            if (range == Range.NegativeOne)
                sectionLineCount = 0;

            try
            {
                //Plugin.Logger.LogInfo("Getting section lines at range " + range.ToString());

                if (sectionLineCount > 0)
                    return EnumeratedValues.GetRange(range.Start, sectionLineCount);

                return new List<string>();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                if (dataAccessAttempts > 0 || Plugin.DebugMode)
                    throw ex;

                errorHandled = true;
                Plugin.Logger.LogError("Invalid section range handled. Please report this issue");
                return GetSectionLines(new Range(range.Start, Math.Min(range.End, EnumeratedValues.Count - 1)));
            }
            finally
            {
                if (errorHandled)
                    dataAccessAttempts++;
                else
                    dataAccessAttempts = 0;
            }
        }

        public List<GateInfo> GetGateData()
        {
            List<string> conditionalLinkData = GetConditionalLinkData();
            List<string> roomData = GetRoomData();

            List<GateInfo> gates = new List<GateInfo>();

            foreach (string roomLine in roomData)
            {
                string gateCode = RegionUtils.GetGateCodeWithValidation(roomLine);

                if (gateCode != null)
                {
                    GateInfo gate = new GateInfo(gateCode);

                    //Handle conditional link information
                    foreach (string conditionalLink in conditionalLinkData.Where(r => r.Contains("EXCLUSIVEROOM") && r.TrimEnd().EndsWith(gate.RoomCode)))
                        gate.ConditionalAccess.Add(SlugcatUtils.GetOrCreate(conditionalLink.Substring(0, conditionalLink.IndexOf(':')))); //The first section is the slugcat

                    /*
                    if (gate.ConditionalAccess.Count > 0)
                    {
                        Plugin.Logger.LogInfo("CONDITIONAL INFO");
                        foreach (SlugcatStats.Name slugcat in gate.ConditionalAccess)
                            Plugin.Logger.LogInfo(slugcat);
                    }
                    */
                    gates.Add(gate);
                }
            }
            return gates;
        }

        public List<ShelterInfo> GetShelterData()
        {
            List<string> roomData = GetRoomData();

            List<ShelterInfo> shelters = new List<ShelterInfo>();

            //TODO: Need to detect non-broken conditional shelters
            foreach (string roomLine in roomData)
            {
                string shelterCode = RegionUtils.GetShelterCodeWithValidation(roomLine);

                if (shelterCode != null)
                    shelters.Add(new ShelterInfo(shelterCode));
            }

            string propertiesFile = RegionUtils.GetFilePath(RegionCode, "properties.txt");

            //Look for broken shelter info
            if (shelters.Count > 0 && File.Exists(propertiesFile))
            {
                List<string> brokenShelterData = new List<string>();
                using (TextReader stream = new StreamReader(propertiesFile))
                {
                    string line;
                    while ((line = stream.ReadLine()) != null)
                    {
                        if (line.StartsWith("Broken Shelters"))
                        {
                            int sepIndex = line.IndexOf(':');

                            if (sepIndex != -1 && sepIndex != line.Length - 1) //Format is okay, and there is data on this line
                                brokenShelterData.Add(line.Substring(sepIndex + 1)); //Whole line is stored, will be processed later
                        }
                    }
                }

                if (brokenShelterData.Count > 0)
                {
                    Plugin.Logger.LogInfo("Broken shelter data found for region " + RegionCode);

                    ShelterInfo lastShelterProcessed = default;
                    foreach (string shelterDataRaw in brokenShelterData)
                    {
                        string[] shelterData = shelterDataRaw.Split(':'); //Expects " White: SL_S11" (whitespace is expected)

                        if (shelterData.Length >= 2) //Expected length - Something is unusual is there if it is anything else
                        {
                            string[] roomCodes = shelterData[1].Split(',');

                            for (int i = 0; i < roomCodes.Length; i++)
                            {
                                string roomCode = roomCodes[i].Trim();

                                bool isNewShelter = lastShelterProcessed.RoomCode != roomCode;

                                //It is common to have the same shelter being targeted across multiple lines
                                ShelterInfo shelter = isNewShelter ?
                                    shelters.Find(s => string.Equals(s.RoomCode, roomCode, StringComparison.InvariantCultureIgnoreCase))
                                  : lastShelterProcessed;

                                if (shelter.RoomCode == roomCode) //ShelterInfo is a struct, checking for this lets us know if we found a match
                                {
                                    lastShelterProcessed = shelter;

                                    string[] slugcats = shelterData[0].Split(',');

                                    foreach (string slugcat in slugcats)
                                    {
                                        //Slugcat may not be available if this fails, which should be fine.
                                        if (SlugcatUtils.TryParse(slugcat, out SlugcatStats.Name found))
                                            shelter.BrokenForTheseSlugcats.Add(found);
                                    }

                                    //This shelter is likely registered as broken, but unsure how the game handles it without slugcat info
                                    if (shelter.BrokenForTheseSlugcats.Count == 0)
                                        Plugin.Logger.LogInfo($"Line 'Broken Shelters: {shelterDataRaw}' has no recognizable slugcat info");
                                }
                                else //Stray property line doesn't match any shelter data processed
                                {
                                    Plugin.Logger.LogInfo("Broken shelter references a room that cannot be found");
                                    Plugin.Logger.LogInfo($"Shelter room [{shelter.RoomCode}]");
                                    Plugin.Logger.LogInfo($"Room [{roomCode}]");
                                }
                            }
                        }
                        else
                        {
                            Plugin.Logger.LogInfo($"Line 'Broken Shelters: {shelterDataRaw}' is invalid");
                        }
                    }
                }
            }

            return shelters;
        }
    }
}
