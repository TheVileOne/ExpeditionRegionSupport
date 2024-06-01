using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Data
{
    public struct Range
    {
        public int Start;
        public int End;

        public int ValueRange => End - Start;

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

        public static Range Zero => _zero;
        public static Range NegativeOne => _negativeOne;

        private static readonly Range _zero = new Range(0, 0);
        private static readonly Range _negativeOne = new Range(-1, -1);
    }
}
