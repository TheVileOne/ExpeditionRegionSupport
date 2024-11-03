using System.Collections.Generic;
using LogUtils.Enums;

namespace LogUtils.Helpers.Comparers
{
    public class ExtEnumValueComparer<T> : ComparerBase<string>, IComparer<T>, IEqualityComparer<T> where T : SharedExtEnum<T>
    {
        public virtual int Compare(T extEnum, T extEnumOther)
        {
            if (extEnum == null)
                return extEnumOther != null ? int.MinValue : 0;

            if (extEnumOther != null)
                return int.MaxValue;

            return extEnum.valueHash.CompareTo(extEnumOther.valueHash);
        }

        public bool Equals(T extEnum, T extEnumOther)
        {
            return Compare(extEnum, extEnumOther) == 0;
        }

        public int GetHashCode(T extEnum)
        {
            if (extEnum == null)
                return 0;
            return extEnum.valueHash;
        }
    }
}
