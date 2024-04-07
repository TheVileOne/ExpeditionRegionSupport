using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Expedition;
using ExpeditionRegionSupport.Filters.Settings;
using ExpeditionRegionSupport.Filters.Utils;
using ExpeditionRegionSupport.Regions.Restrictions;

namespace ExpeditionRegionSupport.Regions
{
    public static class RegionUtils
    {
        public static Dictionary<Challenge, FilterApplicator<string>> RegionFilterCache = new Dictionary<Challenge, FilterApplicator<string>>();

        public static CachedFilterStack<string> AppliedFilters = new CachedFilterStack<string>();

        /// <summary>
        /// The most recent region filter assigned 
        /// </summary>
        public static CachedFilterApplicator<string> CurrentFilter => AppliedFilters.CurrentFilter;

        /// <summary>
        /// A flag that indicates that regions should be stored in a mod managed list upon retrieval
        /// </summary>
        public static bool CacheAvailableRegions;

        public static List<string> AvailableRegionCache;

        public static VisitedRegionsCache RegionsVisitedCache = new VisitedRegionsCache();

        /// <summary>
        /// A dictionary of regions visited, with a RegionCode as a key, and a list of slugcat names as data outside of Expedition
        /// </summary>
        public static Dictionary<string, List<string>> RegionsVisited => ProgressionData.Regions.Visited;

        public static bool HasVisitedRegion(SlugcatStats.Name slugcat, string regionCode)
        {
            if (RegionsVisitedCache.LastAccessed == slugcat)
                return RegionsVisitedCache.RegionsVisited.Contains(regionCode);

            List<string> visitorList;
            if (RegionsVisited.TryGetValue(regionCode, out visitorList))
                return visitorList.Contains(slugcat.value);

            Plugin.Logger.LogInfo("RegionCode detected that isn't part of RegionsVisited dictionary");
            return false;
        }

        public static List<string> GetAvailableRegions(SlugcatStats.Name slugcat)
        {
            //TODO: This logic should not be limited to story regions
            if (!CacheAvailableRegions)
            {
                AvailableRegionCache = null;
                return SlugcatStats.SlugcatStoryRegions(slugcat);
            }

            //Applied filters take priority over the standard cache
            if (AppliedFilters.BaseFilter != null)
                return CurrentFilter.Cache;

            //Make sure cache is applied
            if (AvailableRegionCache == null)
                AvailableRegionCache = SlugcatStats.SlugcatStoryRegions(slugcat);

            return AvailableRegionCache;
        }

        public static List<string> GetVisitedRegions(SlugcatStats.Name slugcat)
        {
            if (RegionsVisitedCache.LastAccessed == slugcat)
                return RegionsVisitedCache.RegionsVisited;

            var enumerator = RegionsVisited.GetEnumerator();

            List<string> visitedRegions = new List<string>();
            while (enumerator.MoveNext())
            {
                string regionCode = enumerator.Current.Key;
                List<string> regionVisitors = enumerator.Current.Value;

                if (regionVisitors.Contains(slugcat.value))
                    visitedRegions.Add(regionCode);
            }

            if (Plugin.DebugMode)
            {
                Plugin.Logger.LogInfo("Visited regions for " + slugcat.value);
                Plugin.Logger.LogInfo(visitedRegions.Count + " region" + (visitedRegions.Count != 1 ? "s" : string.Empty) + " detected");

                if (visitedRegions.Count > 0)
                {
                    StringBuilder sb = new StringBuilder("Regions ");
                    foreach (string region in visitedRegions)
                        sb.Append(region).Append(" ,");
                    Plugin.Logger.LogInfo(sb.ToString().TrimEnd(','));
                }
            }

            RegionsVisitedCache.LastAccessed = slugcat;
            RegionsVisitedCache.RegionsVisited = visitedRegions;
            return visitedRegions;
        }

        public static string GetPearlDeliveryRegion(WorldState state)
        {
            return state != WorldState.Artificer ? "SL" : "SS";
        }

        public static WorldState GetWorldStateFromStoryRegions(SlugcatStats.Name name, List<string> storyRegions = null)
        {
            if (storyRegions == null)
                storyRegions = SlugcatStats.SlugcatStoryRegions(name);

            WorldState state = WorldState.Any;

            if (ModManager.MSC)
            {
                if (storyRegions.Contains("MS")) //Submerged Superstructure
                    state = WorldState.Rivulet;
                else if (storyRegions.Contains("OE")) //Outer Expanse
                    state = WorldState.Gourmand;
                else if (storyRegions.Contains("LM"))
                {
                    if (storyRegions.Contains("LC")) //Waterfront Facility + Metropolis
                        state = WorldState.Artificer;
                    else if (storyRegions.Contains("DM")) //Waterfront Facility + LTTM
                        state = WorldState.SpearMaster;
                }
                else if (storyRegions.Contains("UG")) //Undergrowth
                    state = WorldState.Saint;
            }

            //At this point, we are most likely not a slugcat that depends on a MSC exclusive region.
            if (state == WorldState.Any)
            {
                //For vanilla slugcats, give them limited access
                //WorldStates for Monk, Survivor and Hunter are the same value as WorldState.Standard. Obviously they have unique WorldStates,
                //but all three characters have access to the same story regions.
                //For custom slugcats unrelated to MSC, give them full access
                if (name == SlugcatStats.Name.White || name == SlugcatStats.Name.Yellow || name == SlugcatStats.Name.Red)
                    state = WorldState.Vanilla;
                else
                    state = WorldState.Other;
            }

            string stateString = state.ToString();

            if (stateString == "Hunter" || stateString == "Survivor" || stateString == "Monk")
                stateString = "Vanilla";

            Plugin.Logger.LogInfo("World State " + stateString);
            return state;
        }

        /// <summary>
        /// Assigns the VisitedRegion filter as the BaseFilter to a list of currently available region codes and applies the filter
        /// </summary>
        /// <param name="name">The slugcat name used to retrieve visited region data</param>
        public static void AssignFilter(SlugcatStats.Name name)
        {
            Plugin.Logger.LogInfo("Applying region filter");
            AppliedFilters.AssignBase(new CachedFilterApplicator<string>(GetAvailableRegions(name)));

            //The filter is not yet applied, lets handle that logic here
            if (RegionFilterSettings.IsFilterActive(FilterOption.VisitedRegionsOnly))
            {
                List<string> visitedRegions = GetVisitedRegions(name);
                CurrentFilter.Apply(visitedRegions.Contains); //Filters all unvisited regions and stores it in a list cache
            }
        }

        /// <summary>
        /// Retrieves a filter from the filter cache if one exists. Creates a new filter if it doesn't exist.
        /// </summary>
        public static void AssignFilter(Challenge challenge)
        {
            Challenge challengeType = ChallengeUtils.GetChallengeType(challenge);

            FilterApplicator<string> challengeFilter;
            if (RegionFilterCache.TryGetValue(challenge, out challengeFilter))
            {
                AppliedFilters.Assign((CachedFilterApplicator<string>)challengeFilter);
            }
            else if (AppliedFilters.BaseFilter != null)
            {
                //AssignNew automatically inherits from the previous filter
                RegionFilterCache.Add(challengeType, AppliedFilters.AssignNew());
            }
        }

        public static void ClearFilters()
        {
            Plugin.Logger.LogInfo("Clearing filters");
            AppliedFilters.Clear();
            RegionFilterCache.Clear();
        }

        public static bool IsVanillaRegion(string regionCode)
        {
            return
                regionCode == "SU" || //Outskirts
                regionCode == "HI" || //Industrial Complex
                regionCode == "DS" || //Drainage Systems
                regionCode == "CC" || //Chimney Canopy
                regionCode == "GW" || //Garbage Wastes
                regionCode == "SH" || //Shaded Citadel
                regionCode == "SL" || //Shoreline
                regionCode == "SI" || //Sky Islands
                regionCode == "LF" || //Farm Arrays
                regionCode == "UW" || //The Exterior
                regionCode == "SS" || //Five Pebbles
                regionCode == "SB";   //Subterranean
        }

        public static bool IsMSCRegion(string regionCode)
        {
            return
                regionCode == "MS" || //Submerged Superstructure
                regionCode == "OE" || //Outer Expanse
                regionCode == "HR" || //Rubicon
                regionCode == "LM" || //Waterfront Facility
                regionCode == "DM" || //Lttm (Spearmaster)
                regionCode == "LC" || //Metropolis
                regionCode == "RM" || //The Rot
                regionCode == "CL" || //Silent Construct
                regionCode == "UG" || //Undergrowth
                regionCode == "VS";   //Pipeyard
        }

        public static bool IsCustomRegion(string regionCode)
        {
            return !IsVanillaRegion(regionCode) && !IsMSCRegion(regionCode);
        }

        public static string[] ParseRoomName(string roomName, out string regionCode, out string roomCode)
        {
            string[] roomInfo = SplitRoomName(roomName);

            regionCode = roomInfo[0];
            roomCode = null;

            if (roomInfo.Length > 1)
                roomCode = FormatRoomCode(roomInfo);

            return roomInfo;
        }

        public static string[] SplitRoomName(string roomName)
        {
            return Regex.Split(roomName, "_");
        }

        public static string FormatRoomName(string regionCode, string roomCode)
        {
            return regionCode + '_' + roomCode;
        }

        public static string FormatRoomName(string[] roomInfo)
        {
            string regionCode = roomInfo[0];
            string roomCode = FormatRoomCode(roomInfo);

            return FormatRoomName(regionCode, roomCode);
        }

        /// <summary>
        /// Processes every index after the first as part of the room code.
        /// </summary>
        public static string FormatRoomCode(string[] roomInfo)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 1; i < roomInfo.Length; i++)
            {
                sb.Append(roomInfo[i]);

                if (i != roomInfo.Length - 1) //Add back any extra underscores
                    sb.Append('_');
            }

            return sb.ToString();
        }

        public static bool RoomExists(string roomName)
        {
            return RainWorld.roomNameToIndex.ContainsKey(roomName);
        }

        /// <summary>
        /// Checks whether string contains valid room code information
        /// </summary>
        public static bool ContainsRoomData(string data)
        {
            if (data.Length < 4) return false; //XX_X - Lowest amount of characters for a valid roomname

            return data[2] == '_' || data[3] == '_';
        }
    }
}
