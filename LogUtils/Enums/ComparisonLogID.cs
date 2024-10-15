using LogUtils.Properties;

namespace LogUtils.Enums
{
    public class ComparisonLogID : LogID
    {
        /// <summary>
        /// Constructs a lightweight instead of LogID intended for local comparisons rather than logging.
        /// ComparisonLogID instances are not registered by default, and do not have their own properties (except when an existing LogID has already created one) 
        /// </summary>
        public ComparisonLogID(string filename) : base(filename)
        {
            Access = LogAccess.RemoteAccessOnly;
            IsEnabled = false;

            Properties = LogProperties.PropertyManager.GetProperties(this, null);
        }
    }
}
