using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpeditionRegionSupport.Regions.Restrictions;

namespace ExpeditionRegionSupport.Regions
{
    public class RegionList : List<RegionKey>
    {
        /// <summary>
        /// Regions tend to be processed in batches. We do not need to search the region list for each room entry.
        /// </summary>
        public RegionKey RegionCache;

        /// <summary>
        /// The cache is stored whether or not this list contains it. This is the result of that check.
        /// </summary>
        protected bool IsCacheFound = false;

        public bool IsRestricted => Exists(r => r.IsRegionRestricted || r.IsRoomRestricted);

        /// <summary>
        /// Add the specified region code to the list.
        /// </summary>
        public void Add(string regionCode)
        {
            if (RegionCache.RegionCode != regionCode)
                RegionCache = new RegionKey(regionCode);

            Add(RegionCache);
        }

        public new void Add(RegionKey key)
        {
            RegionCache = key;
            IsCacheFound = true;

            base.Add(key);
        }

        public void Remove(string regionCode)
        {
            Remove(new RegionKey(regionCode));
        }

        public new void Remove(RegionKey key)
        {
            if (RegionCache.Equals(key)) //Cache is stale
            {
                RegionCache = default;
                IsCacheFound = false;
            }

            base.Remove(key);
        }

        public new void RemoveAt(int index)
        {
            if (index < 0 || index >= Count) return;

            if (RegionCache.Equals(this[index])) //Cache is stale
            {
                RegionCache = default;
                IsCacheFound = false;
            }

            base.RemoveAt(index);
        }

        public new void Insert(int index, RegionKey key)
        {
            if (index < 0 || index >= Count) return;

            RegionCache = key;
            IsCacheFound = true;

            base.Insert(index, key);
        }

        /// <summary>
        /// Searches for a RegionKey with the specified region code.
        /// Each time this is called, a cache is set to the last checked RegionKey.
        /// </summary>
        /// <returns>Whether the RegionKey is present in the list</returns>
        public bool Contains(string regionCode)
        {
            if (RegionCache.RegionCode == regionCode)
                return IsCacheFound;

            IsCacheFound = false;

            //We want an accurate RegionCache. Get it directly from the list.
            IsCacheFound = TryFind(regionCode, out RegionCache);

            //Even if not found, we want to define a non-empty RegionKey, so Contains will have a valid reference to check against.
            if (RegionCache.IsEmpty)
                RegionCache = new RegionKey(regionCode);

            return IsCacheFound;
        }

        /// <summary>
        /// Searches for a RegionKey with the specified region code.
        /// </summary>
        /// <returns>The found key, or empty one if not found</returns>
        public RegionKey Find(string regionCode)
        {
            if (RegionCache.RegionCode == regionCode)
            {
                if (IsCacheFound)
                    return RegionCache;

                return default;
            }

            RegionKey found = Find(region => region.RegionCode == regionCode);

            IsCacheFound = !found.IsEmpty;
            RegionCache = IsCacheFound ? found : new RegionKey(regionCode); //Dummy value only set when IsCacheFound is false. Do not use as a reference.

            return found;
        }

        public bool TryFind(string regionCode, out RegionKey foundKey)
        {
            foundKey = Find(regionCode);

            return !foundKey.IsEmpty;
        }
    }

    public readonly struct RegionKey : IEquatable<RegionKey>
    {
        public readonly bool IsEmpty => RegionCode == null;
        public bool IsRegionRestricted => !Restrictions.IsRoomSpecific;
        public bool IsRoomRestricted => Restrictions.RoomRestrictions.Exists(r => !r.IsEmpty);

        /// <summary>
        /// The region code associated with this region key
        /// Format: "<regionCode>_<roomCode>"
        /// </summary>
        public readonly string RegionCode; //SU, LF etc.

        /// <summary>
        /// A list of room codes associated with this region key
        /// Format: "<regionCode>_<roomCode>"
        /// </summary>
        public readonly List<string> AvailableRooms;

        public readonly RegionRestrictions Restrictions;

        public RegionKey(string regionCode)
        {
            RegionCode = regionCode;
            AvailableRooms = new List<string>();
            Restrictions = new RegionRestrictions();
        }

        public RegionKey(string regionCode, RegionRestrictions restrictions) : this(regionCode)
        {
            Restrictions = restrictions;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            return Equals((RegionKey)obj);
        }

        public bool Equals(RegionKey other)
        {
            return RegionCode == other.RegionCode;
        }

        public static bool operator ==(RegionKey key1, RegionKey key2)
        {
            return key1.Equals(key2);
        }

        public static bool operator !=(RegionKey key1, RegionKey key2)
        {
            return !key1.Equals(key2);
        }

        /// <summary>
        /// Returns a RegionRestrictions object that applies to a given room
        /// </summary>
        /// <param name="roomInfo">Either a room name or room code</param>
        /// <param name="isRoomName">Whether to treat roomInfo as a name or code</param>
        public RegionRestrictions GetRoomRestrictions(string roomInfo, bool isRoomName)
        {
            //AvailableRooms isn't useful at the stage this method is needed.
            //Region-specific restrictions have already been processed at this point. Check for room restrictions. 
            if (IsEmpty || !IsRoomRestricted) return null;

            //RoomRestrictions object stores rooms as room names, not room codes
            string roomName = isRoomName ? roomInfo : RegionUtils.FormatRoomName(RegionCode, roomInfo);

            return Restrictions.GetRoomRestrictions(roomName);
        }

        /// <summary>
        /// Logs information about restrictions associated with this RegionKey
        /// </summary>
        public void LogRestrictions()
        {
            Plugin.Logger.LogDebug(string.Empty);
            Plugin.Logger.LogDebug("REGION RESTRICTIONS LOGGED: " + RegionCode);

            if (Restrictions == null)
            {
                Plugin.Logger.LogDebug("Restrictions NULL");
                return;
            }

            Restrictions.LogData();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return RegionCode;
        }
    }
}