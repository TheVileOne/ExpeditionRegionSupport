using LogUtils.Properties;

namespace LogUtils.Enums
{
    /// <summary>
    /// A type of LogID designed for comparisons. Not to be used for logging purposes
    /// </summary>
    public class ComparisonLogID : LogID
    {
        /// <summary>
        /// Constructs a lightweight version of a LogID intended for local comparisons rather than logging
        /// </summary>
        /// <remarks>This type of LogID is not registered by default, and does not have its own properties (unless an existing LogID already has properties)</remarks>
        public ComparisonLogID(string filename, string relativePathNoFile = null) : base(filename)
        {
            Access = LogAccess.RemoteAccessOnly;
            IsInstanceEnabled = false;

            Properties = LogProperties.PropertyManager.GetProperties(this, relativePathNoFile);
        }
    }
}
