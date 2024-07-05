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

        public Dictionary<Type, List<IShareable>> DataCollection = new Dictionary<Type, List<IShareable>>();

        public SharedDataHandler()
        {
            Tag = "Shared Data";
        }

        public IShareable GetData(Type type, string tag)
        {
            RegisterType(type);
            return DataCollection[type].Find(data => data.CheckTag(tag));
        }

        public List<IShareable> GetDataSet(Type type, string tag)
        {
            RegisterType(type);
            return DataCollection[type].FindAll(data => data.CheckTag(tag));
        }

        public void RegisterData(IShareable data)
        {
            RegisterData(data.GetType(), data);
        }

        public void RegisterData(Type type, IShareable data)
        {
            RegisterType(type);

            var itemCollection = DataCollection[type];
            int itemIndex = itemCollection.FindIndex(d => d.CheckTag(tag));

            if (itemIndex != -1)
            {
                UtilityCore.BaseLogger.LogInfo($"Overwriting shared data with tag {data.Tag}");
                itemCollection[itemIndex] = data;
                return;
            }

            itemCollection.Add(data);
        }

        public void RegisterType(Type type)
        {
            DataCollection[type] ??= new List<IShareable>();
        }

        public IShareable GetOrAssign(IShareable data)
        {
            Type type = data.GetType();
            IShareable storedData = GetData(type, data.Tag);

            if (storedData == null)
            {
                RegisterData(type, data);
                storedData = data;
            }
            return storedData;
        }
    }

    public class LogEventData
    {
        //Stub class
    }

    public class SharedField<T> : IShareable
    {
        public bool ReadOnly;

        public string Tag { get; set; }
        public T Value { get; set; }

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
        public string Tag { get; }
        public bool CheckTag(string tag);
    }
}
