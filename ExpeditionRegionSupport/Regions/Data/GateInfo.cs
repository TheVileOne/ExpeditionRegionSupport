using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// A list of slugcats that have access to this gate. Empty means any slugcat has access 
        /// </summary>
        public List<SlugcatStats.Name> ConditionalAccess = new List<SlugcatStats.Name>();

        public bool IsActiveFor(SlugcatStats.Name slugcat) => ConditionalAccess.Count == 0 || ConditionalAccess.Contains(slugcat);

        public GateInfo(string roomCode)
        {
            RoomCode = roomCode.Trim();

            string[] gateCodeData = RoomCode.Split('_'); //Expected format: GATE_SI_SL

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
        public string OtherConnection(string regionCode, string adjustedRegionCode)
        {
            if (ConnectingRegions.Item1 == regionCode || ConnectingRegions.Item1 == adjustedRegionCode)
                return ConnectingRegions.Item1;

            if (ConnectingRegions.Item2 == regionCode || ConnectingRegions.Item2 == adjustedRegionCode)
                return ConnectingRegions.Item2;

            return null;
        }
    }
}
