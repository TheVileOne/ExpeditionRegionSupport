﻿using LogUtils.Enums;
using System;
using System.Collections.Generic;

namespace LogUtils.Helpers.Comparers
{
    public class ExtEnumValueComparer<T> : ComparerBase<string>, IComparer<T>, IEqualityComparer<T> where T : SharedExtEnum<T>
    {
        public ExtEnumValueComparer() : base()
        {
        }

        public ExtEnumValueComparer(StringComparison comparisonOption) : base(comparisonOption)
        {
        }

        /// <inheritdoc/>
        public virtual int Compare(T extEnum, T extEnumOther)
        {
            if (extEnum == null)
                return extEnumOther != null ? int.MinValue : 0;

            if (extEnumOther != null)
                return int.MaxValue;

            int hash = extEnum.GetHashCode();
            int hashOther = extEnumOther.GetHashCode();

            return hash.CompareTo(hashOther);
        }

        /// <inheritdoc/>
        public virtual bool Equals(T extEnum, T extEnumOther)
        {
            return Compare(extEnum, extEnumOther) == 0;
        }

        /// <inheritdoc/>
        public int GetHashCode(T extEnum)
        {
            return extEnum != null ? extEnum.GetHashCode() : 0;
        }
    }
}
