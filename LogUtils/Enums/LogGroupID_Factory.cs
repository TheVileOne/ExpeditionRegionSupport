using System.ComponentModel;

namespace LogUtils.Enums
{
    public partial class LogGroupID
    {
        public static LogID CreateComparisonID(string value)
        {
            return new ComparisonLogID(value, LogIDType.Group);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public new static LogID CreateComparisonID(string filename, string relativePathNoFile = null)
        {
            return LogID.CreateComparisonID(filename, relativePathNoFile);
        }
    }
}
