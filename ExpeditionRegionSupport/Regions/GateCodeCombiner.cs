using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Regions
{
    public static class GateCodeCombiner
    {
        /// <summary>
        /// Formats all possible gate room combinations formed between two collections of strings
        /// </summary>
        public static IEnumerable<string> GetCombinations(IEnumerable<string> codePartsA, IEnumerable<string> codePartsB)
        {
            string[] roomInfo = new string[3];

            roomInfo[0] = "GATE";

            //Check every combination beginning with entries in the first collection in the first index, and then later as the second index
            foreach (string roomName in gateRoomCombinationHelper(roomInfo, codePartsA, codePartsB))
            {
                yield return roomName;
            }

            foreach (string roomName in gateRoomCombinationHelper(roomInfo, codePartsB, codePartsA))
            {
                yield return roomName;
            }
        }

        private static IEnumerable<string> gateRoomCombinationHelper(string[] roomInfo, IEnumerable<string> regionPartsA, IEnumerable<string> regionPartsB)
        {
            foreach (string regionCodeA in regionPartsA)
            {
                roomInfo[1] = regionCodeA;
                foreach (string regionCodeB in regionPartsB)
                {
                    roomInfo[2] = regionCodeB;
                    yield return RegionUtils.FormatRoomName(roomInfo);
                }
            }
        }
    }
}
