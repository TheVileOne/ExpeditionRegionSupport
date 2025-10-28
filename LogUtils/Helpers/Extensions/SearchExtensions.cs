using LogUtils.Enums;
using LogUtils.Helpers.Comparers;
using LogUtils.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LogUtils
{
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Finds a <see cref="LogID"/> instance with the given metadata in the provided collection
        /// </summary>
        /// <remarks>
        /// - Compares ID, Filename, and CurrentFilename fields
        /// </remarks>
        /// <param name="values"></param>
        /// <param name="filename">The filename to search for</param>
        /// <param name="relativePathNoFile">The filepath to search for. When set to null, any filename match will be returned with custom root being prioritized</param>
        public static LogID Find(this IEnumerable<LogID> values, string filename, string relativePathNoFile = null)
        {
            return values.Select(id => id.Properties)
                         .Find(filename, relativePathNoFile);
        }

        /// <summary>
        /// Finds all <see cref="LogID"/> instances with the given metadata in the provided collection
        /// </summary>
        /// <remarks>
        /// - Compares ID, Filename, and CurrentFilename fields
        /// </remarks>
        /// <param name="values"></param>
        /// <param name="filename">The filename to search for</param>
        public static IEnumerable<LogID> FindAll(this IEnumerable<LogID> values, string filename)
        {
            return values.Select(id => id.Properties)
                         .FindAll(filename, CompareOptions.Basic);
        }

        /// <summary>
        /// Finds all <see cref="LogID"/> instances with the given metadata in the provided collection
        /// </summary>
        /// <param name="values"></param>
        /// <param name="filename">The filename to search for</param>
        /// <param name="compareOptions">Represents options that determine which fields to check against</param>
        public static IEnumerable<LogID> FindAll(this IEnumerable<LogID> values, string filename, CompareOptions compareOptions)
        {
            return values.Select(id => id.Properties)
                         .FindAll(p => p.HasFilename(filename, compareOptions));
        }

        /// <summary>
        /// Finds all <see cref="LogID"/> instances that match a predicate in the provided collection
        /// </summary>
        /// <param name="values"></param>
        /// <param name="predicate">The predicate to match</param>
        public static IEnumerable<LogID> FindAll(this IEnumerable<LogID> values, Func<LogProperties, bool> predicate)
        {
            return values.Select(id => id.Properties)
                         .FindAll(predicate);
        }

        /// <summary>
        /// Finds all <see cref="LogID"/> instances with the given tag in the provided collection
        /// </summary>
        /// <param name="values"></param>
        /// <param name="tag">The tag to search for</param>
        public static IEnumerable<LogID> FindByTag(this IEnumerable<LogID> values, string tag)
        {
            return values.Select(id => id.Properties)
                         .FindByTag(tag);
        }

        /// <inheritdoc cref="Find(IEnumerable{LogID}, string, string)"/>
        internal static LogID Find(this IEnumerable<LogProperties> values, string filename, string relativePathNoFile = null)
        {
            IEnumerable<LogID> results = values.FindAll(filename, CompareOptions.Basic);

            if (!results.Any())
                return null;

            bool searchForAnyPath = LogProperties.IsPathWildcard(relativePathNoFile);
            string searchPath = LogProperties.GetContainingPath(relativePathNoFile);

            LogID bestCandidate = null;
            foreach (LogID logID in results)
            {
                if (logID.Properties.HasFolderPath(searchPath))
                {
                    bestCandidate = logID;
                    break; //Best match has been found
                }

                if (searchForAnyPath && bestCandidate == null) //First match is prioritized over any other match when all paths are valid
                    bestCandidate = logID;
            }
            return bestCandidate;
        }

        /// <inheritdoc cref="FindAll(IEnumerable{LogID}, string, CompareOptions)"/>
        internal static IEnumerable<LogID> FindAll(this IEnumerable<LogProperties> values, string filename, CompareOptions compareOptions)
        {
            return values.Where(p => p.HasFilename(filename, compareOptions))
                         .Select(p => p.ID);
        }

        /// <inheritdoc cref="FindAll(IEnumerable{LogID}, Func{LogProperties, bool})"/>
        internal static IEnumerable<LogID> FindAll(this IEnumerable<LogProperties> values, Func<LogProperties, bool> predicate)
        {
            return values.Where(predicate)
                         .Select(p => p.ID);
        }

        /// <inheritdoc cref="FindByTag(IEnumerable{LogID}, string)"/>
        internal static IEnumerable<LogID> FindByTag(this IEnumerable<LogProperties> values, string tag)
        {
            return values.Where(p => p.Tags.Contains(tag, ComparerUtils.StringComparerIgnoreCase))
                         .Select(p => p.ID);
        }
    }
}
