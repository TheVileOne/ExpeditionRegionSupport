using ExpeditionRegionSupport.Data;
using System;
using System.Collections.Generic;
using System.IO;

namespace ExpeditionRegionSupport.Regions.Data
{
    public class RegionDataMiner
    {
        private TextStream _activeStream;
        protected TextStream ActiveStream
        {
            get => _activeStream;
            private set
            {
                if (_activeStream == value) return;

                //Close old stream - once we lose the reference, the stream wont be disposed
                if (/*!KeepStreamOpen && */_activeStream != null)
                {
                    //if (KeepStreamOpen)
                    //    Plugin.Logger.LogDebug("Stream is being closed that should be kept open");

                    CloseStream();
                }

                _activeStream = value;

                if (_activeStream != null)
                    _activeStream.OnDisposed += onStreamDisposed;
            }
        }

        private void onStreamDisposed(TextStream stream)
        {
            if (_activeStream == stream)
            {
                _activeStream.OnDisposed -= onStreamDisposed;
                _activeStream = null;
            }
        }

        private bool _keepStreamOpen;

        /// <summary>
        /// The stream wont be closed when a read process finished. Stream will be disposed when RegionDataMiner is destroyed.
        /// NOTE: This currently does not work correctly when set to true. Reusing the StreamReader for multiple reads is currently not supported.
        /// </summary>
        public bool KeepStreamOpen
        {
            get => _keepStreamOpen;
            set
            {
                if (ActiveStream != null)
                    ActiveStream.DisposeOnStreamEnd = !value;
                _keepStreamOpen = value;
            }
        }

        public const string SECTION_BAT_MIGRATION_BLOCKAGES = "BAT MIGRATION BLOCKAGES";
        public const string SECTION_CONDITIONAL_LINKS = "CONDITIONAL LINKS";
        public const string SECTION_CREATURES = "CREATURES";
        public const string SECTION_ROOMS = "ROOMS";

        /// <summary>
        /// Each world file will contains these sections by default
        /// </summary>
        public static List<string> WORLD_FILE_SECTIONS = new List<string>()
        {
            SECTION_CONDITIONAL_LINKS,
            SECTION_ROOMS,
            SECTION_CREATURES,
            SECTION_BAT_MIGRATION_BLOCKAGES
        };

        public TextStream GetStreamReader(string regionCode)
        {
            string regionFile = RegionUtils.GetWorldFilePath(regionCode);

            if (!File.Exists(regionFile))
            {
                Plugin.Logger.LogInfo("World file missing");
                Plugin.Logger.LogInfo(regionFile);
                return null;
            }

            try
            {
                //TODO: StreamReaders must be able to be reused, or KeepStreamOpen will not work properly, firing a Sharing violation
                //if another StreamReader is created for the same file
                return new TextStream(regionFile)
                {
                    DisposeOnStreamEnd = !KeepStreamOpen
                };
            }
            catch (IOException)
            {
                Plugin.Logger.LogError("Could not read " + RegionUtils.FormatWorldFile(regionCode));
                return null;
            }
        }

        public CachedEnumerable<string> GetBatMigrationLines(string regionCode)
        {
            return GetLines(regionCode, SECTION_BAT_MIGRATION_BLOCKAGES);
        }

        public CachedEnumerable<string> GetConditionalLinkLines(string regionCode)
        {
            return GetLines(regionCode, SECTION_CONDITIONAL_LINKS);
        }

        public CachedEnumerable<string> GetCreatureLines(string regionCode)
        {
            return GetLines(regionCode, SECTION_CREATURES);
        }

        public CachedEnumerable<string> GetRoomLines(string regionCode)
        {
            return GetLines(regionCode, SECTION_ROOMS);
        }

        internal CachedEnumerable<string> GetLines(string regionCode, string sectionName)
        {
            return new CachedEnumerable<string>(ReadLinesIterator(regionCode, sectionName));
        }

        public void CloseStream()
        {
            KeepStreamOpen = false;

            if (_activeStream != null)
                _activeStream.Close();
        }

        ~RegionDataMiner()
        {
            CloseStream();
        }

        public class ReadLinesIterator
        {
            private TextStream _stream;

            /// <summary>
            /// The collection of section headers to look for during enumeration
            /// </summary>
            private List<string> _sectionsWanted;

            private bool enumerateAllSections;

            public ReadLinesIterator(TextStream stream, params string[] regionSections)
            {
                _stream = stream;

                enumerateAllSections = Array.Exists(regionSections, section => section.Equals("ANY"));

                //Either we are looking for a specific set of sections, or every identifiable section
                _sectionsWanted = new List<string>(enumerateAllSections ? WORLD_FILE_SECTIONS : regionSections);
            }

            public IEnumerable<string> GetEnumerable()
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

                        //All section lines are handled within this code block
                        if (activeSection != null)
                        {
                            if (line.StartsWith("//") || line == string.Empty) //Empty or commented out lines
                                continue;
                            if (line.StartsWith("END ")) //Sections must end with an end statement for file to process correctly
                            {
                                //Prepare local variables values for searching for the next section
                                activeSection = null;
                                skipThisSection = false;

                                if (_sectionsWanted.Count == 0)
                                {
                                    Plugin.Logger.LogInfo("Process finished");
                                    yield break;
                                }
                                continue;
                            }

                            if (skipThisSection) continue;

                            yield return line;
                        }

                        //First check for wanted sections, and then check for unwanted sections
                        activeSection = getSectionHeader(line, true);

                        if (activeSection != null)
                            _sectionsWanted.Remove(activeSection); //Sections are supposed to only appear once per file
                        else
                        {
                            //A null indicates there is line data that is not part of any known section, or the start of a new section.
                            //This could be an indication of a custom section that the reader is unfamiliar with.
                            activeSection = getSectionHeader(line, false);
                            skipThisSection = activeSection != null;
                        }
                    }
                    while (line != null);
                }
                finally
                {
                    _stream.Close();
                }
            }

            private string getSectionHeader(string line, bool wantedOnly)
            {
                if (wantedOnly)
                    return _sectionsWanted.Find(line.StartsWith);

                return WORLD_FILE_SECTIONS.Find(line.StartsWith);

                bool sectionHeaderFound = _sectionsWanted.Exists(line.StartsWith);

                if (sectionHeaderFound)
                {
                    Plugin.Logger.LogInfo("Processing " + line);
                    return line;
                }

                Plugin.Logger.LogInfo($"Section header '{line}' not targeted");
                return null;
            }
        }
    }

    public enum WorldSection
    {
        Any = -1,
        ConditionalLinks = 0,
        Rooms = 1,
        Creatures = 2,
        BatMigrationBlockages = 3
    }
}
