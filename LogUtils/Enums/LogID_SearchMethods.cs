using LogUtils.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LogUtils.Enums
{
    public partial class LogID
    {
        /// <summary>
        /// Iterates through a collection of registered <see cref="LogID"/> instances
        /// </summary>
        /// <param name="includeGroupIDs">A flag that allows log group identifiers to be included in the results</param>
        public static IEnumerable<LogID> GetEntries(bool includeGroupIDs = false)
        {
            IEnumerable<LogProperties> entries = includeGroupIDs
                ? LogProperties.PropertyManager.AllProperties
                : LogProperties.PropertyManager.Properties;
            return entries.GetIDs();
        }

        /// <summary>
        /// Finds a registered <see cref="LogID"/> instance with the given filename, and path
        /// </summary>
        /// <remarks>
        /// Applies to log files only<br/>
        /// - Compares against ID, Filename, and CurrentFilename fields
        /// </remarks>
        /// <param name="filename">The filename to search for</param>
        /// <param name="relativePathNoFile">The filepath to search for. When set to null, any filename match will be returned with custom root being prioritized</param>
        public static LogID Find(string filename, string relativePathNoFile = null)
        {
            return LogProperties.PropertyManager.Properties.Find(filename, relativePathNoFile);
        }

        /// <summary>
        /// Finds a registered <see cref="LogID"/> instance with the given identifying value
        /// </summary>
        /// <param name="value">The value to search for</param>
        /// <param name="compareOptions">Represents options that determine which fields to check against</param>
        /// <param name="includeGroupIDs">A flag that allows log group identifiers to be included in the results</param>
        /// <returns>A <see cref="LogID"/> instance that matches the search criteria, or null if no match was found</returns>
        public static LogID Find(string value, CompareOptions compareOptions, bool includeGroupIDs)
        {
            IEnumerable<LogID> results;
            if (!includeGroupIDs)
                results = FindAll(value, compareOptions);
            else
                results = LogProperties.PropertyManager.AllProperties.FindAll(value, compareOptions);
            return results.FirstOrDefault();
        }

        /// <summary>
        /// Finds all registered <see cref="LogID"/> instance with the given filename
        /// </summary>
        /// <remarks>
        /// Applies to log files only<br/>
        /// - Compares against ID, Filename, and CurrentFilename fields
        /// </remarks>
        /// <param name="filename">The filename to search for</param>
        public static IEnumerable<LogID> FindAll(string filename)
        {
            return LogProperties.PropertyManager.Properties.FindAll(filename, CompareOptions.Basic);
        }

        /// <summary>
        /// Finds all registered <see cref="LogID"/> instance matching against common identifying fields
        /// </summary>
        /// <remarks>
        /// - Compares against ID, Filename, and CurrentFilename fields
        /// </remarks>
        /// <param name="value">The value to search for</param>
        /// <param name="includeGroupIDs">A flag that allows log group identifiers to be included in the results</param>
        public static IEnumerable<LogID> FindAll(string value, bool includeGroupIDs)
        {
            if (!includeGroupIDs)
                return FindAll(value);

            return LogProperties.PropertyManager.AllProperties.FindAll(value, CompareOptions.Basic);
        }

        /// <summary>
        /// Finds all registered <see cref="LogID"/> instances with the given filename
        /// </summary>
        /// <remarks>Applies to log files only</remarks>
        /// <param name="filename">The filename to search for</param>
        /// <param name="compareOptions">Represents options that determine which fields to check against</param>
        public static IEnumerable<LogID> FindAll(string filename, CompareOptions compareOptions)
        {
            return LogProperties.PropertyManager.Properties.FindAll(filename, compareOptions);
        }

        /// <summary>
        /// Finds all registered <see cref="LogID"/> instances matching against one or more identifying fields
        /// </summary>
        /// <param name="value">The value to search for</param>
        /// <param name="compareOptions">Represents options that determine which fields to check against</param>
        /// <param name="includeGroupIDs">A flag that allows log group identifiers to be included in the results</param>
        public static IEnumerable<LogID> FindAll(string value, CompareOptions compareOptions, bool includeGroupIDs)
        {
            if (!includeGroupIDs)
                return FindAll(value, compareOptions);

            return LogProperties.PropertyManager.AllProperties.FindAll(value, compareOptions);
        }

        /// <summary>
        /// Finds all registered <see cref="LogID"/> instances that match a predicate
        /// </summary>
        /// <remarks>Applies to log files only</remarks>
        /// <param name="predicate">The predicate to match</param>
        public static IEnumerable<LogID> FindAll(Func<LogProperties, bool> predicate)
        {
            return LogProperties.PropertyManager.Properties.FindAll(predicate);
        }

        /// <summary>
        /// Finds all registered <see cref="LogID"/> instances that match a predicate
        /// </summary>
        /// <param name="predicate">The predicate to match</param>
        /// <param name="includeGroupIDs">A flag that allows log group identifiers to be included in the results</param>
        public static IEnumerable<LogID> FindAll(Func<LogProperties, bool> predicate, bool includeGroupIDs)
        {
            if (!includeGroupIDs)
                return FindAll(predicate);

            return LogProperties.PropertyManager.AllProperties.FindAll(predicate);
        }

        /// <summary>
        /// Finds all registered <see cref="LogID"/> instances with the given tag
        /// </summary>
        /// <remarks>Applies to log files only</remarks>
        /// <param name="tag">The tag to search for</param>
        public static IEnumerable<LogID> FindByTag(string tag)
        {
            return LogProperties.PropertyManager.Properties.FindByTag(tag);
        }

        /// <summary>
        /// Finds all registered <see cref="LogID"/> instances with the given tag
        /// </summary>
        /// <param name="tag">The tag to search for</param>
        /// <param name="includeGroupIDs">A flag that allows log group identifiers to be included in the results</param>
        public static IEnumerable<LogID> FindByTag(string tag, bool includeGroupIDs)
        {
            if (!includeGroupIDs)
                return FindByTag(tag);

            return LogProperties.PropertyManager.AllProperties.FindByTag(tag);
        }

        /// <summary>
        /// Checks whether file, and path combination matches the file and path information of an existing registered <see cref="LogID"/> instance
        /// </summary>
        /// <remarks>Applies to log files only</remarks>
        /// <param name="filename">The filename to search for</param>
        /// <param name="relativePathNoFile">The filepath to search for. When set to null, any filename match will be returned with custom root being prioritized</param>
        public static bool IsRegistered(string filename, string relativePathNoFile = null)
        {
            return Find(filename, relativePathNoFile) != null;
        }

        /// <summary>
        /// Checks whether a registered <see cref="LogID"/> instance with the given identifying value exists
        /// </summary>
        /// <param name="value">The value to search for</param>
        /// <param name="includeGroupIDs">A flag that allows log group identifiers to be included in the results</param>
        public static bool IsRegistered(string value, bool includeGroupIDs)
        {
            IEnumerable<LogID> results = FindAll(value, includeGroupIDs);
            return results.Any();
        }
    }
}
