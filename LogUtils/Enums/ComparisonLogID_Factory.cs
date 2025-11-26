namespace LogUtils.Enums
{
    public partial class ComparisonLogID
    {
        /// <inheritdoc cref="LogID.Factory"/>
        public static new IFactory Factory = new FactoryImpl();

        /// <summary>
        /// Factory implementation
        /// </summary>
        private sealed class FactoryImpl : IFactory
        {
            public LogID CreateID(string filename, string relativePathNoFile)
            {
                return new ComparisonLogID(filename, relativePathNoFile);
            }
        }

        /// <summary>
        /// Represents a type exposing <see cref="LogID"/> construction options
        /// </summary>
        public new interface IFactory
        {
            /// <inheritdoc cref="ComparisonLogID(string, string)"/>
            public LogID CreateID(string filename, string relativePathNoFile = null);
        }
    }
}
