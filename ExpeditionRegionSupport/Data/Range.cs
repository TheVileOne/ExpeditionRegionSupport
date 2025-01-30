using System;

namespace ExpeditionRegionSupport.Data
{
    public struct Range : IEquatable<Range>, IComparable<Range>
    {
        public int Start;
        public int End;

        /// <summary>
        /// Difference between the End (exclusive) and Start (inclusive) values
        /// </summary>
        public int ValueRange => End - Start;

        /// <summary>
        /// Difference between the End (inclusive) and Start (inclusive) values
        /// </summary>
        public int ValueRangeInclusive => ValueRange + 1;

        public Range(int start, int end)
        {
            Start = start;
            End = end;
        }

        /// <summary>
        /// Creates the widest value range between two given ranges
        /// </summary>
        public static Range Combine(Range rangeA, Range rangeB)
        {
            return new Range(Math.Min(rangeA.Start, rangeB.Start), Math.Max(rangeA.End, rangeB.End));
        }

        public bool Equals(Range other)
        {
            return Start == other.Start && End == other.End;
        }

        public int CompareTo(Range other) => CompareByEarliestStart(other);

        public int CompareByEarliestStart(Range other)
        {
            if (Equals(other))
                return 0;

            //Start of range is either less than other, or start in the same place, and ends earlier than other
            if (Start < other.Start || (Start == other.Start && End < other.End))
                return -1;

            //other is considered greater in all other circumstances
            return 1;
        }

        public int CompareByLength(Range other)
        {
            int rangeDiff = ValueRange - other.ValueRange;

            if (rangeDiff != 0) //Output will be negative when the value range of this is less than the value range of other
                return rangeDiff;
            return CompareByEarliestStart(other);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return Start + "," + End;
        }

        public static bool operator ==(Range left, Range right) => left.Equals(right);

        public static bool operator !=(Range left, Range right) => !left.Equals(right);

        public static Range Zero => _zero;
        public static Range NegativeOne => _negativeOne;

        private static readonly Range _zero = new Range(0, 0);
        private static readonly Range _negativeOne = new Range(-1, -1);
    }
}
