using ExpeditionRegionSupport.Filters.Settings;
using ExpeditionRegionSupport.Filters.Utils;
using ExpeditionRegionSupport.Logging.Utils;
using ExpeditionRegionSupport.Regions.Restrictions;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace ExpeditionRegionSupport.Regions
{
    public class RegionSelector
    {
        public static RegionSelector Instance;

        private SlugcatStats.Name activeSlugcat;
        public SlugcatStats.Name ActiveSlugcat
        {
            get => activeSlugcat;
            set
            {
                if (activeSlugcat == value) return;

                activeSlugcat = value;
                ShouldBuildRegionList = true;
            }
        }

        public WorldState ActiveWorldState;

        /// <summary>
        /// A list of regions available for selection
        /// </summary>
        public RegionList RegionsAvailable;

        /// <summary>
        /// A list of regions unavailable for selection
        /// </summary>
        public RegionList RegionsExcluded;

        public RegionList RegionsFiltered;

        /// <summary>
        /// A list of regions that contain active restrictions
        /// </summary>
        public RegionList RegionsRestricted;

        public List<Predicate<string>> ActiveFilters;

        /// <summary>
        /// A list of restrictions added through code mods
        /// </summary>
        public List<RestrictionCheck> ActiveRestrictionChecks = new List<RestrictionCheck>();

        /// <summary>
        /// A list of slugcats that meet an unlock condition for the active game mode. (Expedition is the only supported game mode so far)
        /// </summary>
        public List<SlugcatStats.Name> UnlockedSlugcats = new List<SlugcatStats.Name>();

        /// <summary>
        /// A flag that tells the selector to add shelters as valid player spawns for all custom regions
        /// </summary>
        public bool IncludeCustomShelters;

        public bool ShouldBuildRegionList = true;

        /// <summary>
        /// A flag that controls whether a slugcat can spawn in a region equivalent version of another region that does not match the region version for that slugcat
        /// </summary>
        public bool EnforceRegionEquivalencies = true;

        public RegionKey LastRandomRegion;

        public RegionSelector(SlugcatStats.Name activeSlugcat)
        {
            RegionsAvailable = new RegionList();
            RegionsExcluded = new RegionList();
            RegionsFiltered = new RegionList();

            RegionsRestricted = new RegionList();
            ActiveSlugcat = activeSlugcat;
        }

        public void InitializeRegionList()
        {
            RegionsAvailable.Clear();
            RegionsExcluded.Clear();
            RegionsRestricted.Clear();

            try
            {
                InitializeRestrictions();
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError(ex);
            }

            InitilizeFilters();

            //These methods are already curated to the provided slugcat, and account for MSC being enabled
            List<string> availableStoryRegions = SlugcatStats.SlugcatStoryRegions(ActiveSlugcat);
            List<string> availableOptionalRegions = SlugcatStats.SlugcatOptionalRegions(ActiveSlugcat);

            ActiveWorldState = RegionUtils.GetWorldStateFromStoryRegions(ActiveSlugcat, availableStoryRegions);

            //Story regions are valid as long as they aren't being restricted
            foreach (string regionCode in availableStoryRegions)
                handleStoryOrOptionalRegion(regionCode, true);

            //Check available optional regions to see if they are unlocked
            foreach (string regionCode in availableOptionalRegions)
                handleStoryOrOptionalRegion(regionCode, false);

            //Metropolis is not returned as an optional region, but should be handled as one for certain characters
            if (!handleRestrictedRegion("LC", false))
                handleOptionalRegion("LC");

            if (ActiveSlugcat == SlugcatStats.Name.Red && !handleRestrictedRegion("OE", false))
                handleOptionalRegion("OE");

            if (ModManager.ModdedRegionsEnabled)
            {
                string[] regions = RegionUtils.GetAllRegions();

                foreach (string regionCode in regions)
                {
                    if (!RegionsExcluded.Contains(regionCode))
                    {
                        if (!RegionsAvailable.Contains(regionCode))
                        {
                            if (handleRestrictedRegion(regionCode, true))
                                continue;

                            //This should refer to an unrestricted region that is safe to be added to the list
                            RegionsAvailable.Add(regionCode);
                        }

                        if (IncludeCustomShelters && RegionUtils.IsCustomRegion(regionCode))
                        {
                            Plugin.Logger.LogInfo("Finding shelters for region " + regionCode);

                            //Find all custom shelters that aren't broken for this slugcat
                            IEnumerable<string> customShelters = RegionUtils.GetShelters(regionCode)
                                .Where(s => !s.IsBrokenFor(ActiveSlugcat))
                                .Select(s => s.RoomCode);

                            int shelterCount = 0;
                            foreach (string shelter in customShelters)
                            {
                                if (Plugin.DebugMode)
                                    Plugin.Logger.LogInfo(shelter);
                                AddRoom(shelter);
                                shelterCount++;
                            }
                            Plugin.Logger.LogInfo(shelterCount + " shelters found");
                        }
                    }
                }
            }

            if (Plugin.DebugMode || RainWorld.ShowLogs)
            {
                StringHandler logBuffer = new StringHandler();

                //Log every region available for selection. Some regions may be empty and will be removed later in the selection process
                logBuffer.AddString("Active Regions");
                logBuffer.AddFrom(availableStoryRegions.Where(r => !RegionsExcluded.Contains(r)));

                logBuffer.AddString("Optional Regions");
                logBuffer.AddFrom(availableOptionalRegions.Where(r => !RegionsExcluded.Contains(r)));

                logBuffer.AddString("Other Regions");

                bool hasOtherRegions = false;
                foreach (RegionKey regionKey in RegionsAvailable)
                {
                    string region = regionKey.RegionCode;
                    if (!availableStoryRegions.Contains(region) && !availableOptionalRegions.Contains(region))
                    {
                        logBuffer.AddString(region);
                        hasOtherRegions = true;
                    }
                }

                if (!hasOtherRegions)
                    logBuffer.AddString("NONE");

                Plugin.Logger.LogInfo(logBuffer.ToString());
            }

            ShouldBuildRegionList = false;
        }

        private void handleStoryOrOptionalRegion(string regionCode, bool storyRegion)
        {
            if (handleRestrictedRegion(regionCode, false)) return; //Do not process region if it is restricted

            //Unrestricted story regions are always valid
            if (storyRegion)
            {
                RegionsAvailable.Add(regionCode);
                return;
            }

            //Check to see if optional region is unlocked. Region code is expected to be handled within the method call in this context 
            handleOptionalRegion(regionCode);
        }

        /// <summary>
        /// Check for and handle optional regions based on whether certain slugcats are unlocked in Expedition.
        /// </summary>
        /// <returns>Region code was handled by the method</returns>
        private bool handleOptionalRegion(string regionCode)
        {
            bool excludeRegion = false;
            bool regionHandled = false;
            if (regionCode == "OE")
            {
                excludeRegion = !UnlockedSlugcats.Contains(MoreSlugcatsEnums.SlugcatStatsName.Gourmand);
                regionHandled = true;
            }
            else if (regionCode == "LC")
            {
                excludeRegion = !UnlockedSlugcats.Contains(MoreSlugcatsEnums.SlugcatStatsName.Artificer);
                regionHandled = true;
            }
            else if (regionCode == "MS")
            {
                excludeRegion = !UnlockedSlugcats.Contains(MoreSlugcatsEnums.SlugcatStatsName.Rivulet);
                regionHandled = true;
            }

            if (regionHandled)
            {
                if (excludeRegion)
                    RegionsExcluded.Add(regionCode);
                else
                    RegionsAvailable.Add(regionCode);
            }

            return regionHandled;
        }

        /// <summary>
        /// Certain regions are specific to a world state. Room codes related to those regions, as well as room codes with other
        /// restrictions applied are handled here. Filter checks are also handled here.
        /// </summary>
        /// <param name="regionCode">The region code to check</param>
        /// <param name="regionStatusUnknown">Whether or not we know the region code is a story/optional region</param>
        /// <returns>Region code was handled by the method</returns>
        private bool handleRestrictedRegion(string regionCode, bool regionStatusUnknown)
        {
            bool regionExcluded = false,
                 regionFiltered = false;

            if (applyFilters(regionCode)) //Apply any region filters
            {
                regionExcluded = true;
                regionFiltered = true;
            }
            else if (applyRestrictionChecks(regionCode)) //Apply any code-enabled restrictions
            {
                regionExcluded = true;
            }
            else if (regionStatusUnknown && applyWorldStateChecks(regionCode)) //Check that active WorldState is consistent with region 
            {
                //Known story and optional regions have already been handled when this check runs 
                regionExcluded = true;
            }
            else if (RegionsRestricted.TryFind(regionCode, out RegionKey regionKey) && regionKey.IsRegionRestricted)
            {
                //Check region restrictions handled by RestrictionProcessor (read from file)
                regionExcluded = checkRestrictions(regionCode, regionKey.Restrictions, false);
            }

            if (!regionExcluded)
            {
                //Modcats need an equivalency check for every region, other characters need one for custom regions only
                bool shouldEnforceRegionEquivalencies = EnforceRegionEquivalencies
                    && (SlugcatUtils.IsModcat(ActiveSlugcat) || (ModManager.ModdedRegionsEnabled && RegionUtils.IsCustomRegion(regionCode)));

                if (shouldEnforceRegionEquivalencies)
                {
                    //Disallows slugcats from spawning in alternate versions of regions unless they are allowed to spawn in that region
                    regionExcluded = regionCode != RegionUtils.GetSlugcatEquivalentRegion(regionCode, ActiveSlugcat);
                }
            }

            if (regionExcluded && !RegionsExcluded.Contains(regionCode))
            {
                Plugin.Logger.LogInfo($"Region {regionCode} excluded based on a {(regionFiltered ? "filter" : "restriction")} match");
                RegionsExcluded.Add(regionCode);
            }

            return regionExcluded;
        }

        public void InitializeRestrictions()
        {
            Plugin.Logger.LogInfo("Checking for restrictions");
            Plugin.Logger.LogInfo($"Detected {ActiveRestrictionChecks.Count} restriction checks");

            RegionsRestricted.ForEach(r => r.Restrictions.ResetToDefaults());
            RegionsRestricted = RestrictionProcessor.Process();

            if (Plugin.DebugMode)
            {
                Plugin.Logger.LogDebug("Restriction Info");
                Plugin.Logger.LogDebug("COUNT: " + RegionsRestricted.Count);

                RegionsRestricted.ForEach(r => r.LogRestrictions());
            }
        }

        public void InitilizeFilters()
        {
            Plugin.Logger.LogInfo("Checking for filters");

            ActiveFilters = new List<Predicate<string>>();
            IncludeCustomShelters = false;

            List<FilterOption> activeFilterOptions = RegionFilterSettings.GetActiveFilters();

            foreach (FilterOption filterOption in activeFilterOptions)
            {
                Predicate<string> filter = null;
                switch (filterOption)
                {
                    case FilterOption.VisitedRegionsOnly:
                        filter = new Predicate<string>((r) =>
                        {
                            return !RegionUtils.HasVisitedRegion(ActiveSlugcat, r);
                        });
                        break;
                    case FilterOption.NoVanilla:
                        filter = new Predicate<string>(RegionUtils.IsVanillaRegion);
                        break;
                    case FilterOption.NoDownpour:
                        filter = new Predicate<string>(RegionUtils.IsDownpourRegion);
                        break;
                    case FilterOption.NoCustom:
                        filter = new Predicate<string>(RegionUtils.IsCustomRegion);
                        break;
                    case FilterOption.InheritCustomShelters:
                        IncludeCustomShelters = true;
                        break;
                    default:
                        break;
                }

                if (filter != null)
                    ActiveFilters.Add(filter);
            }

            Plugin.Logger.LogInfo($"{ActiveFilters.Count} active filters detected");
        }

        /// <summary>
        /// Checks active filters applied to regions
        /// </summary>
        /// <param name="regionCode">The region code to check</param>
        /// <returns>Whether the region code should be excluded</returns>
        private bool applyFilters(string regionCode)
        {
            //Check every active filter here
            if (ActiveFilters.Exists(filter => Filter.IsFiltered(filter, regionCode)))
            {
                RegionsFiltered.Add(regionCode);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks code applied conditional restrictions applied to regions. This list would be accessed and used by other code mods defining custom restriction conditions.
        /// </summary>
        /// <param name="regionCode">The region code to check</param>
        /// <returns>Whether the region code should be excluded</returns>
        private bool applyRestrictionChecks(string regionCode)
        {
            //All checks for a given region must pass or region will be excluded
            return ActiveRestrictionChecks.Exists(r => r.RegionCode == regionCode && !r.CheckRegion());
        }

        /// <summary>
        /// Check that the region code belongs to the active WorldState. This mainly compares differences between MSC character regions
        /// </summary>
        /// <param name="regionCode">The region code to check</param>
        /// <returns>Whether the region code should be excluded</returns>
        private bool applyWorldStateChecks(string regionCode)
        {
            bool regionExcluded = false;

            if (regionCode == "SL") //Shoreline
            {
                regionExcluded = ModManager.MSC && (ActiveWorldState & WorldState.OldWorld) != 0;
            }
            else if (regionCode == "SS") //Five Pebbles
            {
                regionExcluded = ModManager.MSC && (ActiveWorldState & (WorldState.Rivulet | WorldState.Saint)) != 0; //Rivulet has The Rot. Saint has Silent Construct.
            }
            else if (regionCode == "OE") //Outer Expanse
            {
                regionExcluded = !ModManager.MSC || (ActiveWorldState & WorldState.Gourmand) == 0;
            }
            else if (regionCode == "LC") //Metropolis
            {
                regionExcluded = !ModManager.MSC || (ActiveWorldState & WorldState.Artificer) == 0;
            }
            else if (regionCode == "LM") //Waterfront Facility
            {
                regionExcluded = !ModManager.MSC || (ActiveWorldState & WorldState.OldWorld) == 0;
            }
            else if (regionCode == "DM") //Looks to the Moon (Spearmaster)
            {
                regionExcluded = !ModManager.MSC || (ActiveWorldState & WorldState.SpearMaster) == 0;
            }
            else if (regionCode == "RM" || regionCode == "MS") //The Rot, Submerged Superstructure
            {
                regionExcluded = !ModManager.MSC || (ActiveWorldState & WorldState.Rivulet) == 0;
            }
            else if (regionCode == "HR" || regionCode == "CL" || regionCode == "UG") //Rubicon, Silent Construct, Undergrowth
            {
                regionExcluded = !ModManager.MSC || (ActiveWorldState & WorldState.Saint) == 0;
            }
            else if (regionCode == "DS" || regionCode == "SH" || regionCode == "UW") //Drainage Systems, Shaded Citadel, The Exterior
            {
                regionExcluded = !ModManager.MSC || (ActiveWorldState & WorldState.Saint) != 0;
            }

            //GameFeatures.WorldState.TryGet(player, out SlugcatStats.Name[] characters)
            return regionExcluded;
        }

        public RegionKey RandomRegion()
        {
            if (RegionsAvailable.Count == 0) return default; //This state will crash the game if we try to process it

            //No region should be selected twice in a row. Remove the last region chosen, and add it back to the list after a region is chosen.
            int regionIndex = -1;
            RegionKey regionBackup = default;
            if (!LastRandomRegion.IsEmpty && RegionsAvailable.Count > 1)
            {
                regionIndex = RegionsAvailable.IndexOf(LastRandomRegion);

                if (regionIndex >= 0)
                {
                    regionBackup = RegionsAvailable[regionIndex];
                    RegionsAvailable.RemoveAt(regionIndex);
                }
            }

            LastRandomRegion = RegionsAvailable[Random.Range(1, 10000) % RegionsAvailable.Count]; //Return a random result based on a modular index.

            if (!regionBackup.IsEmpty)
                RegionsAvailable.Insert(regionIndex, regionBackup);

            Plugin.Logger.LogInfo("Region selected: " + LastRandomRegion.RegionCode);
            return LastRandomRegion;
        }

        public string RandomRoom()
        {
            /*RegionKey regionKey = default;
            for (int i = 0; i < 20; i++)
            {
                regionKey = RandomRegion();
                string room = RandomRoom(regionKey);

                Mod.Logger.LogInfo(regionKey.RegionCode);
                Mod.Logger.LogInfo(room);
            }

            for (int i = 0; i < 1000; i++)
            {
                int testRandom = Random.Range(1, 10000) % RegionsAvailable.Count;
                Mod.Logger.LogInfo(testRandom);
            }*/

            return RandomRoom(RandomRegion());
        }

        public string RandomRoom(RegionKey region)
        {
            if (region.IsEmpty || region.AvailableRooms.Count == 0) return string.Empty; //This state will crash the game if we try to process it

            return RegionUtils.FormatRoomName(region.RegionCode, region.AvailableRooms[Random.Range(1, 10000) % region.AvailableRooms.Count]); //Return a random result based on a modular index.
        }

        public void AddRoom(string roomName)
        {
            string regionCode, roomCode;
            RegionUtils.ParseRoomName(roomName, out regionCode, out roomCode);

            //Check for valid format, and check for restrictions that might apply to this room
            if (roomCode == null || checkRestrictions(regionCode, roomCode))
            {
                Plugin.Logger.LogInfo($"Room {roomName} is not available due to a restriction match");
                return;
            }

            RegionKey regionKey;
            if (RegionsAvailable.TryFind(regionCode, out regionKey)) //Retrieve region associated with room
            {
                regionKey.AvailableRooms.Add(roomCode);
            }
            else //We should not add new regions here. It will not be loadable, or potentially be an unprocessed excluded region.
            {
                Plugin.Logger.LogInfo($"Room {roomName} is not available. Region doesn't exist.");
            }
        }

        /// <summary>
        /// Check if room info is restricted, and should not be used
        /// </summary>
        /// /// <returns>True, if room should not be used</returns>
        private bool checkRestrictions(string regionCode, string roomCode)
        {
            //Region cannot be excluded, and must be detectable as a valid region
            if (RegionsExcluded.Contains(regionCode) || !RegionsAvailable.Contains(regionCode)) return true;

            RegionKey regionKey = RegionsRestricted.Find(regionCode);

            //Region-specific restrictions have already been processed at this point. Check for room restrictions. 
            return checkRestrictions(regionCode, regionKey.GetRoomRestrictions(roomCode, false), true);
        }

        /// <summary>
        /// Check if a room, or region is restricted, and should not be used
        /// </summary>
        /// <param name="isRoomCheck">Whether restrictions belong to a room or region</param>
        /// <returns>True, if room or region should not be used</returns>
        private bool checkRestrictions(string regionCode, RegionRestrictions restrictions, bool isRoomCheck)
        {
            StringHandler logBuffer = new StringHandler();

            logBuffer.AddString("Checking for restrictions");

            try
            {
                if (restrictions == null)
                {
                    logBuffer.AddString("No restrictions found");
                    return false;
                }

                try
                {
                    if (restrictions.WorldState != WorldState.Any)
                    {
                        logBuffer.AddString("Handling World State restriction");

                        //Checks that WorldState restrictions include the current WorldState
                        if ((ActiveWorldState & restrictions.WorldState) == 0)
                            return true;
                    }

                    if (!restrictions.Slugcats.IsEmpty)
                    {
                        logBuffer.AddString("Handling Slugcat restriction");

                        if ((restrictions.Slugcats.Allowed.Count > 0 && !restrictions.Slugcats.Allowed.Contains(ActiveSlugcat))
                          || restrictions.Slugcats.NotAllowed.Contains(ActiveSlugcat))
                            return true;
                    }

                    if (restrictions.ProgressionRestriction != ProgressionRequirements.None)
                    {
                        logBuffer.AddString("Handling Progression restriction");

                        if (restrictions.ProgressionRestriction == ProgressionRequirements.OnVisit)
                        {
                            //Check that player has registered the region in their save data
                            if (!RegionUtils.HasVisitedRegion(ActiveSlugcat, regionCode))
                                return true;
                        }
                        else if (restrictions.ProgressionRestriction == ProgressionRequirements.OnSlugcatUnlocked)
                        {
                            if (!restrictions.Slugcats.UnlockRequired.TrueForAll(UnlockedSlugcats.Contains))
                                return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Logger.LogError(ex);
                }

                logBuffer.AddString("No restrictions found");
                return false;
            }
            finally
            {
                if (Plugin.DebugMode)
                    Plugin.Logger.LogInfo(logBuffer.ToString());
            }
        }

        /// <summary>
        /// When valid room spawns are processed, some regions may not have any valid den entries. Remove such areas from AvailableRegions.
        /// </summary>
        public void RemoveEmptyRegions()
        {
            int i = 0;
            while (i < RegionsAvailable.Count)
            {
                if (RegionsAvailable[i].AvailableRooms.Count == 0)
                {
                    Plugin.Logger.LogInfo("Removing empty region: " + RegionsAvailable[i].RegionCode);
                    RegionsExcluded.Add(RegionsAvailable[i]);
                    RegionsAvailable.RemoveAt(i);
                    i--;
                }
                i++;
            }
        }
    }
}
