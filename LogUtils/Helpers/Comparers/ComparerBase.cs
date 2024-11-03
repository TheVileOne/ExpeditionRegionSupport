using System;
using System.Collections;
using System.Collections.Generic;

namespace LogUtils.Helpers.Comparers
{
    public abstract class ComparerBase<T> : IComparer, IEqualityComparer, IComparer<T>, IEqualityComparer<T>
    {
        protected IComparer<T> InnerComparer;
        protected IEqualityComparer<T> InnerEqualityComparer;

        protected ComparerBase()
        {
            InnerComparer = Comparer<T>.Default;
            InnerEqualityComparer = EqualityComparer<T>.Default;
        }

        protected ComparerBase(StringComparison comparisonOption)
        {
            StringComparer comparer = ComparerUtils.GetComparer(comparisonOption);

            InnerComparer = (IComparer<T>)comparer;
            InnerEqualityComparer = (IEqualityComparer<T>)comparer;
        }

        public virtual int Compare(object obj, object objOther)
        {
            if (obj == null)
                return objOther != null ? int.MinValue : 0;

            if (objOther == null)
                return int.MaxValue;
            return obj.GetHashCode().CompareTo(objOther.GetHashCode());
        }

        public virtual int Compare(T obj, T objOther)
        {
            return InnerComparer.Compare(obj, objOther);
        }

        public virtual bool Equals(T obj, T objOther)
        {
            return InnerEqualityComparer.Equals(obj, objOther);
        }

        public new bool Equals(object obj, object objOther)
        {
            return Compare(obj, objOther) == 0;
        }

        public virtual int GetHashCode(T obj)
        {
            return InnerEqualityComparer.GetHashCode(obj);
        }

        public virtual int GetHashCode(object obj)
        {
            return obj?.GetHashCode() ?? 0;
        }
    }
}
