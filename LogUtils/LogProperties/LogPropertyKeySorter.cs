using LogUtils.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LogUtils.Properties
{
    public class LogPropertyKeySorter
    {
        protected LogPropertyStringDictionary Dictionary;

        public LogPropertyKeySorter(LogPropertyStringDictionary dictionary)
        {
            Dictionary = dictionary;
        }

        public IEnumerable<string> Sort()
        {
            if (Dictionary.Count == 0)
                yield break;

            if (Dictionary.Count == 1)
            {
                yield return Dictionary.Values.Cast<string>().First();
                yield break;
            }

            int sortIndex = 0; //The place in the OrderedArray
            string[] sortOrder = UtilityConsts.DataFields.OrderedFields; //The expected order of the fields

            IDictionaryEnumerator fieldEnumerator = (IDictionaryEnumerator)Dictionary.GetEnumerator();

            List<string> unrecognizedFields = new List<string>();
            List<int> unsorted = new List<int>();

            bool checkUnsorted = false;

            while (fieldEnumerator.MoveNext())
            {
                string key = (string)fieldEnumerator.Key;

                //Give unsorted entries priority
                if (checkUnsorted)
                {
                    //Check unsorted entries until no more matches are found
                    while (unsorted.Contains(sortIndex))
                    {
                        unsorted.Remove(sortIndex);
                        yield return sortOrder[sortIndex];

                        sortIndex++;
                    }
                    //Yield will break out of loop to here
                    checkUnsorted = false;
                }

                if (sortIndex == sortOrder.Length)
                {
                    //All expected fields have been processed - and now we are dealing with the remainder
                    unrecognizedFields.Add(key);
                    continue;
                }

                //Evaluate key against value at current sort index
                if (EqualityComparer.StringComparerIgnoreCase.Equals(key, sortOrder[sortIndex]))
                {
                    sortIndex++;
                    checkUnsorted = true;
                    yield return key;
                    continue;
                }

                /*
                 * We know the order has been interrupted somehow either by a missing field, or an unrecognized field
                 * Check if this key is part of the ordered array, or an unrecognized field
                 */
                int actualIndex = Array.FindIndex(sortOrder, sortIndex, k => EqualityComparer.StringComparerIgnoreCase.Equals(k, key));

                if (actualIndex == -1)
                    unrecognizedFields.Add(key);
                else //For some reason we have skipped over some entries in the ordered array
                    unsorted.Add(actualIndex);
            }

            //There may be entries that were not processed in the loop - handle those entries here 
            while (unsorted.Count > 0)
            {
                if (unsorted.Contains(sortIndex))
                {
                    unsorted.Remove(sortIndex);
                    yield return sortOrder[sortIndex];
                }
                sortIndex++;
            }

            //The only values left to return will be unrecognized fields
            unrecognizedFields.Sort();
            foreach (string field in unrecognizedFields)
                yield return field;
        }
    }
}
