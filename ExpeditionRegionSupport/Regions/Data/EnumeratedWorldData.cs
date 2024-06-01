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
        /// Advance CurrentRange to the next available section range if one exists
        /// </summary>
        public bool AdvanceRange()
        {
            //Search for dictionary entries with ranges that are ahead of the current range, and pick the earliest range
            Range? earliestRange = null;
            foreach (Range range in SectionMap.Values.Where(range => range.CompareTo(CurrentRange) > 0))
            {
                if (earliestRange == null || range.Start < earliestRange?.Start)
                    earliestRange = range;
            }

            //Update the current range
            if (earliestRange != null)
            {
                CurrentRange = earliestRange.Value;
                return true;
            }
            return false;
        }


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
                SectionMap[section] = new Range(-1, -1);
        }

        public override IEnumerator<string> GetEnumerator()
        {
            return dataEnumerator = base.GetEnumerator();
        }

        /// <summary>
        /// Gets room data from the enumerated list. If room data was not collected, list will be empty.
        /// </summary>
        public List<string> GetRoomData()
        {
            Range sectionRange = GetSectionRange(RegionDataMiner.SECTION_ROOMS);

            if (sectionRange.End != -1)
            {
                return GetSectionLines(sectionRange);
            }
            else if (InnerEnumerable != null) //Check that there is still data to process, the section may not be read yet
            {
                if (sectionRange.Start != -1) //Indicates that section has not been read fully, read the rest of the data
                {
                    ReadUntilSectionEnds();

                    sectionRange = GetSectionRange(RegionDataMiner.SECTION_ROOMS); //Get the range a second time

                    if (sectionRange.End != -1)
                        return GetSectionLines(sectionRange);
                }
                else //TODO: Read logic
                {

                }
            }

            return new List<string>();
        }

        public List<string> ReadNextSection()
        {
            return ReadUntilSectionEnds();
        }

        public List<string> ReadUntilSectionEnds()
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
                SectionMap[sectionName] = new Range(EnumeratedValues.Count, -1); //Store the index start for the section
        }

        private void onSectionEnd(string sectionName, bool isSectionWanted)
        {
            if (isSectionWanted)
            {
                Range currentRange = SectionMap[sectionName];
                Range newRange = new Range(currentRange.Start, EnumeratedValues.Count);

                if (Plugin.DebugMode)
                    Plugin.Logger.LogInfo($"Section '{sectionName}' has {newRange.ValueRange} entries");

                if (newRange.ValueRange <= 0) //Section indexes are not in an expected state
                    newRange = Range.NegativeOne;

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
