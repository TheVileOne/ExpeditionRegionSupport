using LogUtils.Enums;
using LogUtils.Properties;
using System;
using System.Collections.Generic;

namespace LogUtils.Helpers.Comparers
{
    public class LogIDComparer : ExtEnumValueComparer<LogID>, IComparer<LogID>, IEqualityComparer<LogID>, IComparer<LogProperties>, IEqualityComparer<LogProperties>
    {
        protected CompareOptions CompareOptions;

        /// <summary>
        /// Constructs a new <see cref="LogIDComparer"/> instance with default comparison options
        /// </summary>
        public LogIDComparer() : this(CompareOptions.None)
        {
        }

        /// <inheritdoc cref="LogIDComparer(CompareOptions, StringComparison)"/>
        public LogIDComparer(CompareOptions compareOptions) : this(compareOptions, StringComparison.InvariantCultureIgnoreCase)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="LogIDComparer"/> instance
        /// </summary>
        /// <param name="compareOptions">Options for influencing comparison and equality results. Set as <see cref="CompareOptions.None"/> </param>
        /// <param name="stringComparison">Defines the case sensitivity, and culture rules when comparing string values</param>
        public LogIDComparer(CompareOptions compareOptions, StringComparison stringComparison) : base(stringComparison)
        {
            CompareOptions = compareOptions;
        }

        /// <inheritdoc/>
        public override int Compare(LogID id, LogID idOther)
        {
            if (id == null)
                return idOther != null ? int.MinValue : 0;

            if (idOther == null)
                return int.MaxValue;

            //Properties field may be null when comparing against ComparisonLogID instances
            if (id.Properties == null || idOther.Properties == null)
                return CompareNullProperties(id, idOther);

            return CompareNormal(id.Properties, idOther.Properties);
        }

        /// <inheritdoc/>
        public int Compare(LogProperties properties, LogProperties propertiesOther)
        {
            if (properties == null)
                return propertiesOther != null ? int.MinValue : 0;

            if (propertiesOther == null)
                return int.MaxValue;

            return CompareNormal(properties, propertiesOther);
        }

        internal int CompareNormal(LogProperties properties, LogProperties propertiesOther)
        {
            string[] compareFields, compareFieldsOther;

            compareFields = properties.GetValuesToCompare(CompareOptions);
            compareFieldsOther = propertiesOther.GetValuesToCompare(CompareOptions);

            int compareValue = -1;
            bool hasAtLeastOneNonEmptyPair = false;

            //Empty arrays, or field data is very unlikely - handle it anyways just in case
            if (compareFields.Length > 0)
            {
                //Attempt to find equal strings belonging to both arrays. Any other string comparison values will also be recorded, the results of which
                //may be sporadic. Comparing more than one CompareOption at a time is not recommended
                foreach (string idField in compareFields)
                {
                    if (idField == string.Empty) continue; //Avoids comparing two empty fields

                    foreach (string idFieldOther in compareFieldsOther)
                    {
                        if (idFieldOther == string.Empty) //Comparing non-empty field to empty field
                        {
                            compareValue = 1;
                            continue;
                        }

                        hasAtLeastOneNonEmptyPair = true;
                        compareValue = base.Compare(idField, idFieldOther);

                        if (compareValue == 0) goto end;
                    }
                }
            }

            if (!hasAtLeastOneNonEmptyPair) //Arrays are either empty, or only contain empty strings - consider as equal
                compareValue = 0;
            end:
            return compareValue;
        }

        /// <summary>
        /// A special comparison case where one, or both <see cref="LogProperties"/> instances is a null value
        /// </summary>
        protected int CompareNullProperties(LogID id, LogID idOther)
        {
            //Consider the ExtEnum value as equivalent to any filename related field when properties are unavailable
            if (id.Properties == null && idOther.Properties == null)
                return base.Compare(id, idOther);

            if (id.Properties == null)
                return CompareHelper(idOther, id);

            return CompareHelper(id, idOther);
        }

        internal int CompareHelper(LogID id, LogID idNoProperties)
        {
            //Get fields from the instance with properties
            string[] compareStrings = id.Properties.GetValuesToCompare(CompareOptions);

            if (compareStrings.Length == 0)
                return base.Compare(id, idNoProperties); //valueHash will be compared

            //Iterate to try to find equal strings
            int compareValue = -1;
            foreach (string value in compareStrings)
            {
                if (compareValue == 0) break;
                if (value != string.Empty)
                    compareValue = base.Compare(value, id.Value);
            }
            return compareValue;
        }

        /// <inheritdoc/>
        public bool Equals(LogProperties properties, LogProperties propertiesOther)
        {
            if (properties == null)
                return properties == propertiesOther;

            if (propertiesOther == null)
                return false;

            return base.Equals(properties.ID, propertiesOther.ID);
        }

        /// <inheritdoc/>
        public int GetHashCode(LogProperties obj)
        {
            return obj != null ? obj.GetHashCode() : 0;
        }
    }
}
