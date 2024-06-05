using ExpeditionRegionSupport.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ExpeditionRegionSupport.Regions.Data
{
    public class RegionDataMiner : IDisposable
    {
        /// <summary>
        /// A dictionary that manages the file stream readers of all RegionDataMiner instances organized by world file path
        /// </summary>
        public static Dictionary<string, List<TextStream>> ManagedStreams = new Dictionary<string, List<TextStream>>();

        /// <summary>
        /// A list of specific streams controlled by the class
        /// </summary>
        public List<TextStream> ActiveStreams = new List<TextStream>();

        /// <summary>
        /// A flag that enables extra debug functionality such as extra data logging
        /// </summary>
        public bool DebugMode;

        private static void onStreamFinished(TextStream stream)
        {
            stream.AllowStreamDisposal = true;

            //Properly handle managed resources
            List<TextStream> managedStreams = ManagedStreams[stream.Filepath];
            if (managedStreams.TrueForAll(s => s.AllowStreamDisposal)) //Only dispose if every reference is allowed to dispose
            {
                managedStreams.ForEach(stream => stream.Close());
                managedStreams.Clear();
            }
            else
            {
                int waitingOnStreamCount = managedStreams.FindAll(s => !s.AllowStreamDisposal).Count;
                Plugin.Logger.LogInfo($"Data stream could not be disposed - Waiting on {waitingOnStreamCount} references");
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
        private bool isDisposed;

        internal static TextStream CreateStreamReader(string regionCode)
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
                TextStream stream = new TextStream(regionFile, false);

                stream.OnStreamEnd += onStreamFinished;

                //Register the stream reference to control how it is disposed
                if (ManagedStreams.ContainsKey(stream.Filepath))
                    ManagedStreams[stream.Filepath].Add(stream);
                else
                    ManagedStreams[stream.Filepath] = new List<TextStream> { stream };

                return stream;
            }
            catch (IOException)
            {
                Plugin.Logger.LogError("Could not read " + RegionUtils.FormatWorldFile(regionCode));
                return null;
            }
        }

        public EnumeratedWorldData GetBatMigrationLines(string regionCode)
        {
            return GetLines(regionCode, SECTION_BAT_MIGRATION_BLOCKAGES);
        }

        public EnumeratedWorldData GetConditionalLinkLines(string regionCode)
        {
            return GetLines(regionCode, SECTION_CONDITIONAL_LINKS);
        }

        public EnumeratedWorldData GetCreatureLines(string regionCode)
        {
            return GetLines(regionCode, SECTION_CREATURES);
        }

        public EnumeratedWorldData GetRoomLines(string regionCode)
        {
            return GetLines(regionCode, SECTION_ROOMS);
        }

        internal EnumeratedWorldData GetLines(string regionCode, params string[] sectionNames)
        {
            TextStream activeStream = CreateStreamReader(regionCode);

            ActiveStreams.Add(activeStream);
            return new EnumeratedWorldData(new ReadLinesIterator(activeStream, sectionNames));
        }

        ~RegionDataMiner()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (isDisposed) return;

            if (DebugMode)
                Plugin.Logger.LogInfo("Data Miner - Allowing data streams to close");

            ActiveStreams.ForEach(stream => stream.AllowStreamDisposal = true);

            //Properly handle managed resources
            foreach (List<TextStream> list in ManagedStreams.Values)
            {
                if (list.TrueForAll(s => s.AllowStreamDisposal))
                {
                    list.ForEach(stream => stream.Close());
                    list.Clear();
                }
            }

            ActiveStreams.Clear();

            if (DebugMode)
            {
                int undisposedStreamCount = 0;
                foreach (List<TextStream> list in ManagedStreams.Values)
                    undisposedStreamCount += list.Count;

                Plugin.Logger.LogInfo("Undisposed streams: " + undisposedStreamCount);
            }
            isDisposed = true;
        }

        public class ReadLinesIterator : IDisposable
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
                                //string statusString;
                                if (isSectionWanted)
                                {
                                    //statusString = "READING";
                                    _sectionsWanted.Remove(activeSection);
                                }
                                else
                                {
                                    //statusString = "SKIPPED";
                                    skipThisSection = true;
                                }

                                //Plugin.Logger.LogInfo($"Section header '{line}' ({statusString})");
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
                return header ?? WORLD_FILE_SECTIONS.Find(line.StartsWith);
            }

            #region Dispose Handlers
            protected virtual void Dispose(bool disposing)
            {
                if (!isDisposed)
                {
                    if (disposing)
                    {
                        _stream = null;
                        OnSectionStart = null;
                        OnSectionEnd = null;
                    }
                    isDisposed = true;
                }
            }

            ~ReadLinesIterator()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                Dispose(disposing: false);
            }

            public void Dispose()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
            #endregion
        }
    }

    public delegate void SectionEventHandler(string sectionName, bool isSectionWanted);

    public enum WorldSection
    {
        Any = -1,
        ConditionalLinks = 0,
        Rooms = 1,
        Creatures = 2,
        BatMigrationBlockages = 3
    }
}
