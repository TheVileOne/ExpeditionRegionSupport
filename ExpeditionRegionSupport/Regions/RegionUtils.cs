using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ExpeditionRegionSupport.Regions.Restrictions;

namespace ExpeditionRegionSupport.Regions
{
    public static class RegionUtils
    {
        public static Dictionary<string, List<string>> RegionsVisited => Plugin.CurrentProgression.miscProgressionData.regionsVisited;

        public static bool HasVisitedRegion(SlugcatStats.Name slugcat, string regionCode)
        {
            return RegionsVisited[regionCode].Contains(slugcat.value);
        }

        public static List<string> GetVisitedRegions(SlugcatStats.Name slugcat)
        {
            var enumerator = RegionsVisited.GetEnumerator();

            List<string> visitedRegions = new List<string>();
            while (enumerator.MoveNext())
            {
                string regionCode = enumerator.Current.Key;
                List<string> regionVisitors = enumerator.Current.Value;

                if (regionVisitors.Contains(slugcat.value))
                    visitedRegions.Add(regionCode);
            }

            return visitedRegions;

            /*
            foreach (ConditionalShelterData conditionalShelterData in Plugin.CurrentProgression.miscProgressionData.ConditionalShelterDiscovery)
            {
                if (conditionalShelterData.checkSlugcatIndex(slugcat))
                {
                    string shelterRegion = conditionalShelterData.GetShelterRegion();

                    if (!visitedRegions.Contains(shelterRegion))
                        visitedRegions.Add(shelterRegion);
                }
            }
            return visitedRegions;
            */
        }

        public static WorldState GetWorldStateFromStoryRegions(SlugcatStats.Name name, string[] storyRegions)
        {
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

            Plugin.Logger.LogDebug("Returning " + state.ToString());
            return state;
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
