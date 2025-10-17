using System;
using System.Collections.Generic;

namespace LogUtils.Enums
{
    //TODO: Finish
    public class LogTargetCombiner : ICombiner<ILogTarget, CompositeLogTarget>
    {
        /// <inheritdoc/>
        public CompositeLogTarget Combine(ILogTarget a, ILogTarget b)
        {
            var flags = new HashSet<ILogTarget>();

            flags.TryAdd(a);
            flags.TryAdd(b);

            return new CompositeLogTarget(flags);
        }

        /// <inheritdoc/>
        public CompositeLogTarget Distinct(ILogTarget a, ILogTarget b)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public ILogTarget GetComplement(ILogTarget target)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public CompositeLogTarget Intersect(ILogTarget a, ILogTarget b)
        {
            throw new NotImplementedException();
        }
    }
}
