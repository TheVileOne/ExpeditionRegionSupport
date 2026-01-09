using LogUtils.Enums;
using LogUtils.Helpers.FileHandling;
using LogUtils.Properties;
using System;
using System.Collections.Generic;

namespace LogUtils.Helpers.Comparers
{
    public class LogPathComparer : IEqualityComparer<LogProperties>, IEqualityComparer<LogID>
    {
        /// <summary>
        /// Accepts a <see cref="LogProperties"/> instance, and returns a fully qualified path string
        /// </summary>
        protected PathProvider Selector;

        public LogPathComparer()
        {
            //Use the default implementation
            Selector = defaultSelector;
        }

        public LogPathComparer(PathProvider pathSelector)
        {
            Selector = pathSelector;
        }

        /// <inheritdoc/>
        public bool Equals(LogID id, LogID idOther)
        {
            return Equals(id?.Properties, idOther?.Properties);
        }

        /// <inheritdoc/>
        public int GetHashCode(LogID obj)
        {
            return GetHashCode(obj?.Properties);
        }

        /// <inheritdoc/>
        public bool Equals(LogProperties properties, LogProperties propertiesOther)
        {
            if (properties == null || propertiesOther == null)
                return properties == propertiesOther;

            return PathUtils.PathsAreEqual(Selector(properties), Selector(propertiesOther));
        }

        /// <inheritdoc/>
        public int GetHashCode(LogProperties obj)
        {
            try
            {
                return ComparerUtils.PathComparer.GetHashCode(Selector(obj));
            }
            catch (NullReferenceException)
            {
                UtilityLogger.LogWarning("Path selector not designed to handle null values");
                return 0;
            }
        }

        private static string defaultSelector(LogProperties properties) => properties?.CurrentFolderPath;
    }

    public delegate string PathProvider(LogProperties properties);
}
