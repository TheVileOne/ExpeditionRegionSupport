using System;

namespace LogUtils.Enums
{
    public partial class LogID
    {
        /// <summary>
        /// Creates a <see cref="LogID"/> instance designed to accessed only in a local context
        /// </summary>
        /// <inheritdoc cref="LogID(string, string, LogAccess, bool)" select="params"/>
        /// <exception cref="InvalidOperationException">A registered <see cref="LogID"/> instance already exists</exception>
        public static LogID CreateTemporaryID(string filename, string relativePathNoFile)
        {
            if (IsRegistered(filename, relativePathNoFile))
                throw new InvalidOperationException("Temporary log ID could not be created; a registered log ID already exists.");

            return new LogID(filename, relativePathNoFile, LogAccess.Private);
        }

        public static LogID CreateComparisonID(string filename, string relativePathNoFile = null)
        {
            return new ComparisonLogID(filename, relativePathNoFile);
        }

        public static LogID CreateComparisonID(string value, LogIDType context)
        {
            return new ComparisonLogID(value, context);
        }
    }
}
