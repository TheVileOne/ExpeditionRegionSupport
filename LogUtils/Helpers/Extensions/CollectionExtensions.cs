using System.Collections;

namespace LogUtils
{
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Checks that an item of a specified type is contained by the enumerable
        /// </summary>
        public static bool ContainsType<T>(this IEnumerable self)
        {
            var enumerator = self.GetEnumerator();

            while (enumerator.MoveNext())
            {
                if (enumerator.Current is T)
                    return true;
            }
            return false;
        }
    }
}
