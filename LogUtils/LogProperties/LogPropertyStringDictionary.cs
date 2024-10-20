using LogUtils.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace LogUtils.Properties
{
    public class LogPropertyStringDictionary : StringDictionary
    {
        protected Hashtable Contents = new Hashtable(EqualityComparer.StringComparerIgnoreCase);
        protected LogPropertyKeySorter Sorter;

        public override string this[string key]
        {
            get
            {
                if (key == null)
                    throw new ArgumentNullException("key");

                return (string)Contents[key];
            }
            set
            {
                if (key == null)
                    throw new ArgumentNullException("key");

                Contents[key] = value;
            }
        }

        public override int Count => Contents.Count;

        public override bool IsSynchronized => Contents.IsSynchronized;

        public override ICollection Keys => Contents.Keys;

        public override object SyncRoot => Contents.SyncRoot;

        public override ICollection Values => Contents.Values;

        public LogPropertyStringDictionary()
        {
            Sorter = new LogPropertyKeySorter(this);
        }

        /// <summary>
        /// Creates and adds a DictionaryEntry from a formatted property string
        /// </summary>
        /// <param name="propertyString">A string with key, and a value separated by ':'</param>
        public void Add(string propertyString)
        {
            if (propertyString == null)
                throw new ArgumentNullException(nameof(propertyString));

            string[] propertyData = propertyString.Split(':');

            if (propertyData.Length > 1)
            {
                string key = propertyData[0];
                string value = propertyData[1];

                if (propertyData.Length > 2)
                    value = propertyString.Substring(propertyString.IndexOf(':') + 1);
                this[key] = value;
            }
            else
            {
                string key = propertyData[0];
                this[key] = null;
            }
        }

        public override void Add(string key, string value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            Contents.Add(key, value);
        }

        public override void Clear()
        {
            Contents.Clear();
        }

        public override bool ContainsKey(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            return Contents.ContainsKey(key);
        }

        public override bool ContainsValue(string value)
        {
            return Contents.ContainsValue(value);
        }

        public override void CopyTo(Array array, int index)
        {
            Contents.CopyTo(array, index);
        }

        public override IEnumerator GetEnumerator()
        {
            return Contents.GetEnumerator();
        }

        public override void Remove(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            Contents.Remove(key);
        }
    }
}
