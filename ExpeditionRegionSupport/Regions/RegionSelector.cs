using ExpeditionRegionSupport.Filters.Settings;
using ExpeditionRegionSupport.Filters.Utils;
using ExpeditionRegionSupport.Regions.Restrictions;
using MoreSlugcats;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
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
                if (value == activeSlugcat) return;

                activeSlugcat = value;
                InitializeRegionList();
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
        /// A list of custom regions requiring the player to visit (discover a shelter) before becoming available.
        /// </summary>
        public RegionList RestrictedRegions;

        public List<Predicate<string>> ActiveFilters;

        public RegionKey LastRandomRegion;

        public RegionSelector(SlugcatStats.Name activeSlugcat)
        {
            RegionsAvailable = new RegionList();
            RegionsExcluded = new RegionList();
            RegionsFiltered = new RegionList();

            RestrictedRegions = new RegionList();
            ActiveSlugcat = activeSlugcat;
        }

        public void InitializeRegionList()
        {
            RegionsAvailable.Clear();
            RegionsExcluded.Clear();
            RestrictedRegions.Clear();

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

            string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + "regions.txt");

            bool fallbackActive = false;
        fallback:

            /*string[] array = File.ReadAllLines(path);
            foreach (string text in array)
            {
                Plugin.Logger.LogInfo(text);
                string regionCode = text.Trim();

                if (!RegionsExcluded.Contains(regionCode) && !RegionsAvailable.Contains(regionCode) && !handleRestrictedRegion(regionCode))
                {
                    //This should refer to an unrestricted region that is safe to be added to the list.
                    RegionsAvailable.Add(regionCode);
                }
            }*/

            //Modded regions have to be detected by a read from file containing the default optional and story regions.
            //No reason to add optional and story regions from memory if they are going to be read from file.
            if (Plugin.SlugBaseEnabled || !ModManager.ModdedRegionsEnabled || fallbackActive || !File.Exists(path))
            {
                //Story regions are valid as long as they aren't being restricted
                foreach (string regionCode in availableStoryRegions)
                    handleStoryOrOptionalRegion(regionCode, true);

                //Check available optional regions to see if they are unlocked
                foreach (string regionCode in availableOptionalRegions)
                    handleStoryOrOptionalRegion(regionCode, false);

                //Metropolis is not returned as an optional region, but should be handled as one for certain characters.
                if (!handleRestrictedRegion("LC", false))
                    handleOptionalRegion("LC");

                if (ActiveSlugcat == SlugcatStats.Name.Red && !handleRestrictedRegion("OE", false))
                    handleOptionalRegion("OE");

                //SlugBase may not return a full list of available regions. Regions.txt is a more reliable place to get regions.
                if (Plugin.SlugBaseEnabled && ModManager.ModdedRegionsEnabled && File.Exists(path))
                {
                    string[] array = File.ReadAllLines(path);

                    //if (RegionsAvailable.Count >= array.Length) //In a perfect world, this should mean all regions have been processed
                    {
                        foreach (string text in array)
                        {
                            Plugin.Logger.LogInfo(text);
                            string regionCode = text.Trim();

                            if (!RegionsExcluded.Contains(regionCode) && !RegionsAvailable.Contains(regionCode) && !handleRestrictedRegion(regionCode, true))
                            {
                                //This should refer to an unrestricted region that is safe to be added to the list.
                                RegionsAvailable.Add(regionCode);
                            }
                        }
                    }
                }
            }
            else
            {
                //Retrieve regions from file
                foreach (string text in File.ReadAllLines(path))
                {
                    string regionCode = text.Trim();

                    if (handleRestrictedRegion(regionCode, true))
                        continue;

                    //Check available optional regions to see if they are unlocked
                    if (availableOptionalRegions.Contains(regionCode) && handleOptionalRegion(regionCode)) //A mod may add regions to this. It could still return false.
                        continue;

                    if (RainWorld.ShowLogs && RegionUtils.IsCustomRegion(regionCode))
                        Plugin.Logger.LogInfo("Custom Region: " + regionCode);

                    Plugin.Logger.LogInfo(regionCode);
                    RegionsAvailable.Add(regionCode);
                }

                if (RegionsAvailable.Count == 0)
                {
                    Plugin.Logger.LogWarning("Regions.txt file data returned zero valid regions. Using fallback method.");
                    fallbackActive = true;
                    goto fallback;
                }
            }

            if (RainWorld.ShowLogs)
            {
                Plugin.Logger.LogInfo("Active Regions");
                foreach (string region in availableStoryRegions)
                    Plugin.Logger.LogInfo(region);

                Plugin.Logger.LogInfo("Optional Regions");
                foreach (string region in availableOptionalRegions)
                    Plugin.Logger.LogInfo(region);

                Plugin.Logger.LogInfo("Other Regions");

                int logCount = 0;
                foreach (RegionKey regionKey in RegionsAvailable)
                {
                    string region = regionKey.RegionCode;

                    if (!availableStoryRegions.Contains(region) && !availableOptionalRegions.Contains(region))
                    {
                        Plugin.Logger.LogInfo(region);
                        logCount++;
                    }
                }

                if (logCount == 0)
                    Plugin.Logger.LogInfo("NONE");
            }
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
            List<SlugcatStats.Name> unlockedSlugcats = Expedition.ExpeditionGame.unlockedExpeditionSlugcats;

            bool excludeRegion = false;
            bool regionHandled = false;
            if (regionCode == "OE")
            {
                excludeRegion = !unlockedSlugcats.Contains(MoreSlugcatsEnums.SlugcatStatsName.Gourmand);
                regionHandled = true;
            }
            else if (regionCode == "LC")
            {
                excludeRegion = !unlockedSlugcats.Contains(MoreSlugcatsEnums.SlugcatStatsName.Artificer);
                regionHandled = true;
            }
            else if (regionCode == "MS")
            {
                excludeRegion = !unlockedSlugcats.Contains(MoreSlugcatsEnums.SlugcatStatsName.Rivulet);
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
        /// <returns>Region code was handled by the method</returns>
        private bool handleRestrictedRegion(string regionCode, bool regionStatusUnknown)
        {
            bool regionExcluded = applyFilters(regionCode);

            //If we know this, we already know that this is valid region for the active WorldState, and slugcat
            if (!regionExcluded && regionStatusUnknown)
            {
                Plugin.Logger.LogInfo("STATUS UNKNOWN");

                //Prevent invalid regions from being selected based on WorldState
                if (regionCode == "SL") //Shoreline
                {
                    regionExcluded = ModManager.MSC && (ActiveWorldState & WorldState.OldWorld) == 0;
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

                /*
                if (regionCode == "SL")
                {
                    regionExcluded = ActiveSlugcat == MoreSlugcatsEnums.SlugcatStatsName.Spear || ActiveSlugcat == MoreSlugcatsEnums.SlugcatStatsName.Artificer;
                }
                else if (regionCode == "SS") //Five Pebbles
                {
                    regionExcluded = ActiveSlugcat == MoreSlugcatsEnums.SlugcatStatsName.Rivulet || ActiveSlugcat == MoreSlugcatsEnums.SlugcatStatsName.Saint;
                }
                else if (regionCode == "OE")
                {
                    regionExcluded = !ModManager.MSC || ActiveSlugcat != MoreSlugcatsEnums.SlugcatStatsName.Gourmand;
                }
                else if (regionCode == "LC")
                {
                    regionExcluded = !ModManager.MSC || ActiveSlugcat != MoreSlugcatsEnums.SlugcatStatsName.Artificer;
                }
                else if (regionCode == "LM")
                {
                    regionExcluded = !ModManager.MSC || (ActiveSlugcat != MoreSlugcatsEnums.SlugcatStatsName.Spear && ActiveSlugcat != MoreSlugcatsEnums.SlugcatStatsName.Artificer);
                }
                else if (regionCode == "DM")
                {
                    regionExcluded = !ModManager.MSC || ActiveSlugcat != MoreSlugcatsEnums.SlugcatStatsName.Spear;
                }
                else if (regionCode == "RM" || regionCode == "MS")
                {
                    regionExcluded = !ModManager.MSC || ActiveSlugcat != MoreSlugcatsEnums.SlugcatStatsName.Rivulet;
                }
                else if (regionCode == "HR" || regionCode == "CL" || regionCode == "UG")
                {
                    regionExcluded = !ModManager.MSC || ActiveSlugcat != MoreSlugcatsEnums.SlugcatStatsName.Saint;
                }
                else if (regionCode == "DS" || regionCode == "SH" || regionCode == "UW")
                {
                    return !ModManager.MSC || ActiveSlugcat == MoreSlugcatsEnums.SlugcatStatsName.Saint;
                }
                */
            }

            RegionKey regionKey;
            if (!regionExcluded && RestrictedRegions.TryFind(regionCode, out regionKey) && regionKey.IsRegionRestricted)
            {
                regionExcluded = checkRestrictions(regionCode, regionKey.Restrictions, false);
            }

            if (regionExcluded && !RegionsExcluded.Contains(regionCode))
            {
                Plugin.Logger.LogInfo($"Region {regionCode} excluded based on a restriction match");
                RegionsExcluded.Add(regionCode);
            }

            return regionExcluded;
        }

        public void InitializeRestrictions()
        {
            RestrictedRegions.ForEach(r => r.Restrictions.ResetToDefaults());
            RestrictedRegions = RestrictionProcessor.Process();

            if (RainWorld.ShowLogs)
            {
                Plugin.Logger.LogDebug("Restriction Info");
                Plugin.Logger.LogDebug("COUNT: " + RestrictedRegions.Count);

                RestrictedRegions.ForEach(r => r.LogRestrictions());
            }
        }

        public void InitilizeFilters()
        {
            Plugin.Logger.LogInfo("Initializing filters");

            ActiveFilters = new List<Predicate<string>>();

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
                    case FilterOption.NoMSC:
                        filter = new Predicate<string>(RegionUtils.IsMSCRegion);
                        break;
                    case FilterOption.NoCustom:
                        filter = new Predicate<string>(RegionUtils.IsCustomRegion);
                        break;
                    case FilterOption.AllShelters:
                    case FilterOption.SheltersOnly:
                    default:
                        break;
                }

                if (filter != null)
                    ActiveFilters.Add(filter);
            }

            Plugin.Logger.LogInfo($"{ActiveFilters.Count} active filters detected");
        }

        private bool applyFilters(string regionCode)
        {
            //Check every active filter here
            if (ActiveFilters.Exists(filter => Filter.IsFiltered(filter, regionCode)))
            {
                Plugin.Logger.LogInfo($"Region {regionCode} filtered");
                RegionsFiltered.Add(regionCode);
                return true;
            }
            return false;
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
            if (roomCode == null || checkRestrictions(regionCode, roomCode)) return;

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
            //Region cannot be excluded, and must be detectable as a valid region. It also must not be a hardcoded restricted room.
            if (RegionsExcluded.Contains(regionCode) || !RegionsAvailable.Contains(regionCode) || regionCode == "MS" && roomCode == "S07") return true;

            RegionKey regionKey = RestrictedRegions.Find(regionCode);

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
            Plugin.Logger.LogInfo("Checking for restrictions");

            if (restrictions == null)
            {
                Plugin.Logger.LogInfo("No restrictions found");
                return false;
            }

            try
            {
                if (!isRoomCheck && restrictions.WorldState != WorldState.Any)
                {
                    Plugin.Logger.LogInfo("Handling World state restriction");

                    if ((ActiveWorldState & restrictions.WorldState) == 0)
                        return true;
                }

                if (!restrictions.Slugcats.IsEmpty)
                {
                    Plugin.Logger.LogInfo("Handling Slugcat restriction");

                    if (!restrictions.Slugcats.Allowed.Contains(ActiveSlugcat) || restrictions.Slugcats.NotAllowed.Contains(ActiveSlugcat))
                        return true;
                }

                if (restrictions.ProgressionRestriction != ProgressionRequirements.None)
                {
                    Plugin.Logger.LogInfo("Handling Progression restriction");

                    if (restrictions.ProgressionRestriction == ProgressionRequirements.OnVisit)
                    {
                        //Check that player has registered the region in their save data
                        if (!RegionUtils.HasVisitedRegion(ActiveSlugcat, regionCode))
                            return true;
                        
                        /*
                        List<string> visitorRecord;
                        if (RegionUtils.RegionsVisited.TryGetValue(regionCode, out visitorRecord) && visitorRecord.Count == 0)
                        {
                            return true;
                        }
                        */
                    }
                    else if (restrictions.ProgressionRestriction == ProgressionRequirements.CampaignFinish)
                    {
                        //TODO: Implement
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError(ex);
            }

            Plugin.Logger.LogInfo("No restrictions found");
            return false;
        }

        /// <summary>
        /// When valid room spawns are processed, some regions may not have any valid den entries. Remove such areas from AvailableRegions.
        /// </summary>
        public void RemoveEmptyRegions()
        {
            int i = 0;
            while (i < RegionsAvailable.Count)
            {
                Plugin.Logger.LogInfo(RegionsAvailable[i].AvailableRooms.Count);

                if (RegionsAvailable[i].AvailableRooms.Count == 0)
                {
                    Plugin.Logger.LogDebug("Removing region: " + RegionsAvailable[i].RegionCode);
                    RegionsExcluded.Add(RegionsAvailable[i]);
                    RegionsAvailable.RemoveAt(i);
                    i--;
                }
                i++;
            }
        }
    }
}
