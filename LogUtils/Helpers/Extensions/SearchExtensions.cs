using LogUtils.Enums;
using LogUtils.Helpers.Comparers;
using LogUtils.Helpers.FileHandling;
using LogUtils.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LogUtils
{
    public static partial class ExtensionMethods
    {
        #region Search methods (LogID/LogProperties)
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
            return values.GetProperties()
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
            return values.GetProperties()
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
            return values.GetProperties()
                         .FindAll(p => p.HasFilename(filename, compareOptions));
        }

        /// <summary>
        /// Finds all <see cref="LogID"/> instances that match a predicate in the provided collection
        /// </summary>
        /// <param name="values"></param>
        /// <param name="predicate">The predicate to match</param>
        public static IEnumerable<LogID> FindAll(this IEnumerable<LogID> values, Func<LogProperties, bool> predicate)
        {
            return values.GetProperties()
                         .FindAll(predicate);
        }

        /// <summary>
        /// Finds all <see cref="LogID"/> instances with the given tag in the provided collection
        /// </summary>
        /// <param name="values"></param>
        /// <param name="tag">The tag to search for</param>
        public static IEnumerable<LogID> FindByTag(this IEnumerable<LogID> values, string tag)
        {
            return values.GetProperties()
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
                         .GetIDs();
        }

        /// <inheritdoc cref="FindAll(IEnumerable{LogID}, Func{LogProperties, bool})"/>
        internal static IEnumerable<LogID> FindAll(this IEnumerable<LogProperties> values, Func<LogProperties, bool> predicate)
        {
            return values.Where(predicate)
                         .GetIDs();
        }

        /// <inheritdoc cref="FindByTag(IEnumerable{LogID}, string)"/>
        internal static IEnumerable<LogID> FindByTag(this IEnumerable<LogProperties> values, string tag)
        {
            return values.Where(p => p.Tags.Contains(tag, ComparerUtils.StringComparerIgnoreCase))
                         .GetIDs();
        }

        /// <summary>
        /// Returns all <see cref="LogID"/> instances belonging to entries in this enumeration
        /// </summary>
        public static IEnumerable<LogID> GetIDs(this IEnumerable<LogProperties> entries)
        {
            return entries.Select(entry => entry.ID);
        }

        /// <summary>
        /// Returns all <see cref="LogProperties"/> instances belonging to entries in this enumeration
        /// </summary>
        public static IEnumerable<LogProperties> GetProperties(this IEnumerable<LogID> entries)
        {
            return entries.Select(entry => entry.Properties);
        }
        #endregion
        #region Search methods (LogGroupID/LogGroupProperties)
        /// <summary>
        /// Returns all <see cref="LogGroupID"/> instances belonging to entries in this enumeration
        /// </summary>
        public static IEnumerable<LogGroupID> GetIDs(this IEnumerable<LogGroupProperties> entries)
        {
            return entries.Select(entry => entry.ID)
                          .Cast<LogGroupID>();
        }

        /// <inheritdoc cref="GetProperties(IEnumerable{LogID})"/>
        public static IEnumerable<LogGroupProperties> GetProperties(this IEnumerable<LogGroupID> entries)
        {
            return entries.Select(entry => entry.Properties);
        }

        /// <summary>
        /// Returns all group members belonging to entries in this enumeration
        /// </summary>
        public static IEnumerable<LogID> GetMembers(this IEnumerable<LogGroupProperties> entries)
        {
            return entries.SelectMany(entry => entry.Members);
        }

        /// <summary>
        /// Returns all <see cref="LogProperties"/> instance belonging to group members of entries in this enumeration
        /// </summary>
        public static IEnumerable<LogProperties> GetMemberProperties(this IEnumerable<LogGroupProperties> entries)
        {
            return entries.GetMembers()
                          .GetProperties();
        }

        public static IEnumerable<LogGroupProperties> WithFolder(this IEnumerable<LogGroupProperties> entries)
        {
            return entries.Where(entry => entry.IsFolderGroup);
        }

        public static IEnumerable<LogGroupProperties> WithoutFolder(this IEnumerable<LogGroupProperties> entries)
        {
            return entries.Where(entry => !entry.IsFolderGroup);
        }

        public static IEnumerable<T> HasPath<T>(this IEnumerable<T> entries, string searchPath) where T : LogProperties
        {
            return entries.Where(entry => PathUtils.ContainsOtherPath(entry.CurrentFolderPath, searchPath));
        }

        public static IEnumerable<T> HasPathExact<T>(this IEnumerable<T> entries, string searchPath) where T : LogProperties
        {
            return entries.Where(entry => PathUtils.PathsAreEqual(entry.CurrentFolderPath, searchPath));
        }
        #endregion
    }
}
