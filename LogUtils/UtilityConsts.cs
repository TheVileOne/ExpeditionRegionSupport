using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogUtils
{
    public static class UtilityConsts
    {
        public static class DataFields
        {
            public const int EXPECTED_FIELD_COUNT = 11;

            public const string LOGID = "logid";
            public const string FILENAME = "filename";
            public const string ALTFILENAME = "altfilename";
            public const string TAGS = "tags";
            public const string VERSION = "version";
            public const string PATH = "path";
            public const string ORIGINAL_PATH = "origpath";
            public const string LAST_KNOWN_PATH = "lastknownpath";
            public const string CUSTOM = "custom";

            public static class Rules
            {
                public const string HEADER = "logrules";
                public const string SHOW_LINE_COUNT = "showlinecount";
                public const string SHOW_CATEGORIES = "showcategories";
            }
        }

        public static class ComponentTags
        {
            public const string PROPERTY_DATA = "LogProperties";
            public const string SHARED_DATA = "SharedData";
        }
    }
}
