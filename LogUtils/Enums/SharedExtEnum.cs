using LogUtils.Helpers.Comparers;
using System;

namespace LogUtils.Enums
{
    public class SharedExtEnum<T> : ExtEnum<T>, IComparable, IComparable<T>, IEquatable<T>, IShareable, IExtEnumBase where T : SharedExtEnum<T>, IShareable
    {
        /// <inheritdoc cref="IExtEnumBase.Index"/>
        public override int Index
        {
            get
            {
                //TODO: Can we prove that managed index can never go out of sync, and write a test for that?
                if (!ReferenceEquals(ManagedReference, this))
                    return ManagedReference.Index;
                return base.Index;
            }
        }

        /// <summary>
        /// The underlying <see cref="ExtEnum{T}"/> value index (used during the registration process when values are being synced)
        /// </summary>
        public int BaseIndex => base.Index;

        /// <summary>
        /// A null-safe reference to the <see cref="SharedExtEnum{T}"/> shared instance that any mod can access
        /// </summary>
        public T ManagedReference;

        /// <inheritdoc/>
        public bool Registered => Index >= 0;

        /// <inheritdoc/>
        public virtual string Tag
        {
            get
            {
                if (RegistrationStage == RegistrationStatus.Completed && !ReferenceEquals(ManagedReference, this))
                    return ManagedReference.Tag;
                return Value;
            }
        }


        /// <inheritdoc/>
        public string Value
        {
            get => base.value;
            protected set => base.value = value;
        }

        /// <inheritdoc cref="Value"/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Naming convention of a dependency")]
        [Obsolete("This property is from the base class. Use Value instead.")]
        public new string value => base.value;

        private Action _registrationCallback;
        private RegistrationStatus _registrationStage = RegistrationStatus.Ready;
        /// <summary>
        /// The current stage in assigning a registered state to this instance
        /// </summary>
        protected virtual RegistrationStatus RegistrationStage => _registrationStage;

        public SharedExtEnum(string value, bool register = false) : base(value, false)
        {
            ManagedReference = (T)UtilityCore.DataHandler.GetOrAssign(this);

            _registrationCallback = handleRegistration;
            if (RegistrationStage == RegistrationStatus.Ready)
                CompleteRegistration();

            void handleRegistration()
            {
                if (register)
                {
                    Register();
                }
                else if (!ReferenceEquals(ManagedReference, this) && ManagedReference.Registered) //Propagate field values from registered reference
                {
                    Value = ManagedReference.Value;
                    valueHash = ManagedReference.valueHash;
                    index = ManagedReference.Index;
                }
            }
        }

        /// <summary>
        /// Retrieves an <see cref="ExtEnum{T}"/> instance at the specified index
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
        /// <exception cref="ArgumentException">The argument provided was not of a valid format</exception>
        /// <exception cref="ValueNotFoundException">A registered entry was not found with the given value</exception>
        public static T Parse(string value)
        {
            value = value?.Trim();

            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value cannot be null or empty.", nameof(value));

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
            value = value?.Trim();

            if (string.IsNullOrEmpty(value))
                return false;

            result = (T)Activator.CreateInstance(typeof(T), value, false);
            return result.Registered;
        }

        /// <summary>
        /// Completes the registration process
        /// </summary>
        protected virtual void CompleteRegistration()
        {
            if (RegistrationStage != RegistrationStatus.Ready)
            {
                UtilityLogger.LogWarning($"Registration stage is unexpected - EXPECTED: {RegistrationStatus.Ready} ACTUAL: {RegistrationStage}");
                return;
            }

            try
            {
                _registrationCallback?.Invoke();
                _registrationCallback = null;
            }
            finally
            {
                _registrationStage = RegistrationStatus.Completed;
            }
        }

        /// <inheritdoc/>
        public virtual void Register()
        {
            RegisterCore();
        }

        /// <inheritdoc/>
        public new virtual void Unregister()
        {
            UnregisterCore();
        }

        /// <inheritdoc cref="Register"/>
        protected void RegisterCore()
        {
            //The shared reference may already exist. Sync any value differences between the two references
            if (!ReferenceEquals(ManagedReference, this))
            {
                if (ManagedReference.Registered)
                {
                    Value = ManagedReference.Value;
                    valueHash = ManagedReference.valueHash;
                    index = ManagedReference.Index;
                }
                else //Registration status should propagate to managed reference
                {
                    ManagedReference.Value = Value;
                    ManagedReference.valueHash = valueHash;

                    //Register the managed reference and get the assigned index from it
                    ManagedReference.Register();

                    index = ManagedReference.Index;
                }
            }
            else if (!Registered)
            {
                values.AddEntry(Value);
                index = values.Count - 1;
            }
        }

        /// <inheritdoc cref="Unregister"/>
        protected void UnregisterCore()
        {
            base.Unregister();
        }

        /// <inheritdoc/>
        public virtual bool CheckTag(string tag)
        {
            return ComparerUtils.StringComparerIgnoreCase.Equals(Tag, tag);
        }

        /// <inheritdoc/>
        public int CompareTo(T value)
        {
            if (value == null)
                return int.MaxValue;

            return Index - value.Index;
        }

        /// <inheritdoc/>
        public new int CompareTo(object value)
        {
            if (value == null)
                return int.MaxValue;

            if (value is not T)
                throw new ArgumentException(string.Format("Object must be the same type as the extEnum. The type passed in was {0}; the extEnum type was {1}.", value.GetType(), enumType));

            return CompareTo((T)value);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type (utilizes the base value hashcode comparison to determine equality)
        /// </summary>
        public bool BaseEquals(T other)
        {
            return base.Equals(other);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type (utilizes a customized value hashcode comparison to determine equality)
        /// </summary>
        public new bool Equals(T other)
        {
            if (other == null)
                return false;

            return ExtEnumValueComparer<T>.CompareByHash(this, other) == 0;
        }

        //These operator overloads will affect the equality checks used in base ExtEnum comparisons. The hashing being different for LogIDs with multiple paths
        //is probably harmless, but more testing is needed to confirm this.
        /*
        public static bool operator ==(SharedExtEnum<T> value, T valueOther)
        {
            UtilityLogger.DebugLog("Fetching equality from custom operator");
            return CompareByHash(value, valueOther) == 0;
        }

        public static bool operator !=(SharedExtEnum<T> value, T valueOther)
        {
            UtilityLogger.DebugLog("Fetching equality from custom operator");
            return CompareByHash(value, valueOther) != 0;
        }
        */

        public static bool operator <(SharedExtEnum<T> left, T right)
        {
            return left?.CompareTo(right) < 0;
        }

        public static bool operator <=(SharedExtEnum<T> left, T right)
        {
            return left?.CompareTo(right) <= 0;
        }

        public static bool operator >(SharedExtEnum<T> left, T right)
        {
            return left?.CompareTo(right) > 0;
        }

        public static bool operator >=(SharedExtEnum<T> left, T right)
        {
            return left?.CompareTo(right) >= 0;
        }

        /// <summary>
        /// Represents the initialization state of the registration process
        /// </summary>
        protected enum RegistrationStatus
        {
            /// <summary>Registration can be completed anytime</summary>
            Ready,
            /// <summary>Registration is deferred until a child class signals it is ready to complete registration</summary>
            WaitingOnSignal,
            /// <summary>Registration has been completed</summary>
            Completed
        }
    }

    /// <summary>
    /// Represents an ExtEnum parsing error
    /// </summary>
    public class ValueNotFoundException(string message) : Exception(message);
}
