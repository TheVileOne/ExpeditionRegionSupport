using System;
using System.Collections.Generic;
using LogUtils.Enums;

namespace LogUtils.Helpers
{
    public class ExtEnumValueComparer<T> : IEqualityComparer<T>, IComparer<T> where T : SharedExtEnum<T>
    {
        protected StringComparer Comparer;

        public ExtEnumValueComparer()
        {
            Comparer = EqualityComparer.StringComparerIgnoreCase;
        }

        public ExtEnumValueComparer(StringComparison compareOption)
        {
            Comparer = EqualityComparer.GetComparer(compareOption);
        }

        public int Compare(T obj, T objOther)
        {
            if (obj == null)
                return objOther == null ? 0 : int.MinValue;

            if (objOther == null)
                return int.MaxValue;

            return obj.valueHash.CompareTo(objOther.valueHash);
        }

        public bool Equals(T obj, T objOther)
        {
            return Compare(obj, objOther) == 0;
        }

        public int GetHashCode(T obj)
        {
            if (obj == null)
                return 0;
            return obj.valueHash;
        }
    }
}
