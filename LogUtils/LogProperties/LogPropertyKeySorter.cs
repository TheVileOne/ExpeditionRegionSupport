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
            List<UnsortedEntry> unsorted = new List<UnsortedEntry>();

            UnsortedEntry found;
            bool checkUnsorted = false;

            while (fieldEnumerator.MoveNext())
            {
                string key = (string)fieldEnumerator.Current;

                //Give unsorted entries priority
                if (checkUnsorted)
                {
                    //Check unsorted entries until no more matches are found
                    while ((found = unsorted.Find(entry => entry.Index == sortIndex)) != default)
                    {
                        sortIndex++;
                        unsorted.Remove(found);
                        yield return found.Value;
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
                    unsorted.Add(new UnsortedEntry(actualIndex, key));
            }

            //Any skipped entries that remain after the loop finishes are handled here
            while (unsorted.Count > 0)
            {
                found = unsorted.Find(entry => entry.Index == sortIndex);

                if (found != null)
                {
                    unsorted.Remove(found);
                    yield return found.Value;
                }
                sortIndex++;
            }

            //The only values left to return will be unrecognized fields
            unrecognizedFields.Sort();
            foreach (string field in unrecognizedFields)
                yield return field;
        }

        private record struct UnsortedEntry(int Index, string Value);
    }
}
