﻿using System.Collections.Generic;
using System.Linq;

namespace ExpeditionRegionSupport.Regions.Restrictions
{
    public partial class RegionRestrictions
    {
        /// <summary>
        /// Merge restrictions betweem a given RegionRestricitons object and 'this'
        /// Assumes that the region code is a match
        /// </summary>
        public void MergeValues(RegionRestrictions r)
        {
            if (!r.HasEntries)
            {
                Plugin.Logger.LogWarning("EMPTY set detected");
                return;
            }

            Plugin.Logger.LogInfo("Restrictions already exist. Attempting to merge");

            MergeUtils.MergeRoomRestrictions(this, r);
            ProcessDuplicateRestrictions();

            MergeUtils.MergeValues(this, r);
        }
        public void ProcessDuplicateRestrictions()
        {
            int i = 0;
            while (i < RoomRestrictions.Count)
            {
                RoomRestrictions current = RoomRestrictions[i];

                if (current.IsEmpty)
                {
                    Plugin.Logger.LogInfo("Removing restrictions with no targeted rooms");
                    RoomRestrictions.RemoveAt(i);
                    i--;
                }

                //All lists before the current index must be checked against the current index
                for (int j = 0; j < i; j++)
                {
                    RoomRestrictions compareTarget = RoomRestrictions[j];

                    //Compare to each list already processed for matching data. Needs to be stored in a new list to avoid interfering with the enumerator
                    IEnumerable<string> duplicateRooms = new List<string>(current.Rooms.Intersect(compareTarget.Rooms));

                    if (duplicateRooms.Count() > 0)
                    {
                        Plugin.Logger.LogInfo("Duplicate restrictions detected");

                        RoomRestrictions existingRestrictions = FindRestrictionMatch(Restrictions.RoomRestrictions.GetRestrictionsByIndex(compareTarget, this, j), current.Restrictions); //Find an existing RoomRestrictions object with the same restrictions if it exists

                        RoomRestrictions restrictionsTarget; //The place duplicate rooms will be stored

                        List<string> listToProcess;
                        List<string> listToProcess2 = null;

                        //All duplicate entries can be moved into an existing object
                        if (existingRestrictions != null)
                        {
                            restrictionsTarget = existingRestrictions;

                            //The list that gets duplicate entries removed will be the list that doesn't match
                            if (existingRestrictions == compareTarget)
                            {
                                listToProcess = current.Rooms;
                            }
                            else if (existingRestrictions == current)
                            {
                                listToProcess = compareTarget.Rooms;
                            }
                            else //Neither list match anymore
                            {
                                listToProcess = current.Rooms;
                                listToProcess2 = compareTarget.Rooms;
                            }
                        }
                        else //This indicates there is a restriction mismatch. Create a new target and insert merged changes into it
                        {
                            listToProcess = current.Rooms;
                            listToProcess2 = compareTarget.Rooms;

                            restrictionsTarget = new RoomRestrictions(current.RegionCode);
                            restrictionsTarget.Restrictions = MergeUtils.GetMergedValues(Restrictions.RoomRestrictions.GetRestrictionsByIndex(compareTarget, this, j), current.Restrictions);
                            RoomRestrictions.Insert(i, restrictionsTarget);
                        }

                        //Remove rooms that exist in either RoomRestrictions list in order to add them to their destination target
                        foreach (string entry in duplicateRooms)
                        {
                            listToProcess.Remove(entry);
                            listToProcess2?.Remove(entry);

                            restrictionsTarget.AddRoom(entry);
                        }
                    }
                }
                i++;
            }
        }

        /// <summary>
        /// Find a non-inheriting match for given restrictions
        /// </summary>
        public RoomRestrictions FindRestrictionMatch(RegionRestrictions restrictions)
        {
            return RoomRestrictions.Find(r => r.Restrictions.CompareWithoutRoomRestrictions(restrictions));
        }

        /// <summary>
        /// Find a non-inheriting match for given restrictions
        /// </summary>
        public RoomRestrictions FindRestrictionMatch(RegionRestrictions r1, RegionRestrictions r2)
        {
            //The output is the merge result between the two restrictions sets
            RegionRestrictions output = MergeUtils.GetMergedValues(r1, r2);

            return FindRestrictionMatch(output); //Find an existing RoomRestrictions object with the same restrictions if it exists
        }
    }
}
