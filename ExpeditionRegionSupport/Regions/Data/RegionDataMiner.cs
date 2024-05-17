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

        public RegionDataMiner()
        {
        }

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

        public IEnumerable<string> GetBatMigrationLines(string regionCode)
        {
            return GetLines(regionCode, SECTION_BAT_MIGRATION_BLOCKAGES);
        }

        public IEnumerable<string> GetConditionalLinkLines(string regionCode)
        {
            return GetLines(regionCode, SECTION_CONDITIONAL_LINKS);
        }

        public IEnumerable<string> GetCreatureLines(string regionCode)
        {
            return GetLines(regionCode, SECTION_CREATURES);
        }

        public IEnumerable<string> GetRoomLines(string regionCode)
        {
            return GetLines(regionCode, SECTION_ROOMS);
        }

        internal IEnumerable<string> GetLines(string regionCode, string sectionName)
        {
            try
            {
                TextStream stream = GetStreamReader(regionCode);

                ActiveStream = stream;

                if (stream != null)
                {
                    string line;
                    bool sectionHeaderFound = false;
                    do
                    {
                        line = stream.ReadLine()?.Trim();

                        if (line == null) //End of file has been reached
                            yield break;

                        if (sectionHeaderFound)
                        {
                            if (line.StartsWith("//") || line == string.Empty) //Empty or commented out lines
                                continue;
                            if (line.StartsWith("END ")) //I hope all world files end their blocks with an end line
                                yield break;

                            yield return line;
                        }
                        else if (line.StartsWith(sectionName))
                            sectionHeaderFound = true; //The header doesn't need to be yielded
                    }
                    while (line != null);
                }
            }
            finally
            {
                if (!KeepStreamOpen)
                    CloseStream();
            }
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
    }
}
