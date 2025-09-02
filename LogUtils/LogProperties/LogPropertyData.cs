using System.Collections;
using System.Collections.Generic;
using DataFields = LogUtils.UtilityConsts.DataFields;

namespace LogUtils.Properties
{
    public class LogPropertyData
    {
        public LogPropertyStringDictionary Fields;

        /// <summary>
        /// A subset of the Fields dictionary containing field data not recognized by the utility. This will include all custom field data
        /// </summary>
        public LogPropertyStringDictionary UnrecognizedFields;

        public List<CommentEntry> Comments;

        public LogPropertyDataProcessor Processor;

        /// <summary>
        /// Stores a flag describing whether the field state read from file matches the expected field order
        /// </summary>
        internal bool FieldOrderMismatch;

        public LogPropertyData(LogPropertyStringDictionary dataFields, List<CommentEntry> comments, bool fieldOrderMismatch = false)
        {
            FieldOrderMismatch = fieldOrderMismatch; //Must be set before unrecognized fields are checked

            Comments = comments;
            Fields = dataFields;
            UnrecognizedFields = GetUnrecognizedFields();
            Processor = new LogPropertyDataProcessor(this);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return LogProperties.CreateIDHash(GetID(), LogProperties.GetContainingPath(Fields[DataFields.ORIGINAL_PATH]));
        }

        public string GetID()
        {
            return Fields[DataFields.LOGID] ?? Fields[DataFields.FILENAME];
        }

        internal LogPropertyStringDictionary GetUnrecognizedFields()
        {
            var unrecognizedFields = new LogPropertyStringDictionary();

            int unknownFieldTotal = getFieldCheckTotal();

            if (unknownFieldTotal > 0)
            {
                //Handle unrecognized, and custom fields by storing them in a list that other mods will be able to access
                IDictionaryEnumerator fieldEnumerator = (IDictionaryEnumerator)Fields.GetEnumerator();
                while (unknownFieldTotal > 0)
                {
                    if (!fieldEnumerator.MoveNext())
                    {
                        UtilityLogger.LogWarning("Unexpected end of field enumeration");
                        break;
                    }

                    string fieldName = (string)fieldEnumerator.Key;
                    string fieldValue = (string)fieldEnumerator.Value;

                    if (!DataFields.IsRecognizedField(fieldName))
                    {
                        if (!fieldName.Equals(DataFields.CUSTOM)) //This header does not need to be stored
                            unrecognizedFields[fieldName] = fieldValue;

                        unknownFieldTotal--;
                        continue;
                    }

                    if (FieldOrderMismatch) //We have to check all fields when true
                        unknownFieldTotal--;
                }
            }

            int getFieldCheckTotal()
            {

                if (FieldOrderMismatch) //We can't know how many unrecognized fields there are. We must check every field.
                    return Fields.Count;
                return Fields.Count - DataFields.EXPECTED_FIELD_COUNT;
            }

            return unrecognizedFields;
        }

        public string GetWriteString(bool useComments)
        {
            return Fields.ToString(useComments ? Comments : null, true);
        }

        public void ProcessFields()
        {
            Processor.Process();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return GetWriteString(false);
        }
    }
}
