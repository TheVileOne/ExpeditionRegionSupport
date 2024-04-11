using System;
using System.Collections.Generic;

namespace ExpeditionRegionSupport.Regions
{
    public readonly struct ShelterInfo
    {
        /// <summary>
        /// A cached list of shelters with conditional slugcat restrictions
        /// </summary>
        public static List<ShelterInfo> BrokenShelters = new List<ShelterInfo>();

        public readonly string RoomCode;

        public readonly List<SlugcatStats.Name> BrokenForTheseSlugcats = new List<SlugcatStats.Name>();

        public bool IsBrokenFor(SlugcatStats.Name slugcat) => BrokenForTheseSlugcats.Contains(slugcat);

        public ShelterInfo(string roomCode)
        {
            RoomCode = roomCode.Trim();
        }
    }
}
