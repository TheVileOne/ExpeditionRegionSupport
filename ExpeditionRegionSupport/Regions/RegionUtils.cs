using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Expedition;
using ExpeditionRegionSupport.Filters.Settings;
using ExpeditionRegionSupport.Filters.Utils;
using ExpeditionRegionSupport.Regions.Data;
using ExpeditionRegionSupport.Regions.Restrictions;
using ExpeditionRegionSupport.Tools;
using RWCustom;

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

        public static RegionsCache RegionsVisitedCache = new RegionsCache();

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
                return RegionsVisitedCache.Regions.Contains(regionCode);

            if (RegionsVisited.TryGetValue(regionCode, out List<string> visitorList))
                return visitorList.Contains(slugcat.value);

            Plugin.Logger.LogWarning("Unexpected region detected that isn't part of RegionsVisited dictionary");
            return false;
        }

        public static string[] GetAllRegions()
        {
            string[] regions = null;
            if (Custom.rainWorld != null)
            {
                if (Custom.rainWorld.progression != null)
                    regions = Custom.rainWorld.progression.regionNames;
                else
                    regions = ProgressionData.PlayerData?.ProgressData?.regionNames;
            }

            if (regions == null)
            {
                Plugin.Logger.LogInfo("Getting regions from file");

                string path = AssetManager.ResolveFilePath(Path.Combine("World", "regions.txt"));
                if (File.Exists(path))
                    regions = File.ReadAllLines(path);
            }

            return regions;
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

        /// <summary>
        /// Stores a list of regions accesible from a region (stored as the key)
        /// </summary>
        public static RegionsCache RegionAccessibilityCache;

        /// <summary>
        /// Returns all active regions that can theoretically be accessed from a given region
        /// </summary>
        public static List<string> GetAccessibleRegions(string regionCode, SlugcatStats.Name slugcat)
        {
            RegionsCache regionCache = RegionsCache.GetOrCreate(RegionAccessibilityCache, slugcat);

            string slugcatEquivalentRegion = GetSlugcatEquivalentRegion(regionCode, slugcat); //Checking incompatible world state is not yet supported

            //The region will either exist in the cache, or belong to an entirely new series of regions
            if (regionCache.Regions.Contains(slugcatEquivalentRegion))
                return regionCache.Regions;

            if (regionCache.Regions.Count > 0) //Do not clear old cache data here - Allow cache to be stored and compared by reference
            {
                regionCache = new RegionsCache();
                regionCache.LastAccessed = slugcat;
            }

            regionCache.Store(slugcatEquivalentRegion);
            FindAllConnectingRegionsRecursive(regionCache.Regions, slugcatEquivalentRegion, slugcat);

            RegionAccessibilityCache = regionCache;
            return regionCache.Regions;
        }

        internal static void FindAllConnectingRegionsRecursive(List<string> connectedRegions, string regionCode, SlugcatStats.Name slugcat, bool firstPass = true)
        {
            MultiUseTimer processTimer = null;
            if (Plugin.DebugMode)
            {
                processTimer = DebugMode.CreateMultiUseTimer(DebugMode.RunningDebugProcess, TimerOutput.RelativeIncrements, false);
                processTimer.ID = "Region Connections";
                processTimer.ReportTotalTime = true;
                processTimer.TotalTimeHeader = $"Getting all connecting regions for {regionCode} (" + (firstPass ? "TOP" : "SUB") + ")";
                processTimer.Start();
            }

            //Get all region codes that connect with this region, and are accessible for the given slugcat
            foreach (string connectedRegion in GetConnectingRegions(regionCode, slugcat, !firstPass))
            {
                //No connecting region will have been compared against the slugcat at this stage
                string slugcatEquivalentRegion = GetSlugcatEquivalentRegion(connectedRegion, slugcat); //The region this slugcat will load from a region gate
                if (!connectedRegions.Contains(slugcatEquivalentRegion))
                {
                    //Add the equivalent region, and then check its connecting regions
                    connectedRegions.Add(slugcatEquivalentRegion);
                    FindAllConnectingRegionsRecursive(connectedRegions, slugcatEquivalentRegion, slugcat, false);
                    processTimer.ReportTime("Getting connecting regions for " + slugcatEquivalentRegion);
                }
            }

            if (Plugin.DebugMode)
                processTimer.Stop();
        }

        /// <summary>
        /// Finds the list of regions that border a given region based on the active gates defined in that region's world file 
        /// </summary>
        /// <param name="regionCode">The region to check</param>
        /// <param name="slugcat">The slugcat to check (in the case of conditional links)</param>
        /// <param name="adjustForSlugcatEquivalences">The proper region code will be used instead of the default when this is true</param>
        public static List<string> GetConnectingRegions(string regionCode, SlugcatStats.Name slugcat, bool adjustForSlugcatEquivalences)
        {
            DebugTimer processTimer = null;
            if (Plugin.DebugMode)
            {
                processTimer = DebugMode.CreateTimer(DebugMode.RunningDebugProcess, false);
                processTimer.Start();
            }

            string slugcatEquivalentRegion, regionBaseEquivalent;

            //This adjust parameter was originally included to reduce equivalency checks. With the cache, this optimisation is probably not necessary.
            //Functionally, this only skips the equivalency check on the first call of this method from FindAllConnectingRegionsRecursive, which already
            //has the equivalency check applied
            if (adjustForSlugcatEquivalences)
                slugcatEquivalentRegion = GetSlugcatEquivalentRegion(regionCode, slugcat, out regionBaseEquivalent);
            else
                slugcatEquivalentRegion = regionBaseEquivalent = regionCode;

            /*
            string reportString = $"Getting connecting regions for {slugcatEquivalentRegion}";

            if (adjustForSlugcatEquivalences && regionCode != slugcatEquivalentRegion)
                reportString += $" (Changed from {regionCode})";

            Plugin.Logger.LogInfo(reportString);
            */

            List<string> connectedRegions = new List<string>();
            foreach (GateInfo gate in GetRegionGates(slugcatEquivalentRegion))
            {
                if (!gate.IsActiveFor(slugcat)) continue;

                string connectedRegion = gate.OtherConnection(regionBaseEquivalent, slugcatEquivalentRegion);

                if (connectedRegion != null) //Null indicates that gate region codes do not match region code parameters
                    connectedRegions.Add(connectedRegion);
                else
                    Plugin.Logger.LogInfo("Gate ignored: " + gate.RoomCode);
            }

            if (Plugin.DebugMode)
            {
                processTimer.ReportTime("Finding connections");
                processTimer.Stop();
            }
            return connectedRegions;
        }

        /// <summary>
        /// Gets the proper region equivalent of the region code for a particular slugcat
        /// </summary>
        /// <param name="regionBaseEquivalent">The slugcat independent region code equivalent</param>
        public static string GetSlugcatEquivalentRegion(string regionCode, SlugcatStats.Name slugcat, out string regionBaseEquivalent)
        {
            return EquivalentRegionCache.GetSlugcatEquivalentRegion(regionCode, slugcat, out regionBaseEquivalent);
        }

        /// <summary>
        /// Gets the proper region equivalent of the region code for a particular slugcat
        /// </summary>
        public static string GetSlugcatEquivalentRegion(string regionCode, SlugcatStats.Name slugcat)
        {
            return EquivalentRegionCache.GetSlugcatEquivalentRegion(regionCode, slugcat);
        }

        public static List<string> GetVisitedRegions(SlugcatStats.Name slugcat)
        {
            if (RegionsVisitedCache.LastAccessed == slugcat)
                return RegionsVisitedCache.Regions;

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
                    Plugin.Logger.LogInfo("Regions " + visitedRegions.FormatToString(','));
            }

            RegionsVisitedCache.LastAccessed = slugcat;
            RegionsVisitedCache.Regions = visitedRegions;
            return visitedRegions;
        }

        public static AbstractRoom GetEquivalentGateRoom(World world, string roomName, bool fallbackCheck)
        {
            if (!fallbackCheck)
            {
                AbstractRoom room = world.GetAbstractRoom(roomName);

                if (room != null)
                    return room;
            }

            Plugin.Logger.LogInfo($"Gate room {roomName} has not been found. Checking for equivalent rooms");

            if (!HasGatePrefix(roomName))
            {
                Plugin.Logger.LogInfo("Room is not a gate room");
                return null;
            }

            string[] roomInfo = ParseRoomName(roomName, out string regionCodeA, out string regionCodeB);

            IEnumerable<string> equivalentRegionCombinationsA, equivalentRegionCombinationsB;

            equivalentRegionCombinationsA = GetAllEquivalentRegions(regionCodeA, true);
            equivalentRegionCombinationsB = GetAllEquivalentRegions(regionCodeB, true);

            IEnumerable<string> roomCombinations = GetEquivalentGateRoomCombinations(equivalentRegionCombinationsA, equivalentRegionCombinationsB);

            //Plugin.Logger.LogInfo("ROOM COMBINATIONS: " + roomCombinations.FormatToString(','));
            
            return world.GetAbstractRoomFrom(roomCombinations);
        }

        public static IEnumerable<string> GetAllEquivalentRegions(string regionCode, bool includeSelf)
        {
            return EquivalentRegionCache.GetAllEquivalentRegions(regionCode, includeSelf);
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
            foreach (string roomLine in roomData)
            {
                string shelterCode = GetShelterCodeWithValidation(roomLine);

                if (shelterCode != null)
                    shelters.Add(new ShelterInfo(shelterCode));
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

        public static List<GateInfo> GetRegionGates(string regionCode)
        {
            DebugTimer processTimer = null;
            if (Plugin.DebugMode)
            {
                processTimer = DebugMode.CreateMultiUseTimer(DebugMode.RunningDebugProcess, TimerOutput.AbsoluteTime, false);
                processTimer.Start();
            }

            RegionDataMiner regionMiner = new RegionDataMiner()
            {
                KeepStreamOpen = true
            };

            List<string> conditionalLinkData = regionMiner.GetConditionalLinkLines(regionCode).ToList();
            IEnumerable<string> roomData = regionMiner.GetRoomLines(regionCode);

            if (Plugin.DebugMode)
            {
                processTimer.ReportTime("File read time for " + regionCode);
                processTimer.Reset();
            }

            if (roomData == null)
                return new List<GateInfo>();

            if (Plugin.DebugMode)
                processTimer.Start();

            List<GateInfo> gates = new List<GateInfo>();

            foreach (string roomLine in roomData)
            {
                string gateCode = GetGateCodeWithValidation(roomLine);

                if (gateCode != null)
                {
                    GateInfo gate = new GateInfo(gateCode);

                    //Handle conditional link information
                    foreach (string conditionalLink in conditionalLinkData.Where(r => r.Contains("EXCLUSIVEROOM") && r.TrimEnd().EndsWith(gate.RoomCode)))
                        gate.ConditionalAccess.Add(SlugcatUtils.GetOrCreate(conditionalLink.Substring(0, conditionalLink.IndexOf(':')))); //The first section is the slugcat

                    /*
                    if (gate.ConditionalAccess.Count > 0)
                    {
                        Plugin.Logger.LogInfo("CONDITIONAL INFO");
                        foreach (SlugcatStats.Name slugcat in gate.ConditionalAccess)
                            Plugin.Logger.LogInfo(slugcat);
                    }
                    */
                    gates.Add(gate);
                }
            }

            if (Plugin.DebugMode)
            {
                processTimer.ReportTime("Gate process time for " + regionCode);
                processTimer.Stop();
            }

            regionMiner.KeepStreamOpen = false;
            return gates;
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

            if (RegionFilterCache.TryGetValue(challenge, out FilterApplicator<string> challengeFilter))
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

        public static EquivalencyCache EquivalentRegionCache;

        /// <summary>
        /// Reads all equivalences.txt files and compiles lines into a dictionary for quick access
        /// </summary>
        public static void CacheEquivalentRegions()
        {
            Plugin.Logger.LogInfo("Caching equivalent regions");

            string[] regions = GetAllRegions();

            //MSC regions with conditional equivalencies
            RegionProfile profile_DS, profile_SH, profile_SL, profile_SS, //Vanilla profiles
                          profile_UG, profile_CL, profile_LM, profile_RM; //MSC profiles

            profile_DS = profile_SH = profile_SL = profile_SS = default;
            profile_UG = profile_CL = profile_LM = profile_RM = default;

            EquivalentRegionCache = new EquivalencyCache();

            //Store a RegionProfile object for each region
            foreach (string regionCode in regions)
            {
                RegionProfile currentProfile = EquivalentRegionCache.Store(regionCode);

                if (ModManager.MSC)
                {
                    switch (regionCode)
                    {
                        case "DS":
                            profile_DS = currentProfile;
                            break;
                        case "SH":
                            profile_SH = currentProfile;
                            break;
                        case "SL":
                            profile_SL = currentProfile;
                            break;
                        case "SS":
                            profile_SS = currentProfile;
                            break;
                        case "UG":
                            profile_UG = currentProfile;
                            break;
                        case "CL":
                            profile_CL = currentProfile;
                            break;
                        case "LM":
                            profile_LM = currentProfile;
                            break;
                        case "RM":
                            profile_RM = currentProfile;
                            break;
                    }
                }
            }

            //Apply known MSC equivalencies here
            if (ModManager.MSC)
            {
                profile_DS.RegisterEquivalency(MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Saint, profile_UG);
                profile_SH.RegisterEquivalency(MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Saint, profile_CL);
                profile_SL.RegisterEquivalency(MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Spear, profile_LM);
                profile_SL.RegisterEquivalency(MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Artificer, profile_LM);
                profile_SS.RegisterEquivalency(MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Rivulet, profile_RM);

                //This slugcat has hooked their equivalent region checks. Until that changes, hardcode these equivalencies.
                SlugcatStats.Name technomancer = new SlugcatStats.Name("technomancer");

                profile_SL.RegisterEquivalency(technomancer, profile_LM);

                RegionProfile profile_SB = EquivalentRegionCache.FindProfile("SB");
                profile_SB.RegisterEquivalency(technomancer, EquivalentRegionCache.FindProfile("TL"));
            }

            foreach (string path in GetFilePathFromAllSources("equivalences.txt"))
            {
                Plugin.Logger.LogInfo("Reading from " + path);

                //The region code stored here replaces any region mentioned in the equivalences.txt file
                string regionCodeFromDirectory = Path.GetFileName(Path.GetDirectoryName(path)).ToUpper(); //Get region code from containing directory

                RegionProfile targetRegion = EquivalentRegionCache.FindProfile(regionCodeFromDirectory);

                if (targetRegion.IsDefault)
                {
                    Plugin.Logger.LogInfo("Region code is unrecognized");
                    continue;
                }

                string[] fileData = File.ReadAllLines(path);

                foreach (string line in fileData)
                {
                    string[] dataValues = line.Trim().Split(','); //Entries are separated by commas

                    foreach (string equivalencyEntry in dataValues)
                    {
                        int sepIndex = equivalencyEntry.IndexOf('-'); //Either will be SU, SU-Spear, or Spear-SU

                        bool appliesToAllSlugcats = sepIndex == -1; //This will get overwritten by any conflicting values

                        string registerTarget;
                        SlugcatStats.Name slugcat;
                        if (appliesToAllSlugcats)
                        {
                            registerTarget = equivalencyEntry.Trim().ToUpper();
                            slugcat = SlugcatUtils.AnySlugcat;
                        }
                        else
                        {
                            //One of these values is the region code, and the other is a slugcat name
                            string valueA = equivalencyEntry.Substring(0, sepIndex).Trim();
                            string valueB = equivalencyEntry.Substring(sepIndex + 1).Trim();

                            if (valueA.Length <= 2)
                            {
                                slugcat = equivalentRegionsCacheHelper(regions, valueA, valueB, out registerTarget);
                            }
                            else if (valueB.Length <= 2)
                            {
                                slugcat = equivalentRegionsCacheHelper(regions, valueB, valueA, out registerTarget);
                            }
                            else //Neither are standard length for a region
                            {
                                slugcat = equivalentRegionsCacheHelper(regions, valueA, valueB, out registerTarget);
                            }
                        }

                        if (registerTarget == null) continue; //The region is likely part of an unloaded mod

                        RegionProfile slugcatEquivalentRegion = EquivalentRegionCache.FindProfile(registerTarget);
                        targetRegion.RegisterEquivalency(slugcat, slugcatEquivalentRegion);
                    }
                }
            }

            if (Plugin.DebugMode)
                EquivalentRegionCache.LogEquivalencyRelationships();
        }

        private static SlugcatStats.Name equivalentRegionsCacheHelper(string[] regions, string valueA, string valueB, out string regionCode)
        {
            regionCode = Array.Find(regions, r => r == valueA.ToUpper());

            SlugcatStats.Name slugcat = null;
            if (regionCode != null)
                slugcat = SlugcatUtils.GetOrCreate(valueB);
            else
            {
                regionCode = Array.Find(regions, r => r == valueB.ToUpper());

                if (regionCode != null)
                    slugcat = SlugcatUtils.GetOrCreate(valueA);
            }
            return slugcat;
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
        /// Gets all version of a file from the toplevel World directory from all locations (expensive)
        /// </summary>
        public static IEnumerable<string> GetFilePathFromAllSources(string fileWanted)
        {
            return AssetManager.ListDirectory("World", true).Select(path =>
            {
                return AssetManager.ResolveFilePath(Path.Combine("World", Path.GetFileName(path), fileWanted));
            }).Where(File.Exists);
        }

        /// <summary>
        /// Formats region code to the standardized world file format
        /// </summary>
        public static string FormatWorldFile(string regionCode)
        {
            return string.Format("world_{0}.txt", regionCode.ToLower());
        }

        /// <summary>
        /// Gets the region part of a room name
        /// </summary>
        public static string ExtractRegionCode(string roomName)
        {
            ParseRoomName(roomName, out string regionCode, out _);
            return regionCode;
        }

        /// <summary>
        /// Gets the room part of a room name
        /// </summary>
        public static string ExtractRoomCode(string roomName)
        {
            ParseRoomName(roomName, out _, out string roomCode);
            return roomCode;
        }

        /// <summary>
        /// Splits a room name into its parts returning the results in an array. The region part, and the room part are available in out parameters
        /// </summary>
        public static string[] ParseRoomName(string roomName, out string regionCode, out string roomCode)
        {
            string[] roomInfo = SplitRoomName(roomName);

            regionCode = roomCode = null;

            try
            {
                int regionCodeIndex = HasGatePrefix(roomName) ? 1 : 0;

                regionCode = roomInfo[regionCodeIndex];
                roomCode = FormatRoomCode(roomInfo, regionCodeIndex + 1);
            }
            catch (IndexOutOfRangeException)
            {
                Plugin.Logger.LogWarning($"Room name {roomName} is malformated");
            }
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
            return string.Join("_", roomInfo);
        }

        /// <summary>
        /// Processes every index after the first as part of the room code.
        /// </summary>
        public static string FormatRoomCode(string[] roomInfo, int roomCodeStartIndex = 1)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = roomCodeStartIndex; i < roomInfo.Length; i++)
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

        public static string GetShelterCodeWithValidation(string data)
        {
            int lastSepIndex = data.LastIndexOf(':');

            //Either the last data position, or next to last data position is checked for the 'SHELTER' keyword
            bool isShelterData = lastSepIndex != -1 &&
                (data.Substring(lastSepIndex + 1).EndsWith(" SHELTER")
              || data.Substring(0, lastSepIndex).TrimEnd().EndsWith(" SHELTER")); //The whitespace is intentional - room codes cannot have whitespace

            if (isShelterData)
                return data.Substring(0, data.IndexOf(':')).Trim();

            if (lastSepIndex == -1)
            {
                Plugin.Logger.LogInfo("Processed shelter line missing a ':'");
                Plugin.Logger.LogInfo("Line " + data);
            }
            return null;
        }

        public static string GetGateCodeWithValidation(string data)
        {
            if (HasGatePrefix(data))
            {
                string[] regionGateData = data.Split(':');

                if (ContainsGateData(regionGateData))
                    return regionGateData[0].Trim();
            }
            else if (data.EndsWith(" GATE")) //The whitespace is intentional - room codes cannot have whitespace
            {
                return data.Substring(0, data.IndexOf(':')).Trim();
            }
            return null;
        }

        public static bool ContainsGateData(string[] data)
        {
            return HasRoomKeyword(data, "GATE", false); //Gate must have a name, at least one valid connection, and end with GATE
        }

        public static bool HasGatePrefix(string roomName)
        {
            return roomName.StartsWith("GATE");
        }

        public static bool HasRoomKeyword(string[] data, string keyword, bool isConditionalLink)
        {
            if (isConditionalLink)
                return data.Length >= 2 && data[1].Trim() == keyword; //Conditional links store the keyword in the 2nd data position

            if (data.Length < 3) return false; //All room data that has keywords with have it stored at the 3rd data position or later

            for (int i = data.Length - 1; i >= 2; i--)
            {
                if (data[i].Trim() == keyword)
                    return true;
            }
            return false;
        }

        public static AbstractRoom GetAbstractRoomFrom(this World world, IEnumerable<string> roomNames)
        {
            foreach (string roomName in roomNames)
            {
                AbstractRoom room = world.GetAbstractRoom(roomName);

                if (room != null)
                    return room;
            }
            return null;
        }
    }
}
