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
            public LogGroupID CreateID(string value, bool register)
            {
                return new LogGroupID(value, register);
            }

            public LogGroupID CreateID(string value, string path, bool register)
            {
                return new LogGroupID(value, path, register);
            }

            public LogID CreateComparisonID(string value)
            {
                return new ComparisonLogID(value, LogIDType.Group);
            }

            public LogGroupID FromPath(string path, bool register = false)
            {
                path = LogProperties.GetContainingPath(path);

                string rootPath = RainWorldDirectory.LocateRoot(path);

                string idValue;
                if (rootPath != null)
                {
                    if (path.Length == rootPath.Length) //Direct match to a root path
                        throw new ArgumentException($"{nameof(FromPath)} does not accept a Rain World root path as a valid id");

                    if (RainWorldDirectory.IsIllegalLogPath(path))
                        throw new ArgumentException($"{nameof(FromPath)} does not allow logging to this path\n" + path);

                    //Remove root portion of the path string
                    idValue = path.Substring(rootPath.Length);
                }
                else //Path is not part of the Rain World folder - the best we can do is to make it path volume independent
                {
                    idValue = PathUtils.PrependWithSeparator(PathUtils.Unroot(path));
                }
                return new LogGroupID(idValue, path, register);
            }
        }

        /// <summary>
        /// Represents a type exposing <see cref="LogID"/> construction options
        /// </summary>
        public new interface IFactory
        {
            /// <inheritdoc cref="LogGroupID(string, bool)"/>
            LogGroupID CreateID(string value, bool register = false);

            /// <inheritdoc cref="LogGroupID(string, string, bool)"/>
            LogGroupID CreateID(string value, string path, bool register = false);

            /// <inheritdoc cref="ComparisonLogID(string, LogIDType)"/>
            LogID CreateComparisonID(string value);

            /// <summary>
            /// Creates a <see cref="LogGroupID"/> using a folder path as an identifier
            /// </summary>
            /// <param name="path">The path to set as the target log path for log group members</param>
            /// <inheritdoc cref="LogGroupID(string, string, bool)">
            /// <param name="register"></param>
            /// </inheritdoc>
            /// <exception cref="ArgumentException">Root paths, certain game directories, and mod containing directories are not allowed to be selected by this factory method</exception>
            LogGroupID FromPath(string path, bool register = false);
        }
    }
}
