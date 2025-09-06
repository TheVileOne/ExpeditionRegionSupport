using System;
using System.Collections.Generic;

namespace LogUtils.Enums
{
    public class LogTargetCombiner : ICombiner<ILogTarget, CompositeLogTarget>
    {
        public CompositeLogTarget Combine(ILogTarget a, ILogTarget b)
        {
            var flags = new HashSet<ILogTarget>();

            flags.TryAdd(a);
            flags.TryAdd(b);

            return new CompositeLogTarget(flags);
        }

        public CompositeLogTarget Distinct(ILogTarget a, ILogTarget b)
        {
            throw new NotImplementedException();
        }

        public ILogTarget GetComplement(ILogTarget target)
        {
            throw new NotImplementedException();
        }

        public CompositeLogTarget Intersect(ILogTarget a, ILogTarget b)
        {
            throw new NotImplementedException();
        }
    }
}
