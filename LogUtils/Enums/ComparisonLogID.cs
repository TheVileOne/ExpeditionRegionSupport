using LogUtils.Properties;
using System.Linq;

namespace LogUtils.Enums
{
    /// <summary>
    /// A type of <see cref="LogID"/> designed for comparisons. Not to be used for logging purposes
    /// </summary>
    public partial class ComparisonLogID : LogID
    {
        /// <summary>
        /// Indicates the kind of <see cref="LogID"/> represented by the comparison instance
        /// </summary>
        public readonly LogIDType RepresentedType;

        /// <summary>
        /// Constructs a lightweight <see cref="LogID"/> instance intended for local comparisons rather than logging
        /// </summary>
        /// <inheritdoc cref="LogID(string, string, LogAccess, bool)"/>
        /// <remarks>This type is not registered by default, and does not have its own properties (unless an existing <see cref="LogID"/> already has properties)</remarks>
        public ComparisonLogID(string filename, string relativePathNoFile = null) : base(filename)
        {
            RepresentedType = LogIDType.File;
            IsInstanceEnabled = false;

            Properties = LogProperties.PropertyManager.GetProperties(this, relativePathNoFile);
        }

        /// <inheritdoc cref="ComparisonLogID(string, string)"/>
        /// <param name="value">The value that identifies the <see cref="ComparisonLogID"/> instance</param>
        /// <param name="representedType">The type of <see cref="LogID"/> represented by this instance</param>
        public ComparisonLogID(string value, LogIDType representedType) : base(processValue(value, representedType))
        {
            RepresentedType = representedType;
            IsInstanceEnabled = false;

            var availableProperties = LogProperties.PropertyManager.GetProperties(this);
            Properties = availableProperties.FirstOrDefault();
        }

        private static string processValue(string valueBase, LogIDType representedType)
        {
            if (representedType == LogIDType.Group)
                return LogGroupID.ID_PREFIX + valueBase; //It is ensured that LogUtils will never pass in a value with the prefix already applied
            return valueBase;
        }
    }
}
