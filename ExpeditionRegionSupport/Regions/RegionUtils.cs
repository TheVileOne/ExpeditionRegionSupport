using System;
using System.Collections.Generic;
using System.IO;
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
        public static readonly List<RestrictionCheck> RestrictionChecks = new List<RestrictionCheck>();

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

        /// <summary>
        /// A dictionary of shelters sorted by RegionCode
        /// </summary>
        public static Dictionary<string, List<ShelterInfo>> Shelters = new Dictionary<string, List<ShelterInfo>>();

        public static bool HasVisitedRegion(SlugcatStats.Name slugcat, string regionCode)
        {
            if (RegionsVisitedCache.LastAccessed == slugcat)
                return RegionsVisitedCache.RegionsVisited.Contains(regionCode);

            List<string> visitorList;
            if (RegionsVisited.TryGetValue(regionCode, out visitorList))
                return visitorList.Contains(slugcat.value);

            Plugin.Logger.LogWarning("Unexpected region detected that isn't part of RegionsVisited dictionary");
            return false;
        }

        public static List<string> GetAllRegions()
        {
            return ProgressionData.PlayerData.ProgressData.regionNames.ToList();
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
                        sb.Append(region).Append(", ");
                    Plugin.Logger.LogInfo(sb.ToString().TrimEnd().TrimEnd(','));
                }
            }

            RegionsVisitedCache.LastAccessed = slugcat;
            RegionsVisitedCache.RegionsVisited = visitedRegions;
            return visitedRegions;
        }

        public static List<ShelterInfo> GetAllShelters()
        {
            List<ShelterInfo> allShelters = new List<ShelterInfo>();

            foreach (string regionCode in GetAllRegions())
                allShelters.AddRange(GetShelters(regionCode));
            return allShelters;
        }

        public static List<ShelterInfo> GetShelters(string regionCode)
        {
            if (Shelters.TryGetValue(regionCode, out List<ShelterInfo> shelterCache))
                return shelterCache;

            RegionDataMiner regionMiner = new RegionDataMiner();

            IEnumerable<string> roomData = regionMiner.GetRoomLines(regionCode);

            if (roomData == null)
                return new List<ShelterInfo>();

            List<ShelterInfo> shelters = new List<ShelterInfo>();

            Shelters[regionCode] = shelters;

            //TODO: Need to detect non-broken conditional shelters
            foreach (string shelterLine in roomData.Where(roomLine => roomLine.EndsWith("SHELTER")))
            {
                int sepIndex = shelterLine.IndexOf(':'); //Expects format "SL_S09:SL_C05:SHELTER

                if (sepIndex != -1) //Format is okay
                    shelters.Add(new ShelterInfo(shelterLine.Substring(0, sepIndex)));
            }

            string regionFile = GetWorldFilePath(regionCode);
            string propertiesFile = GetFilePath(regionCode, "properties.txt");

            //Look for broken shelter info
            if (shelters.Count > 0 && File.Exists(propertiesFile))
            {
                List<string> brokenShelterData = new List<string>();
                using (TextReader stream = new StreamReader(propertiesFile))
                {
                    string line;
                    while ((line = stream.ReadLine()) != null)
                    {
                        if (line.StartsWith("Broken Shelters"))
                        {
                            int sepIndex = line.IndexOf(':');

                            if (sepIndex != -1 && sepIndex != line.Length - 1) //Format is okay, and there is data on this line
                                brokenShelterData.Add(line.Substring(sepIndex + 1)); //Whole line is stored, will be processed later
                        }
                    }
                }

                if (brokenShelterData.Count > 0)
                {
                    Plugin.Logger.LogInfo("Broken shelter data found for region " + regionCode);

                    ShelterInfo lastShelterProcessed = default;
                    foreach (string shelterDataRaw in brokenShelterData)
                    {
                        string[] shelterData = shelterDataRaw.Split(':'); //Expects " White: SL_S11" (whitespace is expected)

                        if (shelterData.Length >= 2) //Expected length - Something is unusual is there if it is anything else
                        {
                            string[] roomCodes = shelterData[1].Split(',');

                            for (int i = 0; i < roomCodes.Length; i++)
                            {
                                string roomCode = roomCodes[i].Trim();

                                bool isNewShelter = lastShelterProcessed.RoomCode != roomCode;

                                //It is common to have the same shelter being targeted across multiple lines
                                ShelterInfo shelter = isNewShelter ?
                                    shelters.Find(s => string.Equals(s.RoomCode, roomCode, StringComparison.InvariantCultureIgnoreCase))
                                  : lastShelterProcessed;

                                if (shelter.RoomCode == roomCode) //ShelterInfo is a struct, checking for this lets us know if we found a match
                                {
                                    lastShelterProcessed = shelter;

                                    string[] slugcats = shelterData[0].Split(',');

                                    foreach (string slugcat in slugcats)
                                    {
                                        //Slugcat may not be available if this fails, which should be fine.
                                        if (SlugcatUtils.TryParse(slugcat, out SlugcatStats.Name found))
                                            shelter.BrokenForTheseSlugcats.Add(found);
                                    }

                                    //This shelter is likely registered as broken, but unsure how the game handles it without slugcat info
                                    if (shelter.BrokenForTheseSlugcats.Count == 0)
                                        Plugin.Logger.LogInfo($"Line 'Broken Shelters: {shelterDataRaw}' has no recognizable slugcat info");
                                }
                                else //Stray property line doesn't match any shelter data processed
                                {
                                    Plugin.Logger.LogInfo("Broken shelter references a room that cannot be found");
                                    Plugin.Logger.LogInfo($"Shelter room [{shelter.RoomCode}]");
                                    Plugin.Logger.LogInfo($"Room [{roomCode}]");
                                }
                            }
                        }
                        else
                        {
                            Plugin.Logger.LogInfo($"Line 'Broken Shelters: {shelterDataRaw}' is invalid");
                        }
                    }
                }
            }

            return shelters;
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

            Plugin.Logger.LogInfo("World State " + state);
            return state;
        }

        /// <summary>
        /// Assigns a code based restriction check that will be used to evaluate available region spawns during region selection  
        /// </summary>
        public static void AssignRestriction(RestrictionCheck restriction)
        {
            RestrictionChecks.Add(restriction);
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

        public static bool IsDownpourRegion(string regionCode)
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
            return !IsVanillaRegion(regionCode) && !IsDownpourRegion(regionCode);
        }

        /// <summary>
        /// Gets full path to the world file for a specified region
        /// </summary>
        public static string GetWorldFilePath(string regionCode)
        {
            return GetFilePath(regionCode, FormatWorldFile(regionCode));
        }

        /// <summary>
        /// Gets full path to a file stored in the world/<regionCode> directory
        /// </summary>
        public static string GetFilePath(string regionCode, string fileWanted)
        {
            return AssetManager.ResolveFilePath(Path.Combine("world", regionCode, fileWanted));
        }

        /// <summary>
        /// Formats region code to the standardized world file format
        /// </summary>
        public static string FormatWorldFile(string regionCode)
        {
            return string.Format("world_{0}.txt", regionCode.ToLower());
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
