namespace LogUtils.Enums
{
    public partial class ComparisonLogID
    {
        /// <inheritdoc cref="LogID.Factory"/>
        public static new _Factory Factory = new _Factory();

        /// <summary>
        /// Factory implementation
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Implementation is required to be public accesible, but will not be accessed directly.")]
        public sealed new class _Factory
        {
            /// <inheritdoc cref="ComparisonLogID(string, string)"/>
            public LogID CreateID(string filename, string relativePathNoFile = null)
            {
                return new ComparisonLogID(filename, relativePathNoFile);
            }
        }
    }
}
