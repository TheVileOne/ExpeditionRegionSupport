using System;
using System.Collections.Generic;
using System.IO;

namespace ExpeditionRegionSupport.Regions.Data
{
    public class RegionDataMiner
    {
        private TextReader _activeStream;
        protected TextReader ActiveStream
        {
            get => _activeStream;
            private set
            {
                //Close old stream - once we lose the reference, the stream wont be disposed
                if (!KeepStreamOpen && _activeStream != null && _activeStream != value)
                    CloseStream();

                _activeStream = value;
            }
        }

        /// <summary>
        /// The stream wont be closed when a read process finished. Stream will be disposed when RegionDataMiner is destroyed.
        /// </summary>
        public bool KeepStreamOpen;

        public const string SECTION_BAT_MIGRATION_BLOCKAGES = "BAT MIGRATION BLOCKAGES";
        public const string SECTION_CONDITIONAL_LINKS = "CONDITIONAL LINKS";
        public const string SECTION_CREATURES = "CREATURES";
        public const string SECTION_ROOMS = "ROOMS";

        public RegionDataMiner()
        {
        }

        public TextReader GetStreamReader(string regionCode)
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
                return new TextStream(regionFile);
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
                TextReader stream = GetStreamReader(regionCode);
                
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
            ActiveStream?.Close();
        }

        ~RegionDataMiner()
        {
            CloseStream();
        }
    }

    /// <summary>
    /// This custom class ensures that stream's dispose logic is only called once
    /// </summary>
    public class TextStream : StreamReader, IDisposable
    {
        public bool IsDisposed { get; private set; }

        public TextStream(string file) : base(file)
        {
        }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }

        public override void Close()
        {
            if (!IsDisposed)
                base.Close();
        }
    }
}
