using System.Runtime.CompilerServices;

namespace LogUtils.Diagnostics.Tools
{
    /// <summary>
    /// A simple struct that helps with identifying sources of null data
    /// </summary>
    public struct NullChecker
    {
        public int TotalNullsFound;

        /// <summary>
        /// This number represents the item count position of the latest value to be confirmed as null
        /// </summary>
        public int NullIndex;

        public string NullSourceName;

        /// <summary>
        /// Allows a limit to be set on null matches detected. Useful for detecting nulls in an object reference chain.
        /// <br></br>
        /// <br>Example: obj.FieldA.FieldB</br>
        /// <br>Note: Make sure that you null check each potential null, before trying to pass it into the NullChecker.</br>
        /// <br>The null-condition operator (?) can be used to make this easier.</br>
        /// </summary>
        public int DetectionThreshold;

        private int valueCounter;

        /// <summary>
        /// Performs a null check on an object. Resulting feedback is then stored as field data
        /// </summary>
        /// <param name="obj">Object to evaluate</param>
        /// <param name="callerArgumentName">The name of the reference. Leave blank to autocapture argument</param>
        public void Check(object obj, [CallerArgumentExpression("obj")] string callerArgumentName = "")
        {
            if (DetectionThreshold > 0 && TotalNullsFound >= DetectionThreshold)
                return;

            valueCounter++;
            if (obj == null)
            {
                NullSourceName = callerArgumentName;
                NullIndex = valueCounter;
                TotalNullsFound++;
            }
        }
    }
}
