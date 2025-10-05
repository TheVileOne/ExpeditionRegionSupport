using LogUtils.Helpers.Comparers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace LogUtils.Properties
{
    public class LogPropertyStringDictionary : StringDictionary
    {
        protected Hashtable Contents = new Hashtable(ComparerUtils.StringComparerIgnoreCase);
        protected LogPropertyKeySorter Sorter;

        public override string this[string key]
        {
            get
            {
                if (key == null)
                    throw new ArgumentNullException(nameof(key));

                return (string)Contents[key];
            }
            set
            {
                if (key == null)
                    throw new ArgumentNullException(nameof(key));

                Contents[key] = value;
            }
        }

        /// <inheritdoc/>
        public override int Count => Contents.Count;

        /// <inheritdoc/>
        public override bool IsSynchronized => Contents.IsSynchronized;

        /// <inheritdoc/>
        public override ICollection Keys => Contents.Keys;

        /// <inheritdoc/>
        public override object SyncRoot => Contents.SyncRoot;

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override void Add(string key, string value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            Contents.Add(key, value);
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            Contents.Clear();
        }

        /// <inheritdoc/>
        public override bool ContainsKey(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            return Contents.ContainsKey(key);
        }

        /// <inheritdoc/>
        public override bool ContainsValue(string value)
        {
            return Contents.ContainsValue(value);
        }

        /// <inheritdoc/>
        public override void CopyTo(Array array, int index)
        {
            Contents.CopyTo(array, index);
        }

        /// <inheritdoc/>
        public override IEnumerator GetEnumerator()
        {
            return Contents.GetEnumerator();
        }

        /// <inheritdoc/>
        public override void Remove(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            Contents.Remove(key);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return ToString(null, false);
        }

        public string ToString(bool sortFields)
        {
            return ToString(null, sortFields);
        }

        public string ToString(List<CommentEntry> comments, bool sortFields)
        {
            if (comments == null)
                comments = new List<CommentEntry>();

            StringBuilder sb = new StringBuilder();

            if (sortFields)
            {
                foreach (string dataField in Sorter.Sort())
                {
                    sb.AppendComments(dataField, comments);
                    sb.AppendPropertyString(dataField, (string)Contents[dataField]);
                }
            }
            else
            {
                IDictionaryEnumerator fieldEnumerator = (IDictionaryEnumerator)GetEnumerator();

                while (fieldEnumerator.MoveNext())
                {
                    string dataField = (string)fieldEnumerator.Key;
                    string value = (string)fieldEnumerator.Value;

                    sb.AppendComments(dataField, comments);
                    sb.AppendPropertyString(dataField, value);
                }
            }
            return sb.ToString();
        }
    }
}
