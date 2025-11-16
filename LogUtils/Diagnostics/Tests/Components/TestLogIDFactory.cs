using LogUtils.Enums;
using LogUtils.Helpers.FileHandling;

namespace LogUtils.Diagnostics.Tests.Components
{
    /// <summary>
    /// Factory implementation
    /// </summary>
    public sealed class TestLogIDFactory
    {
        internal const LogAccess DEFAULT_ACCESS = LogAccess.FullAccess;

        /// <summary>
        /// Creates a new <see cref="TestLogID"/> instance with a randomized filename
        /// </summary>
        public TestLogID Create()
        {
            return TestEnumFactory.Add(new TestLogID(DEFAULT_ACCESS));
        }

        /// <summary>
        /// Creates a new <see cref="TestLogID"/> instance
        /// </summary>
        public TestLogID Create(string filename, string path = null)
        {
            return TestEnumFactory.Add(new TestLogID(filename, path, DEFAULT_ACCESS));
        }

        /// <summary>
        /// Creates a new <see cref="LogGroupID"/> instance
        /// </summary>
        public LogGroupID CreateLogGroup(string name, string folderPath = null)
        {
            if (folderPath == null)
                return TestEnumFactory.Add(new LogGroupID(name, register: false));

            return TestEnumFactory.Add(new LogGroupID(name, folderPath, register: false));
        }

        /// <summary>
        /// Assigns a new <see cref="LogID"/> member to a log group
        /// </summary>
        public LogID CreateLogGroupMember(LogGroupID groupID, string filename, string path = null)
        {
            return TestEnumFactory.Add(new LogID(groupID, filename, path, DEFAULT_ACCESS));
        }

        /// <summary>
        /// Creates a new <see cref="TestLogID"/> instance using the <see cref="SharedExtEnum{T}.Value"/> of an existing instance
        /// </summary>
        public TestLogID FromTarget(LogID target)
        {
            return TestEnumFactory.Add(new TestLogID(target.Value, null, DEFAULT_ACCESS));
        }

        /// <inheritdoc cref="FromTarget(LogID)"/>
        public TestLogID FromTarget(LogID target, string path)
        {
            return TestEnumFactory.Add(new TestLogID(target.Value, path, DEFAULT_ACCESS));
        }

        /// <summary>
        /// Creates a new <see cref="TestLogID"/> instance with a randomized filename
        /// </summary>
        public TestLogID FromPath(string path)
        {
            return TestEnumFactory.Add(new TestLogID(PathUtils.GetRandomFilename(FileExt.DEFAULT), path, DEFAULT_ACCESS));
        }
    }
}
