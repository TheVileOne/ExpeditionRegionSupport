using ExpeditionRegionSupport.Regions;
using ExpeditionRegionSupport.Regions.Data;
using ExpeditionRegionSupport.Tools;
using System;
using System.Collections.Generic;

namespace ExpeditionRegionSupport
{
    public static class DebugMethods
    {
        public static void ProcessAllAccessibleRegions(SlugcatStats.Name slugcat)
        {
            DebugTimer fullProcessTimer = new DebugTimer();

            fullProcessTimer.Start();

            RegionsCache largestRegionCache = null; //Stores the biggest cache by size
            foreach (string regionCode in RegionUtils.GetAllRegions())
            {
                if (largestRegionCache != null)
                    RegionUtils.RegionAccessibilityCache = largestRegionCache;

                Plugin.Logger.LogDebug("REGION " + regionCode);
                Plugin.Logger.LogDebug("ACCESSIBILITY LIST");

                DebugTimer processTimer = new DebugTimer();

                processTimer.Start();

                List<string> accessibleRegions = RegionUtils.GetAccessibleRegions(regionCode, slugcat);

                if (largestRegionCache == null || largestRegionCache.Regions.Count < RegionUtils.RegionAccessibilityCache.Regions.Count)
                {
                    Plugin.Logger.LogDebug("New largest cache detected");
                    largestRegionCache = RegionUtils.RegionAccessibilityCache;
                }

                string reportString = accessibleRegions.Count > 0 ? accessibleRegions.FormatToString(',') : "NONE";
                Plugin.Logger.LogDebug(reportString);

                processTimer.ReportTime();
                processTimer.Stop();
            }

            fullProcessTimer.ReportTime("Entire process");
            fullProcessTimer.Stop();
        }

        public static void TestRegionMiner()
        {
            var logger = Plugin.Logger;

            RegionDataMiner regionMiner = new RegionDataMiner();

            IEnumerable<string> roomData_SI = regionMiner.GetRoomLines("SI");

            logger.LogInfo(string.Empty);
            logger.LogInfo("Showing ROOM LINES");
            logger.LogInfo(string.Empty);

            var enumerator = roomData_SI.GetEnumerator();

            enumerator.MoveNext();
            string line1 = enumerator.Current;
            enumerator.MoveNext();
            string line2 = enumerator.Current;
            enumerator.MoveNext();
            string line3 = enumerator.Current;

            logger.LogInfo(line1 + " " + line2 + " " + line3);

            //foreach (string roomData in roomData_SI)
            //    Logger.LogInfo(roomData);

            IEnumerable<string> roomData_HI = regionMiner.GetRoomLines("HI");

            logger.LogInfo(string.Empty);
            logger.LogInfo("Showing ROOM LINES");
            logger.LogInfo(string.Empty);
            foreach (string roomData in roomData_HI)
                logger.LogInfo(roomData);

            //Print again to confirm that stream closes after use
            logger.LogInfo(string.Empty);
            logger.LogInfo("Showing ROOM LINES");
            logger.LogInfo(string.Empty);
            roomData_SI = regionMiner.GetRoomLines("SI");

            foreach (string roomData in roomData_SI)
                logger.LogInfo(roomData);
        }
    }
}
