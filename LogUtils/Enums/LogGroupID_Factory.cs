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
            
            public LogID CreateComparisonID(string value)
            {
                return new ComparisonLogID(value, LogIDType.Group);
            }
        }

        /// <summary>
        /// Represents a type exposing <see cref="LogID"/> construction options
        /// </summary>
        public new interface IFactory
        {
            /// <inheritdoc cref="LogGroupID(string, bool)"/>
            LogGroupID CreateID(string value, bool register = false);

            /// <inheritdoc cref="ComparisonLogID(string, LogIDType)"/>
            LogID CreateComparisonID(string value);
        }
    }
}
