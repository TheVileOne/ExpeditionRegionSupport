using System;
using System.Collections.Generic;

namespace ExpeditionRegionSupport.Regions.Data
{
    public class GateInfo
    {
        public readonly string RoomCode;

        /// <summary>
        /// The regions that this gate connects to in no specific order
        /// </summary>
        public ValueTuple<string, string> ConnectingRegions;

        /// <summary>
        /// The string representation of the karma access requirements (as accessed from same order as seen in ConnectingRegions) 
        /// </summary>
        public ValueTuple<string, string> KarmaRequirement;

        /// <summary>
        /// A list of slugcat timelines that have access to this gate. Empty means any slugcat has access 
        /// </summary>
        public List<SlugcatStats.Name> ConditionalAccess = new List<SlugcatStats.Name>();

        public bool IsActiveFor(SlugcatStats.Name slugcat) => ConditionalAccess.Count == 0 || ConditionalAccess.Contains(slugcat);

        public GateInfo(string roomCode)
        {
            RoomCode = roomCode.Trim();

            string[] gateCodeData = RegionUtils.SplitRoomName(RoomCode); //Expected format: GATE_SI_SL

            if (gateCodeData.Length < 3)
            {
                Plugin.Logger.LogWarning($"Invalid region gate format detected [{RoomCode}]");
                return;
            }

            ConnectingRegions = new ValueTuple<string, string>(gateCodeData[1], gateCodeData[2]);
        }

        /// <summary>
        /// Returns the region code connection that does not match the given region code. If neither code matches, this method returns null
        /// </summary>
        public string OtherConnection(string baseRegionCode, string actualRegionCode)
        {
            if (ConnectingRegions.Item1 == baseRegionCode || ConnectingRegions.Item1 == actualRegionCode)
                return ConnectingRegions.Item2;

            if (ConnectingRegions.Item2 == baseRegionCode || ConnectingRegions.Item2 == actualRegionCode)
                return ConnectingRegions.Item1;

            return null;
        }

        public override string ToString()
        {
            return RoomCode;
        }
    }
}
