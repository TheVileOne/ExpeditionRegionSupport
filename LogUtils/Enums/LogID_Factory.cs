using System;
using System.ComponentModel;

namespace LogUtils.Enums
{
    public partial class LogID
    {
        /// <summary>
        /// Access factory methods for creating specific kinds of <see cref="LogID"/> instances 
        /// </summary>
        public static _Factory Factory = new _Factory();

        /// <summary>
        /// Factory implementation
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Implementation is required to be public accesible, but will not be accessed directly.")]
        public sealed class _Factory
        {
            /// <inheritdoc cref="LogID(string, LogAccess, bool)"/>
            public LogID CreateID(string filename, LogAccess access, bool register = false)
            {
                return new LogID(filename, access, register);
            }

            /// <inheritdoc cref="LogID(string, string, LogAccess, bool)"/>
            public LogID CreateID(string filename, string relativePathNoFile, LogAccess access, bool register = false)
            {
                return new LogID(filename, relativePathNoFile, access, register);
            }

            /// <summary>
            /// Creates a <see cref="LogID"/> instance designed to accessed only in a local context
            /// </summary>
            /// <inheritdoc cref="LogID(string, string, LogAccess, bool)" select="params"/>
            /// <exception cref="InvalidOperationException">A registered <see cref="LogID"/> instance already exists</exception>
            public LogID CreateTemporaryID(string filename, string relativePathNoFile)
            {
                if (IsRegistered(filename, relativePathNoFile))
                    throw new InvalidOperationException("Temporary log ID could not be created; a registered log ID already exists.");

                return new LogID(filename, relativePathNoFile, LogAccess.Private);
            }

            /// <inheritdoc cref="ComparisonLogID(string, string)"/>
            public LogID CreateComparisonID(string filename, string relativePathNoFile = null)
            {
                return new ComparisonLogID(filename, relativePathNoFile);
            }

            /// <inheritdoc cref="ComparisonLogID(string, LogIDType)"/>
            public LogID CreateComparisonID(string value, LogIDType context)
            {
                return new ComparisonLogID(value, context);
            }
        }

        /// <inheritdoc cref="_Factory.CreateTemporaryID"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete($"Use {nameof(Factory.CreateTemporaryID)} instead.")]
        public static LogID CreateTemporaryID(string filename, string relativePathNoFile)
        {
            return Factory.CreateTemporaryID(filename, relativePathNoFile);
        }
    }
}
