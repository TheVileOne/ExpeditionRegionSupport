using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport
{
    public static class ProgressionData
    {
        public static class PlayerData
        {
            /// <summary>
            /// The reference to PlayerProgression outside of Expedition mode
            /// </summary>
            public static PlayerProgression ProgressData;
            public static PlayerProgression.MiscProgressionData MiscProgressData => ProgressData.miscProgressionData;
        }

        public static class ExpeditionPlayerData
        {
            /// <summary>
            /// The reference to PlayerProgression while playing Expedition mode
            /// </summary>
            public static PlayerProgression ProgressData;
            public static PlayerProgression.MiscProgressionData MiscProgressData => ProgressData.miscProgressionData;
        }

        public static class Regions
        {
            /// <summary>
            /// A dictionary of regions visited, with a RegionCode as a key, and a record of slugcat names as data
            /// </summary>
            public static readonly Dictionary<string, List<string>> Visited = new Dictionary<string, List<string>>();

            public static void UpdateRegionsVisited()
            {
                Dictionary<string, List<string>> regionsVisitedMain, regionsVisitedExpedition;

                regionsVisitedMain = ProgressionData.PlayerData.MiscProgressData.regionsVisited;
                regionsVisitedExpedition = ProgressionData.ExpeditionPlayerData.MiscProgressData.regionsVisited;

                Visited.Clear();
                Visited.AddRange(regionsVisitedMain);

                foreach (var entry in regionsVisitedExpedition.Except(regionsVisitedMain))
                    Visited.Add(entry.Key, entry.Value);
            }
        }
    }
}
