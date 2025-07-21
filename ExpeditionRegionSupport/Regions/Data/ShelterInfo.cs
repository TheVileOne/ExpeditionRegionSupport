using System.Collections.Generic;

namespace ExpeditionRegionSupport.Regions.Data
{
    public readonly struct ShelterInfo
    {
        public readonly string RoomCode;

        public readonly List<SlugcatStats.Name> BrokenForTheseSlugcats = new List<SlugcatStats.Name>();

        public bool IsBrokenFor(SlugcatStats.Name slugcat) => BrokenForTheseSlugcats.Contains(slugcat);

        public ShelterInfo(string roomCode)
        {
            RoomCode = roomCode.Trim();
            Plugin.Logger.LogInfo("Shelter Info created for " + RoomCode);
        }
    }
}
