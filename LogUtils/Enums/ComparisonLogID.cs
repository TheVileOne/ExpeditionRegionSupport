using LogUtils.Properties;
using System.IO;
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

        /// <inheritdoc/>
        public override string Tag
        {
            get
            {
                if (ManagedReference != null && !ReferenceEquals(ManagedReference, this)) //Can be null here when it is accessed through the constructor
                    return ManagedReference.Tag;

                if (Properties != null && RepresentedType == LogIDType.File) //Files and groups use different identifying information
                    return Path.Combine(Properties.OriginalFolderPath, Value);

                return Value;
            }
        }

        /// <summary>
        /// Constructs a lightweight <see cref="LogID"/> instance intended for local comparisons rather than logging
        /// </summary>
        /// <inheritdoc cref="LogID(string, string, LogAccess, bool)"/>
        /// <remarks>This type is not registered by default, and does not have its own properties (unless an existing <see cref="LogID"/> already has properties)</remarks>
        public ComparisonLogID(string filename, string relativePathNoFile = null) : base(Sanitize(filename))
        {
            RepresentedType = LogIDType.File;
            IsInstanceEnabled = false;

            Properties = LogProperties.PropertyManager.GetProperties(this, relativePathNoFile);
        }

        /// <inheritdoc cref="ComparisonLogID(string, string)"/>
        /// <param name="value">The value that identifies the <see cref="ComparisonLogID"/> instance</param>
        /// <param name="representedType">The type of <see cref="LogID"/> represented by this instance</param>
        public ComparisonLogID(string value, LogIDType representedType) : base(CreateIDValue(value, representedType))
        {
            RepresentedType = representedType;
            IsInstanceEnabled = false;

            if (representedType == LogIDType.File)
                Properties = LogProperties.PropertyManager.GetProperties(this, null); //Maintains same behavior as other constructor
            else
            {
                var availableProperties = LogProperties.PropertyManager.GetProperties(this);
                Properties = availableProperties.FirstOrDefault();
            }
        }

        /// <summary>
        /// Unsupported operation on this type
        /// </summary>
        public override void Register()
        {
            UtilityLogger.LogWarning($"Registering {nameof(ComparisonLogID)} is unsupported");
        }

        /// <summary>
        /// Unsupported operation on this type
        /// </summary>
        public override void Unregister()
        {
            UtilityLogger.LogWarning($"Unregistering {nameof(ComparisonLogID)} is unsupported");
        }
    }
}
