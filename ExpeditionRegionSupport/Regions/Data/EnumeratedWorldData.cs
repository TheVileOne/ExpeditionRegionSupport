using ExpeditionRegionSupport.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Regions.Data
{
    public class EnumeratedWorldData : CachedEnumerable<string>, IHaveEnumeratedSections
    {
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
        private RegionDataMiner.ReadLinesIterator _ReadLinesIterator;

        public EnumeratedWorldData(RegionDataMiner.ReadLinesIterator iterator) : base(iterator.GetEnumerable())
        {
            CurrentRange = Range.NegativeOne;

            _ReadLinesIterator = iterator;
            _ReadLinesIterator.OnSectionStart += onSectionStart;
            _ReadLinesIterator.OnSectionEnd += onSectionEnd;

            SectionMap = new Dictionary<string, Range>();
            InitializeSections();
        }

        protected void InitializeSections()
        {
            //Initiate a section for each major world file section
            foreach (string section in RegionDataMiner.WORLD_FILE_SECTIONS)
                SectionMap[section] = Range.NegativeOne;
        }

        public override IEnumerator<string> GetEnumerator()
        {
            return dataEnumerator = base.GetEnumerator();
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
            Range sectionRange = GetSectionRange(sectionName);

            if (CanRetrieveSectionLines(sectionRange))
            {
                return GetSectionLines(sectionRange);
            }
            else if (!ProcessingComplete) //Check that there is still data to process, the section may not be read yet
            {
                if (sectionRange.Start != -1) //Indicates that section has not been read fully, read the rest of the data
                {
                    ReadUntilSectionEnds();

                    sectionRange = GetSectionRange(sectionName); //Get the range a second time
                    return GetSectionLines(sectionRange);
                }
                else
                {
                    _ReadLinesIterator.OnSectionEnd += onSectionEndEvent;
                    bool sectionFound = false;
                    while (!sectionFound && dataEnumerator.MoveNext()) { } //Read lines until we find the section, or run out of lines

                    _ReadLinesIterator.OnSectionEnd -= onSectionEndEvent;

                    if (sectionFound)
                    {
                        sectionRange = CurrentRange;
                        return GetSectionLines(sectionRange);
                    }

                    void onSectionEndEvent(string sectionName, bool isSectionWanted)
                    {
                        sectionFound = true;
                    }
                }
            }

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
                if (CurrentRange != Range.NegativeOne)
                {
                    //Last section has not been fully processed. Process it first, and then process the next section fully
                    ReadUntilSectionEnds();
                }

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
            _ReadLinesIterator.OnSectionEnd += onSectionEndEvent;

            List<string> sectionLines = new List<string>();
            bool sectionEnded = false;
            while (!sectionEnded)
            {
                if (dataEnumerator.MoveNext())
                    sectionLines.Add(dataEnumerator.Current); //Add lines to list until end of section
                else
                    sectionEnded = true;
            }

            _ReadLinesIterator.OnSectionEnd -= onSectionEndEvent;
            return sectionLines;

            void onSectionEndEvent(string sectionName, bool isSectionWanted)
            {
                sectionEnded = true;
            }
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

                if (Plugin.DebugMode)
                    Plugin.Logger.LogInfo($"Section '{sectionName}' has {newRange.ValueRange} entries");

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
            int sectionLineCount = range.ValueRange;

            try
            {
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
    }
}
