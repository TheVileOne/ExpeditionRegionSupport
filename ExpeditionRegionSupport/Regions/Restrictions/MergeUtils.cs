using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Regions.Restrictions
{
    public static class MergeUtils
    {
        public static void MergeValues(RegionRestrictions r1, RegionRestrictions r2)
        {
            applyValues(r1, r2, ref r1);
        }

        public static RegionRestrictions GetMergedValues(RegionRestrictions r1, RegionRestrictions r2)
        {
            RegionRestrictions output = new RegionRestrictions();

            applyValues(r1, r2, ref output);
            return output;
        }

        /// <summary>
        /// Compares restriction fields of two RegionRestrictions objects, and stores the merged output in a third object.
        /// Values in the RegionRestricitons passed by ref will be ignored and overwritten. 
        /// </summary>
        private static void applyValues(RegionRestrictions r1, RegionRestrictions r2, ref RegionRestrictions target)
        {
            //This method will assume that target is either empty or equal to r1. Enforce this.
            if (target == r2)
            {
                RegionRestrictions temp = r1;
                r1 = r2;
                r2 = temp;
            }
            else if (target != r1)
                target.InheritValues(r1, true);

            if (r1.WorldState != r2.WorldState)
            {
                if (r1.WorldState == WorldState.Any)
                    target.WorldState = r2.WorldState;
                else if (r2.WorldState != WorldState.Any) //Don't allow a modify command to clear restrictions already in place
                    target.WorldState |= r2.WorldState;

                Plugin.Logger.LogInfo("WORLDSTATE updated");
            }

            if (r1.ProgressionRestriction != r2.ProgressionRestriction)
            {
                if (r1.ProgressionRestriction == ProgressionRequirements.None)
                    target.ProgressionRestriction = r2.ProgressionRestriction;

                Plugin.Logger.LogInfo("Progression Requirement updated");
            }

            target.Slugcats.MergeValues(r2.Slugcats);
        }

        public static void MergeRoomRestrictions(RegionRestrictions r1, RegionRestrictions r2)
        {
            bool restrictionMatch = r1.CompareWithoutRoomRestrictions(r2);

            Plugin.Logger.LogInfo("Restrictions target specific rooms. Checking compatibility");
            Plugin.Logger.LogInfo(restrictionMatch ? "Restrictions fields match" : "Restrictions fields mismatch");
            Plugin.Logger.LogInfo("1. " + (r1.IsRoomSpecific ? "HAS INHERITS" : "NO INHERITS"));
            Plugin.Logger.LogInfo("2. " + (r2.IsRoomSpecific ? "HAS INHERITS" : "NO INHERITS"));

            if (restrictionMatch) //Every restriction field shares the same values
            {
                if (r1.IsRoomSpecific) //At least one room is inheriting restrictions stored in 'r1'
                {
                    if (!r2.IsRoomSpecific) //The merge candidate has region-wide restrictions
                    {
                        //'this' will inherit the region-wide restrictions of 'r2'
                        r1.RoomRestrictions.RemoveAll(r => r.InheritRestrictionsFromParent);
                    }

                    //Search for a merge compatible place to store incoming restricted room data
                    foreach (RoomRestrictions rs in r2.RoomRestrictions)
                        processMerge(r1, rs, r2);
                }
                else //There are no rooms inheriting restrictions stored in 'r1'
                {
                    foreach (RoomRestrictions rs in r2.RoomRestrictions)
                    {
                        //Inherited rooms can be ignored here (because restrictions apply to the entire region)
                        if (rs.InheritRestrictionsFromParent)
                            continue;

                        processMerge(r1, rs, r2);
                    }
                }
            }
            else //At least one restriction field does not match
            {
                if (r1.IsRoomSpecific)  //At least one room is inheriting restrictions stored in 'r1'
                {
                    if (!r2.IsRoomSpecific) //The merge candidate has region-wide restrictions
                    {
                        r1.RoomRestrictions.ForEach(rs => rs.Restrictions.InheritValues(r1)); //The restrictions are passed down to the room level

                        //Search for a merge compatible place to store incoming restricted room data
                        foreach (RoomRestrictions rs in r2.RoomRestrictions)
                            processMerge(r1, rs, r2);
                    }
                    else //Both sides inherit restrictions
                    {
                        foreach (RoomRestrictions rs in r2.RoomRestrictions)
                        {
                            rs.ReplaceInheritence(r2); //The restrictions are passed down to the room level
                            processMerge(r1, rs, r2);
                        }
                    }
                }
                else //There are no rooms inheriting restrictions stored in 'r1'
                {
                    Plugin.Logger.LogDebug("DEBUG SECTION");
                    r2.LogData();

                    foreach (RoomRestrictions rs in r2.RoomRestrictions)
                    {
                        Plugin.Logger.LogDebug("SLUGCATS PRIMARY");
                        foreach (var name in r1.Slugcats.Allowed)
                        {
                            Plugin.Logger.LogDebug("[MERGE PRIMARY 1] " + name);
                        }
                        foreach (var name in r2.Slugcats.Allowed)
                        {
                            Plugin.Logger.LogDebug("[MERGE PRIMARY 2] " + name);
                        }
                        rs.ReplaceInheritence(r2); //The restrictions are passed down to the room level
                        processMerge(r1, rs, r2);
                    }

                    Plugin.Logger.LogDebug("DEBUG SECTION END");
                }
            }

            BringInheritsToFront(r1);
        }

        /// <summary>
        /// Make sure that only one RoomRestrictions inherits from parent, and that object is first in the list
        /// </summary>
        public static void BringInheritsToFront(RegionRestrictions r1)
        {
            bool found = false;
            int i = 0;
            while (i < r1.RoomRestrictions.Count)
            {
                RoomRestrictions current = r1.RoomRestrictions[i];

                if (current.InheritRestrictionsFromParent)
                {
                    if (found)
                    {
                        Plugin.Logger.LogInfo("Duplicate restriction set found. Merging");
                        r1.RoomRestrictions[0].AddRooms(current.Rooms);
                        r1.RoomRestrictions.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        found = true;

                        //Move to start of collection
                        if (i > 0)
                        {
                            r1.RoomRestrictions.RemoveAt(i);
                            r1.RoomRestrictions.Insert(0, current);
                        }
                    }
                }
                i++;
            }
        }

        /// <summary>
        /// Not used
        /// </summary>
        /// <param name="restrictions"></param>
        /// <param name="source"></param>
        /// <param name="removeInherits"></param>
        /// <param name="skipOverInherits"></param>
        private static void processMerge(RegionRestrictions target, List<RoomRestrictions> restrictions, RegionRestrictions source, bool removeInherits = false, bool skipOverInherits = false)
        {
            //Search for a merge compatible place to store incoming restricted room data
            foreach (RoomRestrictions rs in restrictions)
            {
                if (rs.InheritRestrictionsFromParent)
                {
                    //removeInherits will pass restrictions from the 'source' down to the room level
                    if (removeInherits)
                        rs.Restrictions.InheritValues(source);
                    else if (skipOverInherits)
                        continue;
                }

                processMerge(target, rs, source);
            }
        }

        private static void processMerge(RegionRestrictions target, RoomRestrictions restrictions, RegionRestrictions source)
        {
            if (tryMergeRestrictions(target, restrictions, source))
                return;

            //The rest can be added directly to RoomRestrictions list
            target.RoomRestrictions.Add(restrictions);
        }

        /// <summary>
        /// Attempts to find an existing RoomRestrictions stored in 'this' that has the same restrictions fields as a provided RoomRestrictions.
        /// If found, adds rooms to RoomRestrictions stored in `this`.
        /// </summary>
        /// <param name="restrictions">The restrictions to merge</param>
        /// <param name="source">The owner of `restrictions`</param>
        /// <returns>Whether or not the merge target was changed</returns>
        private static bool tryMergeRestrictions(RegionRestrictions target, RoomRestrictions restrictions, RegionRestrictions source)
        {
            RoomRestrictions mergeTarget = findMergeTarget(target, restrictions, source);

            Plugin.Logger.LogDebug("MERGE TARGET: " + mergeTarget ?? "NULL");

            if (mergeTarget != null)
                return mergeTarget.AddRooms(restrictions.Rooms);

            return false;
        }

        private static RoomRestrictions findMergeTarget(RegionRestrictions target, RoomRestrictions restrictions, RegionRestrictions source)
        {
            return target.RoomRestrictions.Find(rs => rs.CompareFields(RoomRestrictions.GetRestrictions(restrictions, source), target));
        }
    }
}
