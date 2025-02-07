using LogUtils.Diagnostics;
using System;

namespace LogUtils.Enums
{
    public class SharedExtEnum<T> : ExtEnum<T>, IComparable, IComparable<T>, IShareable where T : SharedExtEnum<T>, IShareable
    {
        public override int Index
        {
            get
            {
                if (!ReferenceEquals(ManagedReference, this))
                    return ManagedReference.Index;
                return index;
            }
        }

        /// <summary>
        /// A null-safe reference to the SharedExtEnum that any mod can access
        /// </summary>
        public T ManagedReference;
        public bool Registered => Index >= 0;
        public string Tag
        {
            get
            {
                if (!ReferenceEquals(ManagedReference, this))
                    return ManagedReference?.Tag ?? value; //Can be null here when it is accessed through the constructor
                return value;
            }
        }

        /// <summary>
        /// An identifying string assigned to each ExtEnum
        /// </summary>
        public new string value
        {
            get => base.value;
            protected set => base.@value = value;
        }

        public SharedExtEnum(string value, bool register = false) : base(value, false)
        {
            ManagedReference = (T)UtilityCore.DataHandler.GetOrAssign(this);

            Assert.That(ManagedReference).IsNotNull();

            if (register)
            {
                Register();
            }
            else if (!ReferenceEquals(ManagedReference, this) && ManagedReference.Registered) //Propagate field values from registered reference
            {
                base.value = ManagedReference.value;
                valueHash = ManagedReference.valueHash;
                index = ManagedReference.Index;
            }
        }

        /// <summary>
        /// Retrieves an ExtEnum instance at the specified index
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The index provided is outside the bounds of the collection</exception>
        public static T EntryAt(int index)
        {
            Type enumType = typeof(T);
            string[] enumNames = GetNames(enumType);

            if (index < 0 || index >= enumNames.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            return (T)Activator.CreateInstance(enumType, enumNames[index], false);
        }

        /// <summary>
        /// A static means of finding a registered instance, or creating a new instance if there are no registered instances
        /// </summary>
        /// <param name="value">Case insensitive value to compare with</param>
        /// <param name="result">The instance created from the provided value</param>
        /// <exception cref="ArgumentException">The argument provided was not of a valid format</exception>
        /// <exception cref="ValueNotFoundException">A registered entry was not found with the given value</exception>
        public static T Parse(string value)
        {
            value = value.Trim();

            if (value == string.Empty)
                throw new ArgumentException("An empty string is not considered a valid value.");

            T result = (T)Activator.CreateInstance(typeof(T), value, false);
            if (!result.Registered)
                throw new ValueNotFoundException(string.Format("An ExtEnum of the type {0} does not exist with the value {1}.", typeof(T), value));
            return result;
        }

        /// <summary>
        /// A static means of finding a registered instance
        /// </summary>
        /// <param name="value">Case insensitive value to compare with</param>
        /// <param name="result">The instance created from the provided value</param>
        /// <returns>Returns whether a registered instance was found</returns>
        public static bool TryParse(string value, out T result)
        {
            result = null;
            value = value.Trim();

            if (value == string.Empty)
                return false;

            result = (T)Activator.CreateInstance(typeof(T), value, false);
            return result.Registered;
        }

        public virtual void Register()
        {
            //The shared reference may already exist. Sync any value differences between the two references
            if (!ReferenceEquals(ManagedReference, this))
            {
                if (ManagedReference.Registered)
                {
                    value = ManagedReference.value;
                    valueHash = ManagedReference.valueHash;
                    index = ManagedReference.Index;
                }
                else //Registration status should propagate to managed reference
                {
                    ManagedReference.value = value;
                    ManagedReference.valueHash = valueHash;

                    //Register the managed reference and get the assigned index from it
                    ManagedReference.Register();

                    index = ManagedReference.Index;
                }
            }
            else if (!Registered)
            {
                values.AddEntry(value);
                index = values.Count - 1;
            }
        }

        public virtual bool CheckTag(string tag)
        {
            return string.Equals(value, tag, StringComparison.InvariantCultureIgnoreCase);
        }

        public int CompareTo(T value)
        {
            if (value == null)
                return int.MaxValue;

            return Index - value.Index;
        }

        public new int CompareTo(object value)
        {
            if (value == null)
                return int.MaxValue;

            if (value is not T)
                throw new ArgumentException(string.Format("Object must be the same type as the extEnum. The type passed in was {0}; the extEnum type was {1}.", value.GetType(), enumType));

            return CompareTo((T)value);
        }
    }

    public class ValueNotFoundException : Exception
    {
        public ValueNotFoundException(string message) : base(message)
        {
        }
    }
}
