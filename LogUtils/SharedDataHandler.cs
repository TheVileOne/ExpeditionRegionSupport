using System;
using System.Collections.Generic;

namespace LogUtils
{
    /// <summary>
    /// This class maintains a list of data inside of a MonoBehavior for easy access from multiple mod sources
    /// </summary>
    public class SharedDataHandler : UtilityComponent
    {
        public LogEventData EventData;

        public List<IShareable> SharedData = new List<IShareable>();

        public SharedDataHandler()
        {
            Tag = "Shared Data";
        }

        public IShareable GetData(string tag)
        {
            return SharedData.Find(data => data.CheckTag(tag));
        }
    }

    public class LogEventData
    {
        //Stub class
    }

    public class SharedField<T> : IShareable
    {
        public bool ReadOnly;

        public string Tag;
        public T Value;

        public SharedField(string tag, T value)
        {
            Tag = tag;
            Value = value;
        }

        public bool CheckTag(string tag)
        {
            return string.Equals(Tag, tag, StringComparison.InvariantCultureIgnoreCase);
        }
    }

    public interface IShareable
    {
        public bool CheckTag(string tag);
    }
}
