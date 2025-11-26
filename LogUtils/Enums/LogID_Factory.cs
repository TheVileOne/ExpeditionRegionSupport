using System;
using System.ComponentModel;

namespace LogUtils.Enums
{
    public partial class LogID
    {
        /// <summary>
        /// Access factory methods for creating specific kinds of <see cref="LogID"/> instances 
        /// </summary>
        public static IFactory Factory = new FactoryImpl();

        /// <summary>
        /// Factory implementation
        /// </summary>
        private sealed class FactoryImpl : IFactory
        {
            public LogID CreateID(string filename, LogAccess access, bool register)
            {
                return new LogID(filename, access, register);
            }

            public LogID CreateID(string filename, string relativePathNoFile, LogAccess access, bool register)
            {
                return new LogID(filename, relativePathNoFile, access, register);
            }

            public LogID CreateTemporaryID(string filename, string relativePathNoFile)
            {
                if (IsRegistered(filename, relativePathNoFile))
                    throw new InvalidOperationException("Temporary log ID could not be created; a registered log ID already exists.");

                return new LogID(filename, relativePathNoFile, LogAccess.Private);
            }

            public LogID CreateComparisonID(string filename, string relativePathNoFile)
            {
                return new ComparisonLogID(filename, relativePathNoFile);
            }

            public LogID CreateComparisonID(string value, LogIDType context)
            {
                return new ComparisonLogID(value, context);
            }
        }

        /// <summary>
        /// Represents a type exposing <see cref="LogID"/> construction options
        /// </summary>
        public interface IFactory
        {
            /// <inheritdoc cref="LogID(string, LogAccess, bool)"/>
            LogID CreateID(string filename, LogAccess access, bool register = false);

            /// <inheritdoc cref="LogID(string, string, LogAccess, bool)"/>
            LogID CreateID(string filename, string relativePathNoFile, LogAccess access, bool register = false);

            /// <summary>
            /// Creates a <see cref="LogID"/> instance designed to accessed only in a local context
            /// </summary>
            /// <inheritdoc cref="LogID(string, string, LogAccess, bool)" select="params"/>
            /// <exception cref="InvalidOperationException">A registered <see cref="LogID"/> instance already exists</exception>
            LogID CreateTemporaryID(string filename, string relativePathNoFile);

            /// <inheritdoc cref="ComparisonLogID(string, string)"/>
            LogID CreateComparisonID(string filename, string relativePathNoFile = null);

            /// <inheritdoc cref="ComparisonLogID(string, LogIDType)"/>
            LogID CreateComparisonID(string value, LogIDType context);
        }

        /// <inheritdoc cref="IFactory.CreateTemporaryID"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete($"Use {nameof(Factory.CreateTemporaryID)} instead.")]
        public static LogID CreateTemporaryID(string filename, string relativePathNoFile)
        {
            return Factory.CreateTemporaryID(filename, relativePathNoFile);
        }
    }
}
