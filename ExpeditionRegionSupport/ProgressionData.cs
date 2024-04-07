using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport
{
    /// <summary>
    /// A class for managing game-controlled progression data
    /// </summary>
    public class ProgressionData
    {
        /// <summary>
        /// The reference to PlayerProgression outside of Expedition mode
        /// </summary>
        public static ProgressionData PlayerData;

        /// <summary>
        /// The reference to PlayerProgression while playing Expedition mode
        /// </summary>
        public static ProgressionData ExpeditionPlayerData;

        public PlayerProgression ProgressData;
        public PlayerProgression.MiscProgressionData MiscProgressData => ProgressData.miscProgressionData;

        public ProgressionData(PlayerProgression progression)
        {
            ProgressData = progression;
        }

        public static class Regions
        {
            public static bool HasStaleRegionCache = true;

            private static readonly Dictionary<string, List<string>> visited = new Dictionary<string, List<string>>();
            /// <summary>
            /// A dictionary of regions visited, with a RegionCode as a key, and a record of slugcat names as data
            /// </summary>
            public static Dictionary<string, List<string>> Visited
            {
                get
                {
                    if (HasStaleRegionCache)
                    {
                        UpdateRegionsVisited();
                        HasStaleRegionCache = false;
                    }
                    return visited;
                }
            }

            public static void UpdateRegionsVisited()
            {
                Dictionary<string, List<string>> regionsVisitedMain, regionsVisitedExpedition;

                regionsVisitedMain = PlayerData.MiscProgressData.regionsVisited;
                regionsVisitedExpedition = ExpeditionPlayerData.MiscProgressData.regionsVisited;

                Visited.Clear();
                Visited.AddRange(regionsVisitedMain);

                foreach (var entry in regionsVisitedExpedition.Except(regionsVisitedMain))
                    Visited.Add(entry.Key, entry.Value);
            }
        }
    }
}
