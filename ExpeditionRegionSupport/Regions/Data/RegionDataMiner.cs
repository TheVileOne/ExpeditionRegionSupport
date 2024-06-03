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
            return new EnumeratedWorldData(new RegionDataMinerIterator(activeStream, sectionNames));
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
