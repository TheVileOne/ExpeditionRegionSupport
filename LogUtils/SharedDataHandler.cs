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

        public override string Tag => UtilityConsts.ComponentTags.SHARED_DATA;

        /// <summary>
        /// Finds the first IShareable with a given tag under the specified type
        /// </summary>
        /// <param name="type">The desired type to associate IShareables with (e.g. Shareable booleans can be registered under the boolean type</param>
        /// <param name="tag">The search tag</param>
        public IShareable Find(Type type, string tag)
        {
            RegisterType(type);
            return DataCollection[type].Find(data => data.CheckTag(tag));
        }

        /// <summary>
        /// Finds all IShareable instances with a given tag under the specified type
        /// </summary>
        /// <param name="type">The desired type to associate IShareables with (e.g. Shareable booleans can be registered under the boolean type</param>
        /// <param name="tag">The search tag</param>
        public List<IShareable> FindAll(Type type, string tag)
        {
            RegisterType(type);
            return DataCollection[type].FindAll(data => data.CheckTag(tag));
        }

        /// <summary>
        /// Assigns an IShareable to the data list of its own type
        /// </summary>
        /// <param name="data">Data to store under a registered type</param>
        public void RegisterData(IShareable data)
        {
            RegisterData(data.GetType(), data);
        }

        /// <summary>
        /// Assigns an IShareable to the data list of the specified type
        /// </summary>
        /// <param name="type">The desired type to associate IShareables with (e.g. Shareable booleans can be registered under the boolean type</param>
        /// <param name="data">Data to store under a registered type</param>
        public void RegisterData(Type type, IShareable data)
        {
            RegisterType(type);

            var itemCollection = DataCollection[type];

            if (itemCollection.Exists(d => d.CheckTag(tag)))
            {
                UtilityCore.BaseLogger.LogWarning($"Data already exists of type {type} with tag {data.Tag}. Overwriting on register is not allowed");
                return;
            }

            itemCollection.Add(data);
        }

        /// <summary>
        /// Assigns a list for data of the specified type
        /// </summary>
        /// <param name="type">The desired type to associate IShareables with (e.g. Shareable booleans can be registered under the boolean type</param>
        public void RegisterType(Type type)
        {
            DataCollection[type] ??= new List<IShareable>();
        }

        /// <summary>
        /// Assigns an IShareable to the data list of the specified type, existing data will be overwritten
        /// </summary>
        /// <param name="type">The desired type to associate IShareables with (e.g. Shareable booleans can be registered under the boolean type</param>
        /// <param name="data">Data to store under a registered type</param>
        public void ReplaceData(Type type, IShareable data)
        {
            RegisterType(type);

            var itemCollection = DataCollection[type];
            int itemIndex = itemCollection.FindIndex(d => d.CheckTag(tag));

            if (itemCollection.Exists(d => d.CheckTag(tag)))
            {
                UtilityCore.BaseLogger.LogWarning($"Replacing existing data of type {type} with tag {data.Tag}");

                itemCollection[itemIndex] = data;
                return;
            }

            itemCollection.Add(data);
        }

        /// <summary>
        /// Retrieves a registered SharedField instance with the specified tag, and optional initial value
        /// </summary>
        /// <typeparam name="T">The type of data that will be stored in the field</typeparam>
        /// <param name="tag">The search tag</param>
        /// <param name="initValue">A value to be optionally set on creation of the SharedField</param>
        public SharedField<T> GetField<T>(string tag, T initValue = default)
        {
            return GetField(typeof(T), tag, initValue);
        }

        /// <summary>
        /// Retrieves a registered SharedField instance with the specified tag, and optional initial value
        /// </summary>
        /// <typeparam name="T">The type of data that will be stored in the field</typeparam>
        /// <param name="tag">The search tag</param>
        /// <param name="type">The desired type to associate IShareables with</param>
        /// <param name="initValue">A value to be optionally set on creation of the SharedField</param>
        public SharedField<T> GetField<T>(Type type, string tag, T initValue = default)
        {
            var field = Find(type, tag) as SharedField<T>; //Check if a field has already been registered

            if (field == null)
            {
                field = new SharedField<T>(tag, initValue); //Create new shared field and register it
                RegisterData(type, field);
            }
            else if (EqualityComparer<T>.Default.Equals(field.Value))
            {
                //We do not want to modify existing field data, so only do so when it already the default. This logic still may potentially create unwanted behavior
                field.Value = initValue;
            }

            return field;
        }

        /// <summary>
        /// Retrieves an IShareable with an existing data tag, or stores, and returns the given one if it does not yet exist
        /// This method uses the data type to retrieve the data
        /// </summary>
        /// <param name="data">The data to check for, or retrieve</param>
        /// <returns>The associated shared data object</returns>
        public IShareable GetOrAssign(IShareable data)
        {
            Type type = data.GetType();
            IShareable storedData = Find(type, data.Tag);

            if (storedData == null)
            {
                RegisterData(type, data);
                storedData = data;
            }
            return storedData;
        }

        public override Dictionary<string, object> GetFields()
        {
            Dictionary<string, object> fields = base.GetFields();

            fields[nameof(EventData)] = EventData;
            fields[nameof(DataCollection)] = DataCollection;
            return fields;
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
