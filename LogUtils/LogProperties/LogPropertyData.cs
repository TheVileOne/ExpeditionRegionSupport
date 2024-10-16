﻿using System.Collections;
using System.Collections.Specialized;
using DataFields = LogUtils.UtilityConsts.DataFields;

namespace LogUtils.Properties
{
    public class LogPropertyData
    {
        public StringDictionary Fields;

        /// <summary>
        /// A subset of the Fields dictionary containing field data not recognized by the utility. This will include all custom field data
        /// </summary>
        public StringDictionary UnrecognizedFields;

        public LogPropertyDataProcessor Processor;

        public LogPropertyData(StringDictionary dataFields)
        {
            Fields = dataFields;
            UnrecognizedFields = GetUnrecognizedFields();
            Processor = new LogPropertyDataProcessor(this);
        }

        internal StringDictionary GetUnrecognizedFields()
        {
            var unrecognizedFields = new StringDictionary();

            int unknownFieldTotal = Fields.Count - DataFields.EXPECTED_FIELD_COUNT;

            if (unknownFieldTotal > 0)
            {
                //Handle unrecognized, and custom fields by storing them in a list that other mods will be able to access
                IDictionaryEnumerator fieldEnumerator = (IDictionaryEnumerator)Fields.GetEnumerator();

                while (unknownFieldTotal > 0)
                {
                    fieldEnumerator.MoveNext();

                    DictionaryEntry fieldEntry = fieldEnumerator.Key switch
                    {
                        DataFields.LOGID => default,
                        DataFields.FILENAME => default,
                        DataFields.ALTFILENAME => default,
                        DataFields.TAGS => default,
                        DataFields.VERSION => default,
                        DataFields.PATH => default,
                        DataFields.ORIGINAL_PATH => default,
                        DataFields.LAST_KNOWN_PATH => default,
                        DataFields.Intro.MESSAGE => default,
                        DataFields.Intro.TIMESTAMP => default,
                        DataFields.Outro.MESSAGE => default,
                        DataFields.Outro.TIMESTAMP => default,
                        DataFields.LOGS_FOLDER_AWARE => default,
                        DataFields.LOGS_FOLDER_ELIGIBLE => default,
                        DataFields.SHOW_LOGS_AWARE => default,
                        DataFields.Rules.HEADER => default,
                        DataFields.Rules.SHOW_LINE_COUNT => default,
                        DataFields.Rules.SHOW_CATEGORIES => default,
                        _ => fieldEnumerator.Entry
                    };

                    if (!fieldEntry.Equals(default(DictionaryEntry)))
                    {
                        if (!fieldEntry.Key.Equals(DataFields.CUSTOM)) //This header does not need to be stored
                            UnrecognizedFields[(string)fieldEntry.Key] = (string)fieldEntry.Value;

                        unknownFieldTotal--;
                    }
                }
            }
            return unrecognizedFields;
        }

        public void ProcessFields()
        {
            Processor.Process();
        }
    }
}
