using LogUtils.Threading;
using System.Collections.Generic;
using System.Linq;

namespace LogUtils
{
    /// <summary>
    /// A type that provides an acquirable lock object
    /// </summary>
    public interface ILockable
    {
        /// <summary>
        /// Gets an acquirable lock object
        /// </summary>
        Lock GetLock();
    }

    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Selects all acquirable lock objects from the enumeration
        /// </summary>
        public static IEnumerable<Lock> GetLocks(this IEnumerable<ILockable> lockProvider)
        {
            return lockProvider.Select(provider => provider.GetLock());
        }
    }
}
