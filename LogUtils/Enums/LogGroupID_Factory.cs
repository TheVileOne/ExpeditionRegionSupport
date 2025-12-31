using LogUtils.Helpers.FileHandling;
using LogUtils.Properties;
using System;

namespace LogUtils.Enums
{
    public partial class LogGroupID
    {
        /// <inheritdoc cref="LogID.Factory"/>
        public static new IFactory Factory = new FactoryImpl();

        /// <summary>
        /// Factory implementation
        /// </summary>
        private sealed class FactoryImpl : IFactory
        {
            public LogID CreateComparisonID(string value)
            {
                return new ComparisonLogID(value, LogIDType.Group);
            }

            /// <inheritdoc cref="CreateNamedGroup(string, string, string, bool)"/>
            public LogGroupID CreateNamedGroup(string name, bool register = false)
            {
                return CreateNamedGroup(name, null, null, register);
            }

            /// <inheritdoc cref="CreateNamedGroup(string, string, string, bool)"/>
            public LogGroupID CreateNamedGroup(string name, string path, bool register = false)
            {
                return CreateNamedGroup(name, path, null, register);
            }

            /// <summary>
            /// Creates a <see cref="LogGroupID"/> instance with an identifying name value.
            /// </summary>
            /// <param name="name">Unique identifying value for the group.</param>
            /// <param name="path">Default folder location (including folder name) of log group files.</param>
            /// <param name="modIDHint">The plugin ID that identifies a mod specific folder location to associate with the group path.</param>
            /// <inheritdoc cref="LogGroupID(string, string, bool)"/>
            /// <param name="register"></param>
            public LogGroupID CreateNamedGroup(string name, string path, string modIDHint, bool register = false)
            {
                //TODO: Implement mod hintpaths
                return new LogGroupID(name, path, register);
            }

            /// <summary>
            /// Creates a <see cref="LogGroupID"/> instance without an identifying name or path.
            /// </summary>
            public LogGroupID CreateAnonymousGroup()
            {
                return CreateAnonymousGroup(null, null);
            }

            /// <inheritdoc cref="CreateAnonymousGroup(string, string)"/>
            public LogGroupID CreateAnonymousGroup(string path)
            {
                return CreateAnonymousGroup(path, null);
            }

            /// <summary>
            /// Creates a <see cref="LogGroupID"/> instance without an identifying name value.
            /// </summary>
            /// <param name="path">Default folder location (including folder name) of log group files.</param>
            /// <param name="modIDHint">The plugin ID that identifies a mod specific folder location to associate with the group path.</param>
            public LogGroupID CreateAnonymousGroup(string path, string modIDHint)
            {
                //TODO: Implement mod hintpaths
                return new LogGroupID(null, path);
            }
        }

        /// <summary>
        /// Represents a type exposing <see cref="LogID"/> construction options
        /// </summary>
        public new interface IFactory
        {
            /// <inheritdoc cref="ComparisonLogID(string, LogIDType)"/>
            LogID CreateComparisonID(string value);

            /// <inheritdoc cref="FactoryImpl.CreateNamedGroup(string, string, string, bool)"/>
            LogGroupID CreateNamedGroup(string name, bool register = false);

            /// <inheritdoc cref="FactoryImpl.CreateNamedGroup(string, string, string, bool)"/>
            LogGroupID CreateNamedGroup(string name, string path, bool register = false);

            /// <inheritdoc cref="FactoryImpl.CreateNamedGroup(string, string, string, bool)"/>
            LogGroupID CreateNamedGroup(string name, string path, string modIDHint, bool register = false);

            /// <inheritdoc cref="FactoryImpl.CreateAnonymousGroup()"/>
            LogGroupID CreateAnonymousGroup();

            /// <inheritdoc cref="FactoryImpl.CreateAnonymousGroup(string, string)"/>
            LogGroupID CreateAnonymousGroup(string path);

            /// <inheritdoc cref="FactoryImpl.CreateAnonymousGroup(string, string)"/>
            LogGroupID CreateAnonymousGroup(string path, string modIDHint);
        }
    }
}
