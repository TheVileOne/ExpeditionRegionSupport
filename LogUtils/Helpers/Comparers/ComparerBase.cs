using System;
using System.Collections.Generic;

namespace LogUtils.Helpers.Comparers
{
    public abstract class ComparerBase<T> : IComparer<T>, IEqualityComparer<T>
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

        public virtual int Compare(T obj, T objOther)
        {
            return InnerComparer.Compare(obj, objOther);
        }

        public virtual bool Equals(T obj, T objOther)
        {
            return InnerEqualityComparer.Equals(obj, objOther);
        }

        public virtual int GetHashCode(T obj)
        {
            return InnerEqualityComparer.GetHashCode(obj);
        }
    }
}
