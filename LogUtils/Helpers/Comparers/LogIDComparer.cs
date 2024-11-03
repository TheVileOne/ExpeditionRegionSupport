using LogUtils.Enums;
using LogUtils.Properties;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogUtils.Helpers.Comparers
{
    public class LogIDComparer : ExtEnumValueComparer<LogID>, IComparer<LogID>, IEqualityComparer<LogID>
    {
        protected CompareOptions CompareOptions;

        public LogIDComparer(CompareOptions compareOptions) : this(compareOptions, StringComparison.InvariantCultureIgnoreCase)
        {
        }

        public LogIDComparer(CompareOptions compareOptions, StringComparison comparisonOption) : base(comparisonOption)
        {
            CompareOptions = compareOptions;
        }

        public override int Compare(LogID id, LogID idOther)
        {
            if (id == null)
                return idOther != null ? int.MinValue : 0;

            if (idOther == null)
                return int.MaxValue;

            //Properties field may be null when comparing against ComparisonLogID instances
            if (id.Properties == null || idOther.Properties == null)
                return CompareNullProperties(id, idOther);

            string[] compareFields, compareFieldsOther;

            compareFields = id.Properties.GetFilenamesToCompare(CompareOptions);
            compareFieldsOther = idOther.Properties.GetFilenamesToCompare(CompareOptions);

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
            //Filenames are equal, but filepaths may not be. Compare the paths too
            if (compareValue == 0)
                compareValue = ComparerUtils.PathComparer.Compare(id.Properties.CurrentFolderPath, idOther.Properties.CurrentFolderPath);

            return compareValue;
        }

        /// <summary>
        /// Compare LogID instances when one, or both instances has a null properties field
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
            string[] compareStrings = id.Properties.GetFilenamesToCompare(CompareOptions);

            if (compareStrings.Length == 0)
                return base.Compare(id, idNoProperties); //valueHash will be compared

            //Iterate to try to find equal strings
            int compareValue = -1;
            foreach (string value in compareStrings)
            {
                if (compareValue == 0) break;
                if (value != string.Empty)
                    compareValue = base.Compare(value, id.value);
            }
            return compareValue;
        }
    }
}
