using ExpeditionRegionSupport.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Regions.Data
{
    public class RegionDataMinerIterator : ReadLinesIterator
    {
        private TextStream _stream;

        /// <summary>
        /// The collection of section headers to look for during enumeration
        /// </summary>
        private List<string> _sectionsWanted;

        private bool enumerateAllSections;

        public SectionEventHandler OnSectionStart;
        public SectionEventHandler OnSectionEnd;

        private bool isDisposed;

        public RegionDataMinerIterator(TextStream stream, params string[] regionSections) : base(stream)
        {
            _stream = stream;
            _stream.ReadIterator = this;

            enumerateAllSections = Array.Exists(regionSections, section => section.Equals("ANY"));

            //Either we are looking for a specific set of sections, or every identifiable section
            _sectionsWanted = new List<string>(enumerateAllSections ? RegionDataMiner.WORLD_FILE_SECTIONS : regionSections);
        }

        public override IEnumerable<string> GetEnumerable()
        {
            if (_stream == null) yield break; //Nothing to process
            try
            {
                string line;
                string activeSection = null;
                bool skipThisSection = false;
                do
                {
                    line = _stream.ReadLine();

                    if (line == null) //End of file has been reached
                        yield break;

                    if (line.StartsWith("//") || line == string.Empty) //Empty or commented out lines
                        continue;

                    //All section lines are handled within this code block
                    if (activeSection != null)
                    {
                        if (line.StartsWith("END ")) //Sections must end with an end statement for file to process correctly
                        {
                            OnSectionEnd?.Invoke(activeSection, !skipThisSection);
                            //Prepare local variables values for searching for the next section
                            activeSection = null;
                            skipThisSection = false;

                            if (_sectionsWanted.Count == 0)
                            {
                                _stream.OnStreamEnd?.Invoke(_stream);
                                Plugin.Logger.LogInfo("Process finished");
                                yield break;
                            }
                            continue;
                        }

                        if (skipThisSection) continue;

                        yield return line;
                    }
                    else
                    {
                        activeSection = getSectionHeader(line, out bool isSectionWanted);

                        /*
                         * When a wanted section header is detected, the section can be removed from the wanted list
                         * Unwanted sections will be skipped completely
                         */
                        if (activeSection != null)
                        {
                            string statusString;
                            if (isSectionWanted)
                            {
                                statusString = "READING";
                                _sectionsWanted.Remove(activeSection);
                            }
                            else
                            {
                                statusString = "SKIPPED";
                                skipThisSection = true;
                            }

                            Plugin.Logger.LogInfo($"Section header '{line}' ({statusString})");
                            OnSectionStart?.Invoke(activeSection, isSectionWanted);
                        }
                        else
                        {
                            Plugin.Logger.LogInfo($"Unknown line detected between sections '{line}'");
                        }
                    }
                }
                while (line != null);
            }
            finally
            {
                _stream.Close();
            }
        }

        /// <summary>
        /// Compares line with list of known section headers, returning first match it finds, or null otherwise
        /// </summary>
        /// <param name="line">The string to check</param>
        /// <param name="isWanted">Whether section header is associated with a wanted section</param>
        private string getSectionHeader(string line, out bool isWanted)
        {
            string header = _sectionsWanted.Find(line.StartsWith);

            isWanted = header != null;
            return header ?? RegionDataMiner.WORLD_FILE_SECTIONS.Find(line.StartsWith);
        }

        #region Dispose Handlers
        protected override void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                base.Dispose(disposing);

                if (disposing)
                {
                    _stream = null;
                    OnSectionStart = null;
                    OnSectionEnd = null;
                }
                isDisposed = true;
            }
        }
        #endregion
    }
}
