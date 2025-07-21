﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ExpeditionRegionSupport.Regions.Restrictions
{
    public partial class RegionRestrictions
    {
        public WorldState WorldState = WorldState.Any;
        public ProgressionRequirements ProgressionRestriction = ProgressionRequirements.None;

        public SlugcatRestrictions Slugcats = new SlugcatRestrictions();

        /// <summary>
        /// Any restrictions that need to be applied per room, or collection of rooms is applied here
        /// 
        /// Using a list allows one group of rooms to have one restriction set that is different than another group of rooms.
        /// Avoid including the same rooms in multiple restriction sets. Only process the first room detected.
        /// </summary>
        public List<RoomRestrictions> RoomRestrictions = new List<RoomRestrictions>();

        public bool HasEntries => !NoRestrictedFields || HasRoomEntries;

        public bool NoRestrictedFields =>
                    WorldState == WorldState.Any &&
                    ProgressionRestriction == ProgressionRequirements.None &&
                    Slugcats.IsEmpty;

        public bool HasRoomEntries => Restrictions.RoomRestrictions.HasEntries(RoomRestrictions);

        /// <summary>
        /// Restrictions apply to specific rooms rather than entire region
        /// </summary>
        public bool IsRoomSpecific => Restrictions.RoomRestrictions.InheritsFromParent(RoomRestrictions);

        public RegionRestrictions()
        {
        }

        public RegionRestrictions(WorldState worldState)
        {
            WorldState = worldState;
        }

        public void InheritValues(RegionRestrictions r, bool noRooms = false)
        {
            WorldState = r.WorldState;
            Slugcats = new SlugcatRestrictions()
            {
                Allowed = new List<SlugcatStats.Name>(r.Slugcats.Allowed),
                NotAllowed = new List<SlugcatStats.Name>(r.Slugcats.NotAllowed),
                UnlockRequired = new List<SlugcatStats.Name>(r.Slugcats.UnlockRequired)
            };

            if (!noRooms)
                RoomRestrictions = new List<RoomRestrictions>(r.RoomRestrictions);
            ProgressionRestriction = r.ProgressionRestriction;
        }

        /// <summary>
        /// Checks that restriction fields are equal ignoring any room restrictions 
        /// </summary>
        public bool CompareWithoutRoomRestrictions(RegionRestrictions restrictions)
        {
            return WorldState == restrictions.WorldState && ProgressionRestriction == restrictions.ProgressionRestriction && Slugcats.Equals(restrictions.Slugcats);
        }

        /// <summary>
        /// Change fields back to their default values
        /// </summary>
        public void ResetToDefaults()
        {
            WorldState = WorldState.Any;
            Slugcats.Clear();
            RoomRestrictions.Clear();
            ProgressionRestriction = ProgressionRequirements.None;
        }

        public RegionRestrictions GetRoomRestrictions(string roomName)
        {
            RoomRestrictions found = RoomRestrictions.Find(rs => rs.Rooms.Contains(roomName));

            //Return the correct restrictions source applying to this room
            if (found != null)
                return found.InheritRestrictionsFromParent ? this : found.Restrictions;

            return null;
        }

        /// <summary>
        /// Logs field data to file
        /// </summary>
        /// <param name="appliesToRooms">Indicates that this object is owned by a RoomRestrictions object</param>
        public void LogData(bool appliesToRooms = false)
        {
            //Only display inherited field values once
            if (appliesToRooms || !IsRoomSpecific)
                Plugin.Logger.LogDebug(ToString());

            if (!appliesToRooms)
            {
                Plugin.Logger.LogDebug("Room Restrictions");

                if (Restrictions.RoomRestrictions.HasEntries(RoomRestrictions))
                {
                    Plugin.Logger.LogDebug("Restriction Set Count: " + RoomRestrictions.Count);

                    if (RoomRestrictions.Count == 1)
                        RoomRestrictions[0].LogData(this);
                    else
                    {
                        int displayIndex = 0;

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < RoomRestrictions.Count; i++)
                        {
                            RoomRestrictions current = RoomRestrictions[i];

                            if (current.IsEmpty) continue;

                            Plugin.Logger.LogDebug($"[{displayIndex}]");
                            current.LogData(this);

                            /*sb.Append($"[{i}]");
                            sb.Append(Environment.NewLine);
                            sb.Append(RoomRestrictions[i].ToString());
                            sb.Append(Environment.NewLine);*/

                            displayIndex++;
                        }
                        //Plugin.Logger.LogDebug(sb.ToString());
                    }
                }
                else
                    Plugin.Logger.LogDebug("NONE");
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine()
              .Append("WorldState: ")
              .Append(WorldState)
              .Append(Environment.NewLine)
              .Append(Slugcats.ToString())
              .Append("Progression Restrictions: ")
              .Append(ProgressionRestriction);

            return sb.ToString();
        }
    }

    public class RoomRestrictions
    {
        public string RegionCode;

        public RegionRestrictions Restrictions;
        public List<string> Rooms = new List<string>();

        public bool IsEmpty => Rooms.Count == 0;

        /// <summary>
        /// Without any room specific restrictions, a room will take on the restrictions from the parent region.
        /// </summary>
        public bool InheritRestrictionsFromParent => !Restrictions.HasEntries;

        public static bool HasEntries(List<RoomRestrictions> restrictions)
        {
            return restrictions.Exists(r => !r.IsEmpty);
        }

        public static bool InheritsFromParent(List<RoomRestrictions> restrictions)
        {
            return restrictions.Exists(r => r.InheritRestrictionsFromParent);
        }

        /// <summary>
        /// Returns the restrictions associated with a RoomRestrictions object
        /// </summary>
        public static RegionRestrictions GetRestrictions(RoomRestrictions r, RegionRestrictions source)
        {
            return r.InheritRestrictionsFromParent ? source : r.Restrictions;
        }

        /// <summary>
        /// Returns the restrictions associated with a RoomRestrictions object.
        /// Uses index position as a shortcut for checking inheritence.
        /// </summary>
        public static RegionRestrictions GetRestrictionsByIndex(RoomRestrictions r, RegionRestrictions source, int index)
        {
            return index > 0 ? r.Restrictions : GetRestrictions(r, source);
        }

        /// <summary>
        /// This is specifically used by the RestrictionProcessor to store rooms before their regions can be processed
        /// </summary>
        public RoomRestrictions()
        {
            Restrictions = new RegionRestrictions();
        }

        public RoomRestrictions(string regionCode)
        {
            RegionCode = regionCode;
            Restrictions = new RegionRestrictions();
        }

        public bool AddRoom(string room, bool enforceUniqueRooms = true)
        {
            room = room.Trim();

            if (enforceUniqueRooms && Rooms.Contains(room))
                return false;

            Rooms.Add(room);
            return true;
        }

        public bool AddRooms(IEnumerable<string> rooms)
        {
            bool roomAdded = false;
            foreach (string room in rooms)
                roomAdded |= AddRoom(room);

            return roomAdded;
        }

        /// <summary>
        /// Compares restriction fields while ignoring content in Rooms list
        /// </summary>
        public bool CompareFields(RoomRestrictions r, RegionRestrictions mergeSource)
        {
            return CompareFields(r.Restrictions, mergeSource);
        }

        public bool CompareFields(RegionRestrictions r, RegionRestrictions mergeSource)
        {
            if (InheritRestrictionsFromParent)
                return mergeSource.CompareWithoutRoomRestrictions(r);

            return Restrictions.CompareWithoutRoomRestrictions(r);
        }

        /// <summary>
        /// When object inherits from parent, remove that inheritence by storing restrictions directly.
        /// </summary>
        /// <param name="newValues">New restrictions to store</param>
        public void ReplaceInheritence(RegionRestrictions newValues)
        {
            if (InheritRestrictionsFromParent)
                Restrictions.InheritValues(newValues);
        }

        public void LogData()
        {
            Plugin.Logger.LogDebug(ToString());

            //This overload doesn't expose the parent.
            if (InheritRestrictionsFromParent)
            {
                Plugin.Logger.LogDebug("Inherits");
                return;
            }

            Restrictions.LogData(true);
        }

        public void LogData(RegionRestrictions source)
        {
            Plugin.Logger.LogDebug(ToString());
            GetRestrictions(this, source).LogData(true);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (string room in Rooms)
            {
                sb.Append(room);
                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Contains data on which slugcats should be allowed, or not allowed to access a region, or region spawn location
    /// </summary>
    public class SlugcatRestrictions
    {
        public bool IsEmpty => Allowed.Count == 0 && NotAllowed.Count == 0 && UnlockRequired.Count == 0;

        public List<SlugcatStats.Name> Allowed = new List<SlugcatStats.Name>();
        public List<SlugcatStats.Name> NotAllowed = new List<SlugcatStats.Name>();
        public List<SlugcatStats.Name> UnlockRequired = new List<SlugcatStats.Name>();

        public void Allow(string name)
        {
            if (SlugcatUtils.TryParse(name, out SlugcatStats.Name found))
            {
                Allow(found);
                return;
            }

            Plugin.Logger.LogWarning("Unable to process slugcat name " + name.Trim());
        }

        public bool Allow(SlugcatStats.Name name)
        {
            NotAllowed.Remove(name);

            if (!Allowed.Contains(name))
            {
                Allowed.Add(name);
                return true;
            }
            return false;
        }

        public void Disallow(string name)
        {
            if (SlugcatUtils.TryParse(name, out SlugcatStats.Name found))
            {
                Disallow(found);
                return;
            }

            Plugin.Logger.LogWarning("Unable to process slugcat name " + name.Trim());
        }

        public bool Disallow(SlugcatStats.Name name)
        {
            Allowed.Remove(name);

            if (!NotAllowed.Contains(name))
            {
                NotAllowed.Add(name);
                return true;
            }
            return false;
        }

        public void SetUnlockRequirement(string name)
        {
            SetUnlockRequirement(SlugcatUtils.GetOrCreate(name));
        }

        public bool SetUnlockRequirement(SlugcatStats.Name name)
        {
            if (!UnlockRequired.Contains(name))
            {
                UnlockRequired.Add(name);
                return true;
            }
            return false;
        }

        public void Clear()
        {
            Allowed.Clear();
            NotAllowed.Clear();
            UnlockRequired.Clear();
        }

        public void MergeValues(SlugcatRestrictions r)
        {
            bool hasChanged = false;

            //Each list from the incoming restriction set needs to be combined with its corresponding list
            hasChanged |= applyMerge(r.Allowed, Allow, "ALLOW");
            hasChanged |= applyMerge(r.NotAllowed, Disallow, "DISALLOW");
            hasChanged |= applyMerge(r.UnlockRequired, SetUnlockRequirement, "UNLOCK REQUIRED");

            if (hasChanged)
                Plugin.Logger.LogInfo("SLUGCATS updated");
        }

        /// <summary>
        /// Takes a list of SlugcatStats.Name objects, and processes each object through a handler
        /// </summary>
        private bool applyMerge(List<SlugcatStats.Name> mergeValues, Func<SlugcatStats.Name, bool> mergeHandler, string logHeader)
        {
            bool hasChanged = false;
            foreach (SlugcatStats.Name name in mergeValues)
            {
                Plugin.Logger.LogInfo($"[{logHeader}]" + name);
                hasChanged |= mergeHandler.Invoke(name);
            }
            return hasChanged;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            string hasEntryString, noEntryString;

            hasEntryString = "Slugcats Allowed";
            noEntryString = "Slugcats Allowed: ALL";

            buildStrings(Allowed);

            hasEntryString = "Slugcats Not Allowed";
            noEntryString = "Slugcats Not Allowed: NONE";

            buildStrings(NotAllowed);

            hasEntryString = "Unlock Required";
            noEntryString = "Unlock Required: NONE";

            buildStrings(UnlockRequired);

            void buildStrings(List<SlugcatStats.Name> slugcatNames)
            {
                if (slugcatNames.Count == 0)
                {
                    sb.Append(noEntryString);
                    sb.Append(Environment.NewLine);
                }
                else
                {
                    sb.Append(hasEntryString);
                    sb.Append(Environment.NewLine);

                    foreach (SlugcatStats.Name name in slugcatNames)
                    {
                        sb.Append(name.value);
                        sb.Append(Environment.NewLine);
                    }
                }
            }
            return sb.ToString();
        }

        public override bool Equals(object obj)
        {
            SlugcatRestrictions restrictions = obj as SlugcatRestrictions;

            if (restrictions != null)
            {
                //Counts do not match
                if (this.Allowed.Count != restrictions.Allowed.Count
                 || this.NotAllowed.Count != restrictions.NotAllowed.Count
                 || this.UnlockRequired.Count != restrictions.UnlockRequired.Count)
                {
                    return false;
                }

                //Slugcats do not match
                if (this.Allowed.Exists(s => !restrictions.Allowed.Contains(s))
                 || this.NotAllowed.Exists(s => !restrictions.NotAllowed.Contains(s))
                 || this.UnlockRequired.Exists(s => !restrictions.UnlockRequired.Contains(s)))
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            var comparer = EqualityComparer<List<SlugcatStats.Name>>.Default;

            int hashCode = 1549165079;
            hashCode = (hashCode * -1521134295) + IsEmpty.GetHashCode();
            hashCode = (hashCode * -1521134295) + comparer.GetHashCode(Allowed);
            hashCode = (hashCode * -1521134295) + comparer.GetHashCode(NotAllowed);
            hashCode = (hashCode * -1521134295) + comparer.GetHashCode(UnlockRequired);
            return hashCode;
        }
    }

    [Flags]
    public enum ProgressionRequirements
    {
        None = 0,
        OnVisit = 1,
        OnSlugcatUnlocked = 2
    }

    [Flags]
    public enum WorldState
    {
        Invalid = -1,
        None = 0,
        Any = Vanilla | MSC | Other,
        Monk = 1,
        Survivor = 1,
        Hunter = 1,
        Vanilla = 1, //Regions for Monk, Survivor, Hunter and any other campaign that has the same regions.
        SpearMaster = 2,
        Artificer = 4,
        OldWorld = SpearMaster | Artificer,
        Rivulet = 8,
        Gourmand = 16,
        Saint = 32,
        MSC = OldWorld | Rivulet | Gourmand | Saint,
        Other = 64
    }
}
