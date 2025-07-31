using System.Collections;

namespace LogUtils.Helpers.Extensions
{
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Extension method that checks if an enumerable contains an item of a specified type
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
